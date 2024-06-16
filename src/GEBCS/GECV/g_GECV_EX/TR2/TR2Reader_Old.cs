using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.TR2
{
    public partial class TR2Reader
    {

        public static TR2Version ThinkTR2Version(byte[] tr2_file_data)
        {
            using (MemoryStream ms = new MemoryStream(tr2_file_data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {


                    int file_header = br.ReadInt32();

                    

                    if (file_header != TR2_HEADER)
                    {

                        throw new FileLoadException($"This is Not TR2 File!");

                    }


                    
                    int file_header_magic = br.ReadInt32();
                    Console.WriteLine($"Think Header Version:{file_header_magic}");
                    int year = GetYearFromHeader(file_header_magic);

                    if(year == 2015)
                    {
                        return TR2Version.PC;
                    }else if(year == 1999 || year == 2000 || year == 2010)
                    {
                        return TR2Version.SONY;
                    }
                    else
                    {
                        throw new InvalidDataException($"{file_header_magic}({file_header_magic.ToString("X")})({year}) Is Not Any Known Data.");
                    }


                    //switch (file_header_magic)
                    //{
                    //    case TR2Reader.NEW_VERSION_HEADER:
                    //        Console.WriteLine($"{file_header_magic}={TR2Reader.NEW_VERSION_HEADER} So This is New (PC) Verison.");
                    //        return TR2Version.PC;
                    //    case TR2Reader.OLD_VERSION_HEADER:
                    //        Console.WriteLine($"{file_header_magic}={TR2Reader.OLD_VERSION_HEADER} So This is Old (SONY_A) Verison.");
                    //        return TR2Version.SONY_A;
                    //    case TR2Reader.OLD_OLD_VERSION_HEADER:
                    //        Console.WriteLine($"{file_header_magic}={TR2Reader.OLD_OLD_VERSION_HEADER} So This is Old Old (SONY_B) Verison.");
                    //        return TR2Version.SONY_B;
                    //    default:
                    //        throw new InvalidDataException($"{file_header_magic}({file_header_magic.ToString("X")}) Is Not Any Known Data.");
                    //}

                    long offset = br.BaseStream.Position;


                    string table_name = StreamUtils.readNullterminated(br);


                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                    br.BaseStream.Seek(48, SeekOrigin.Current);


                    int table_column_infromation_offset = br.ReadInt32();

                    int table_column_infromation_count = br.ReadInt32();

                    var table_column_infromation = new TR2ColumnInformation[table_column_infromation_count];


                    br.BaseStream.Seek(table_column_infromation_offset, SeekOrigin.Begin);

                    for (int i = 0; i < table_column_infromation.Length; i++)
                    {

                        var data = new TR2ColumnInformation();

                        data.id = br.ReadInt32();
                        data.offset = br.ReadInt32();
                        data.magic = br.ReadInt32();
                        data.csize = br.ReadInt32();
                        data.usize = br.ReadInt32();

                        Console.WriteLine($"TS2 DATA INFORMATION:\nID:{data.id}\noffset:{data.offset}\nmagic:{data.magic}\ncsize:{data.csize}\nusize:{data.usize}");

                        offset = br.BaseStream.Position;

                        br.BaseStream.Seek(data.offset, SeekOrigin.Begin);

                        data.column_data = new TR2ColumnData();

                        data.column_data.bin_data = br.ReadBytes(data.csize);

                        table_column_infromation[i] = data;

                        br.BaseStream.Seek(offset, SeekOrigin.Begin);

                    }


                    var first_bin = table_column_infromation[0];

                    int header_length = first_bin.offset;

                    Console.WriteLine($"Guess header offset is :{header_length.ToString("X")},current poisition:{br.BaseStream.Position.ToString("X")}.");
                    Console.WriteLine($"{br.BaseStream.Position}+4={br.BaseStream.Position + 4} > {header_length}?");
                    if (br.BaseStream.Position + 4 <= header_length)
                    {
                        int count = br.ReadInt32();
                        if (count > 0)
                        {
                            Console.WriteLine($"Count:{count},So this is PC Version.");
                            return TR2Version.PC;
                        }
                        else
                        {
                            Console.WriteLine($"Count:{count},So this is Sony Version.");
                            return TR2Version.SONY;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{br.BaseStream.Position}+4={br.BaseStream.Position+4} > {header_length},no count version (SONY_A).");
                        return TR2Version.SONY;
                    }


                }


            }





        }

        public static short GetYearFromHeader(int magic_header)
        {
            int value = magic_header; //文心一言写的，面向字符串编码，跟我没关系。
            string hexString = value.ToString("X8"); // 将int转换为至少8位的十六进制字符串，不足部分用0填充  
            string desiredSubstring = hexString.Substring(0, 4); // 截取前四个字符  
            //Console.WriteLine("Year:"+desiredSubstring); // 输出: 07DA

            short year = Convert.ToInt16(desiredSubstring,16);
            return year;
        }

        public void BuildColumnBinaryData_SonyA()
        {

            column_counter = new TR2ColumnCounter();

            Dictionary<int, List<int>> Tr2HeaderRebuilder = new Dictionary<int, List<int>>();



            for (int i = 0; i < this.table_column_infromation.Length; i++)
            {

                TR2ColumnData column_data = this.table_column_infromation[i].column_data;

                Console.WriteLine($"Binary Data Length:{column_data.bin_data.Length}.");
                string[] bin_data_hex = FileUtils.GetHexEditorStyleString(column_data.bin_data);

                foreach (var str in bin_data_hex)
                {

                    Console.WriteLine(str);

                }


                using (MemoryStream ms = new MemoryStream(column_data.bin_data))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        br.BaseStream.Seek(0, SeekOrigin.Begin);
                        column_data.column_name = StreamUtils.readNullterminated(br);
                        Console.WriteLine($"Column Name:{column_data.column_name}.");
                        br.BaseStream.Seek(48, SeekOrigin.Begin);
                        column_data.column_serial_left = br.ReadInt64();
                        column_data.column_serial_right = br.ReadInt64();
                        long offset = br.BaseStream.Position;
                        column_data.column_type = StreamUtils.readNullterminated(br);
                        Console.WriteLine($"Column Type:{column_data.column_type}.");

                        br.BaseStream.Seek(offset, SeekOrigin.Begin);
                        br.BaseStream.Seek(0x70, SeekOrigin.Begin);


                        column_data.data_70_73 = br.ReadInt32();
                        column_data.data_74 = br.ReadByte();
                        column_data.data_75 = br.ReadByte();
                        column_data.data_76_array_size = br.ReadByte();
                        column_data.data_77 = br.ReadByte();
                        column_data.data_78_7B = br.ReadInt32();
                        column_data.data_7C_7F_column_data_count = br.ReadInt32();



                        Console.WriteLine($"TS2 DATA COLUMN INFORMATION:\nname:{column_data.column_name}\nserial_left:{column_data.column_serial_left}\nserial_right:{column_data.column_serial_right}\n70-73:{column_data.data_70_73}\n74:{column_data.data_74}\n75:{column_data.data_75}\n76:{column_data.data_76_array_size}\n77:{column_data.data_77}\n78-7B:{column_data.data_78_7B}\n7C-7F:{column_data.data_7C_7F_column_data_count}");





                        if (TR2Reader.IsStringFormat(column_data.column_type) && column_data.data_76_array_size > 1)
                        {

                            if (column_data.data_7C_7F_column_data_count % column_data.data_76_array_size != 0)
                            {
                                throw new InvalidDataException($"Total Text Array Caculate Error:{column_data.data_7C_7F_column_data_count} % {column_data.data_76_array_size} != 0");
                            }
                            else
                            {
                                Console.WriteLine($"Text Array Total Size:{column_data.data_7C_7F_column_data_count / column_data.data_76_array_size}");
                            }

                            int calc_total = column_data.data_7C_7F_column_data_count / column_data.data_76_array_size;
                            column_data.column_data_list = new TR2ColumnDataList[calc_total];

                            long br_position = br.BaseStream.Position;
                            Tr2HeaderRebuilder.Add(i, new List<int>());
                            for (int ri = 0; ri < column_data.column_data_list.Length; ri++)
                            {


                                column_data.column_data_list[ri].column_data = new TR2ColumnDataArray[column_data.data_76_array_size];
                                Console.WriteLine($"{ri + 1} Data Every Text Array Have {column_data.column_data_list[ri].column_data.Length} Object.");

                                for (int rri = 0; rri < column_data.data_76_array_size; rri++)
                                {
                                    var data_arr_list_arr = new TR2ColumnDataArray();
                                    int data_arr_id = br.ReadInt32();
                                    int data_arr_offset = br.ReadInt32(); //SONY A

                                    int data_arr_length = br.ReadInt32();
                                    data_arr_list_arr.OldSonyDataArrayId = data_arr_id;
                                    data_arr_list_arr.OldSonyDataArrayCursor = data_arr_offset;
                                    data_arr_list_arr.OldSonyDataArrayLength = data_arr_length;
                                    Console.WriteLine($"{ri}-{rri}:TS2 OLD SONY_A DATA SPECIAL TEXT INFORMATION:\nid:{data_arr_id.ToString("X")}\noffset:{data_arr_offset.ToString("X")}\nlength:{data_arr_length.ToString("X")}");
                                    long br_position_rri = br.BaseStream.Position;
                                    br.BaseStream.Seek(data_arr_offset, SeekOrigin.Begin);
                                    dynamic xdata;
                                    switch (column_data.column_type)
                                    {
                                        case "ASCII":
                                            xdata = StreamUtils.readZeroterminated(br);
                                            data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(xdata);
                                            xdata = Encoding.ASCII.GetString(xdata);
                                            data_arr_list_arr.value_string_view = xdata.ToString();
                                            data_arr_list_arr.value = xdata;
                                            break;
                                        case "UTF-16LE": //NO USED
                                            xdata = StreamUtils.readWideDataterminated(br);
                                            data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(xdata);
                                            xdata = Encoding.Unicode.GetString(xdata);
                                            data_arr_list_arr.value_string_view = xdata.ToString();
                                            data_arr_list_arr.value = xdata;
                                            break;
                                        case "UTF-8":
                                            xdata = StreamUtils.readZeroterminated(br);
                                            data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(xdata);
                                            xdata = Encoding.UTF8.GetString(xdata);
                                            data_arr_list_arr.value_string_view = xdata.ToString();
                                            data_arr_list_arr.value = xdata;
                                            break;
                                        case "UTF-16":
                                            xdata = StreamUtils.readWideDataterminated(br);
                                            data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(xdata);
                                            xdata = Encoding.Unicode.GetString(xdata); //BIG?
                                            data_arr_list_arr.value_string_view = xdata.ToString();
                                            data_arr_list_arr.value = xdata;
                                            break;
                                        default:
                                            throw new InvalidCastException($"What is {column_data.column_type}? (Parsing Array Text)");
                                    }

                                    if (rri == 0)
                                    {
                                        Tr2HeaderRebuilder[i].Add(data_arr_id);
                                        Console.WriteLine($"Special Text Add First Id To Header Rebuilder:{data_arr_id}");
                                    }

                                    Console.WriteLine($"Object {rri} Read Data:{data_arr_list_arr.value_string_view}");
                                    column_data.column_data_list[ri].column_data[rri] = data_arr_list_arr;
                                    br.BaseStream.Seek(br_position_rri, SeekOrigin.Begin);
                                }
                            }



                            br.BaseStream.Seek(br_position, SeekOrigin.Begin);


                        }
                        else
                        {


                            column_data.column_data_list = new TR2ColumnDataList[column_data.data_7C_7F_column_data_count];
                            List<int> duplicate_check_list = new List<int>();
                            for (int si = 0; si < column_data.column_data_list.Length; si++)
                            {


                                if (!Tr2HeaderRebuilder.ContainsKey(i))
                                {
                                    Tr2HeaderRebuilder.Add(i, new List<int>());
                                    Console.WriteLine($"Register New Rebuilder Data:{i}");
                                }





                                int data_arr_id = br.ReadInt32();
                                int data_arr_offset = br.ReadInt32(); //SONY A
                                int data_arr_length = br.ReadInt32();


                                bool duplicate_header_check = false;

                                foreach (var header_check_item in Tr2HeaderRebuilder[i])
                                {
                                    if (header_check_item == data_arr_id)
                                    {
                                        duplicate_header_check = true;
                                        Console.WriteLine($"Duplicate Rebuilder Data:{data_arr_id}");
                                        break;
                                    }
                                }

                                if (!duplicate_header_check)
                                {
                                    Tr2HeaderRebuilder[i].Add(data_arr_id);
                                    Console.WriteLine($"Add Rebuilder Data:{data_arr_id}");
                                }




                                Console.WriteLine($"TS2 OLD SONY_A DATA LIST INFORMATION:\nid:{data_arr_id.ToString("X")}\noffset:{data_arr_offset.ToString("X")}\nlength:{data_arr_length.ToString("X")}");

                                column_data.column_data_list[si].OldSonyDataId = data_arr_id;




                                column_data.column_data_list[si].OldSonyDataCursor = data_arr_offset;
                                column_data.column_data_list[si].OldSonyDataLength = data_arr_length;

                                long br_position = br.BaseStream.Position;
                                Console.WriteLine($"Data Cursor:{data_arr_offset.ToString("X")}.");
                                if (data_arr_offset >= br.BaseStream.Length)
                                {
                                    var data_arr_list = new TR2ColumnDataList();
                                    data_arr_list.IsInVaildOffset = true;
                                    Console.WriteLine("Is Invaild Offset");
                                    column_data.column_data_list[si] = data_arr_list;
                                    continue;
                                }
                                else
                                {

                                    if (duplicate_check_list.Contains(data_arr_offset))
                                    {

                                        column_data.column_data_list[si].DulpicatedObjectOffset = data_arr_offset;
                                    }
                                    else
                                    {
                                        duplicate_check_list.Add(data_arr_offset);
                                    }

                                    if (column_data.column_data_list[si].OldSonyDataLength != 0)
                                    {
                                        switch (column_data.column_type)
                                        {
                                            case "ASCII":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.data_76_array_size;
                                                break;
                                            case "UTF-16LE": //NO USED
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.data_76_array_size;
                                                break;
                                            case "UTF-8":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.data_76_array_size;
                                                break;
                                            case "UTF-16":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.data_76_array_size;
                                                break;
                                            case "INT8":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.column_data_list[si].OldSonyDataLength / 1;
                                                break;
                                            case "UINT8":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.column_data_list[si].OldSonyDataLength / 1;
                                                break;
                                            case "INT16":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.column_data_list[si].OldSonyDataLength / 2;
                                                break;
                                            case "UINT16":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.column_data_list[si].OldSonyDataLength / 2;
                                                break;
                                            case "INT32":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.column_data_list[si].OldSonyDataLength / 4;
                                                break;
                                            case "UINT32":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.column_data_list[si].OldSonyDataLength / 4;
                                                break;
                                            case "FLOAT32":
                                                column_data.column_data_list[si].OldSonyVaildCount = column_data.column_data_list[si].OldSonyDataLength / 4;
                                                break;
                                            default:
                                                throw new InvalidCastException($"What is {column_data.column_type}?");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Warning:{si}.Old Sony Length == {column_data.column_data_list[si].OldSonyDataLength}.");
                                        column_data.column_data_list[si].OldSonyVaildCount = column_data.data_76_array_size;
                                    }



                                    Console.WriteLine($"Array:{column_data.data_76_array_size},Length Vaild Count:{column_data.column_data_list[si].OldSonyVaildCount}");

                                    column_data.column_data_list[si].column_data = new TR2ColumnDataArray[column_data.data_76_array_size];
                                    Console.WriteLine($"{si + 1} Data Every Cell Have {column_data.column_data_list[si].OldSonyVaildCount} Object.");
                                    br.BaseStream.Seek(data_arr_offset, SeekOrigin.Begin);

                                    for (int ssi = 0; ssi < column_data.column_data_list[si].column_data.Length; ssi++)
                                    {
                                        var data_arr_list_arr = new TR2ColumnDataArray();



                                        Console.WriteLine($"Object {ssi} Data Cursor:{br.BaseStream.Position.ToString("X")}.");
                                        dynamic xdata;

                                        if (ssi >= column_data.column_data_list[si].OldSonyVaildCount)
                                        {
                                            data_arr_list_arr.IsInVaildArrayOffset = true;
                                            Console.WriteLine($"Over Length Data: Index:{ssi},Length:{column_data.column_data_list[si].OldSonyVaildCount}.");
                                        }
                                        else
                                        {


                                            switch (column_data.column_type)
                                            {
                                                case "ASCII":
                                                    xdata = StreamUtils.readZeroterminated(br);
                                                    data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(xdata);
                                                    xdata = Encoding.ASCII.GetString(xdata);
                                                    data_arr_list_arr.value_string_view = xdata.ToString();
                                                    data_arr_list_arr.value = xdata;
                                                    break;
                                                case "UTF-16LE": //NO USED
                                                    xdata = StreamUtils.readWideDataterminated(br);
                                                    data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(xdata);
                                                    xdata = Encoding.Unicode.GetString(xdata);
                                                    data_arr_list_arr.value_string_view = xdata.ToString();
                                                    data_arr_list_arr.value = xdata;
                                                    break;
                                                case "UTF-8":
                                                    xdata = StreamUtils.readZeroterminated(br);
                                                    data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(xdata);
                                                    xdata = Encoding.UTF8.GetString(xdata);
                                                    data_arr_list_arr.value_string_view = xdata.ToString();
                                                    data_arr_list_arr.value = xdata;
                                                    break;
                                                case "UTF-16":
                                                    xdata = StreamUtils.readWideDataterminated(br);
                                                    data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(xdata);
                                                    xdata = Encoding.Unicode.GetString(xdata); //BIG?
                                                    data_arr_list_arr.value_string_view = xdata.ToString();
                                                    data_arr_list_arr.value = xdata;
                                                    break;
                                                case "INT8":
                                                    if (br.BaseStream.Position + 1 > br.BaseStream.Length)
                                                    {
                                                        data_arr_list_arr.IsInVaildArrayOffset = true;
                                                    }
                                                    else
                                                    {
                                                        sbyte tdata = br.ReadSByte();
                                                        data_arr_list_arr.value_hex_view = tdata.ToString("X2");
                                                        xdata = tdata;
                                                        data_arr_list_arr.value_string_view = xdata.ToString();
                                                        data_arr_list_arr.value = xdata;
                                                    }
                                                    break;
                                                case "UINT8":
                                                    if (br.BaseStream.Position + 1 > br.BaseStream.Length)
                                                    {
                                                        data_arr_list_arr.IsInVaildArrayOffset = true;
                                                    }
                                                    else
                                                    {
                                                        byte t2data = br.ReadByte();
                                                        data_arr_list_arr.value_hex_view = t2data.ToString("X2");
                                                        xdata = t2data;
                                                        data_arr_list_arr.value_string_view = xdata.ToString();
                                                        data_arr_list_arr.value = xdata;
                                                    }
                                                    break;
                                                case "INT16":
                                                    if (br.BaseStream.Position + 2 > br.BaseStream.Length)
                                                    {
                                                        data_arr_list_arr.IsInVaildArrayOffset = true;
                                                    }
                                                    else
                                                    {
                                                        Int16 t3data = br.ReadInt16();
                                                        data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(t3data));
                                                        xdata = t3data;
                                                        data_arr_list_arr.value_string_view = xdata.ToString();
                                                        data_arr_list_arr.value = xdata;
                                                    }
                                                    break;
                                                case "UINT16":
                                                    if (br.BaseStream.Position + 2 > br.BaseStream.Length)
                                                    {
                                                        data_arr_list_arr.IsInVaildArrayOffset = true;
                                                    }
                                                    else
                                                    {
                                                        UInt16 t4data = br.ReadUInt16();
                                                        data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(t4data));
                                                        xdata = t4data;
                                                        data_arr_list_arr.value_string_view = xdata.ToString();
                                                        data_arr_list_arr.value = xdata;
                                                    }
                                                    break;
                                                case "INT32":
                                                    if (br.BaseStream.Position + 4 > br.BaseStream.Length)
                                                    {
                                                        data_arr_list_arr.IsInVaildArrayOffset = true;
                                                    }
                                                    else
                                                    {
                                                        Int32 t5data = br.ReadInt32();
                                                        data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(t5data));
                                                        xdata = t5data;
                                                        data_arr_list_arr.value_string_view = xdata.ToString();
                                                        data_arr_list_arr.value = xdata;
                                                    }
                                                    break;
                                                case "UINT32":
                                                    if (br.BaseStream.Position + 4 > br.BaseStream.Length)
                                                    {
                                                        data_arr_list_arr.IsInVaildArrayOffset = true;

                                                    }
                                                    else
                                                    {
                                                        UInt32 t6data = br.ReadUInt32();
                                                        data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(t6data));
                                                        xdata = t6data;
                                                        data_arr_list_arr.value_string_view = xdata.ToString();
                                                        data_arr_list_arr.value = xdata;
                                                    }
                                                    break;
                                                case "FLOAT32":
                                                    if (br.BaseStream.Position + 4 > br.BaseStream.Length)
                                                    {
                                                        data_arr_list_arr.IsInVaildArrayOffset = true;

                                                    }
                                                    else
                                                    {
                                                        Single t7data = br.ReadSingle();
                                                        data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(t7data));
                                                        xdata = t7data;
                                                        data_arr_list_arr.value_string_view = xdata.ToString();
                                                        data_arr_list_arr.value = xdata;
                                                    }
                                                    break;
                                                default:
                                                    throw new InvalidCastException($"What is {column_data.column_type}?");
                                            }

                                            //if (string.IsNullOrEmpty(data_arr_list_arr.value_hex_view))
                                            //{
                                            //    data_arr_list_arr.value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(xdata));
                                            //}



                                            Console.WriteLine($"Object {ssi} Read Data:{data_arr_list_arr.value_string_view}");
                                        }


                                        column_data.column_data_list[si].column_data[ssi] = data_arr_list_arr;


                                    }

                                    br.BaseStream.Seek(br_position, SeekOrigin.Begin);
                                }





                            }
                        }
                        column_data.last_mark = br.ReadInt32();
                        Console.WriteLine($"Old File last_mark:{column_data.last_mark.ToString("X")}");



                    }


                }
                this.table_column_infromation[i].column_data = column_data;

            }



            var first_header = Tr2HeaderRebuilder[0];

            Console.WriteLine($"Print TR2 Header Rebuilder Map:");
            foreach (var header in Tr2HeaderRebuilder)
            {
                Console.Write("{0,4}|", header.Key);

                foreach (var header_data in header.Value)
                {
                    Console.Write("{0,4},", header_data);
                }

                Console.WriteLine();
            }





            foreach (var header_checker_item in Tr2HeaderRebuilder)
            {

                if (header_checker_item.Key != 0)
                {

                    if (first_header.Count != header_checker_item.Value.Count)
                    {
                        throw new InvalidDataException($"0 Header Count {first_header.Count} != {header_checker_item.Key} Header Data Count {header_checker_item.Value.Count}");
                    }



                    for (int ci = 0; ci < first_header.Count; ci++)
                    {
                        if (first_header[ci] != header_checker_item.Value[ci])
                        {
                            throw new InvalidDataException($"Column:{ci} Wrong:0 Header Data {first_header[ci]} != {header_checker_item.Key} Header Data {header_checker_item.Value[ci]}");
                        }
                    }

                }
                else
                {
                    continue;
                }



            };

            column_counter.count = first_header.Count;
            column_counter.id = first_header.ToArray();


        }

    }
}
