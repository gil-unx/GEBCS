using GECV_EX.Utils;
using System.Text;
using System.Xml.Serialization;

namespace GECV_EX.TR2
{

    [XmlRoot]
    [Serializable]
    public partial class TR2Reader
    {
        [XmlIgnore]
        public static readonly int TR2_HEADER = 0x3272742E;

        [XmlAttribute]
        public TR2Version TR2_VERSION;

        [XmlAttribute]
        public int file_header;

        [XmlAttribute]
        public int file_header_magic; //0x07DF0002

        [XmlElement]
        public string table_name; //byte[48]

        [XmlIgnore]
        public int table_column_infromation_offset;

        [XmlIgnore]
        public int table_column_infromation_count;

        [XmlElement]
        public TR2ColumnCounter column_counter; //OLD TR2 NOT THIS

        [XmlArray]
        public TR2ColumnInformation[] table_column_infromation;

        //[XmlIgnore]
        //public const int OLD_VERSION_HEADER = 0x07CF0000;

        //[XmlIgnore]
        //public const int OLD_OLD_VERSION_HEADER = 0x07DA0000;

        //[XmlIgnore]
        //public const int NEW_VERSION_HEADER = 0x07DF0002;

        

        //zero16

        [XmlRoot]
        [Serializable]
        public struct TR2ColumnInformation
        {

            [XmlAttribute]
            public int id;
            [XmlIgnore]
            public int offset; //Running Data
            [XmlAttribute]
            public int magic;
            [XmlIgnore]
            public int csize;
            [XmlIgnore]
            public int usize;

            [XmlElement]
            public TR2ColumnData column_data;


            //public string GetTableNameForEditor()
            //{
            //    return $"{id} {column_data.column_name} {column_data.column_type}";
            //}

        }

        [XmlRoot]
        [Serializable]
        public struct TR2ColumnCounter
        {

            [XmlIgnore]
            public int count;
            [XmlArray]
            public int[] id;

        }

        [XmlRoot]
        [Serializable]
        public struct TR2ColumnData
        {

            [XmlIgnore]
            public byte[] bin_data;


            [XmlAttribute]
            public string column_name; //byte[48]

            [XmlAttribute]
            public Int64 column_serial_left; //8

            [XmlAttribute]
            public Int64 column_serial_right; //8

            [XmlAttribute]
            public string column_type; //byte[48]

            [XmlAttribute]
            public int data_70_73;


            [XmlAttribute]
            public byte data_74;

            [XmlAttribute]
            public byte data_75;

            [XmlAttribute]
            public byte data_76_array_size; //ARRAY

            [XmlAttribute]
            public byte data_77;

            [XmlAttribute]
            public int data_78_7B;

            [XmlAttribute]
            public int data_7C_7F_column_data_count;

            [XmlArray]
            public TR2ColumnDataList[] column_data_list;

            [XmlIgnore]
            public int last_mark;

        }
        //ZERO16

        [XmlRoot]
        [Serializable]
        public struct TR2ColumnDataList
        {

            [XmlArray]
            public TR2ColumnDataArray[] column_data;

            [XmlAttribute]
            public int OldSonyVaildCount;

            [XmlAttribute]
            public bool IsInVaildOffset;

            [XmlAttribute]
            public int DulpicatedObjectOffset;

            [XmlAttribute]
            public int OldSonyDataId;
            [XmlAttribute]
            public int OldSonyDataCursor;
            [XmlAttribute]
            public int OldSonyDataLength;



        }

        [XmlRoot]
        [Serializable]
        public struct TR2ColumnDataArray
        {

            [XmlAttribute]
            public bool IsInVaildArrayOffset;

            [XmlElement]
            public dynamic value;

            [XmlElement]
            public string value_hex_view;

            [XmlElement]
            public string value_string_view;


            [XmlAttribute]
            public int OldSonyDataArrayId;
            [XmlAttribute]
            public int OldSonyDataArrayCursor;
            [XmlAttribute]
            public int OldSonyDataArrayLength;



        }




        public TR2Reader(byte[] tr2_file_data,TR2Version tr2_version = TR2Version.PC)
        {
            this.TR2_VERSION = tr2_version;

            using (MemoryStream ms = new MemoryStream(tr2_file_data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {


                    file_header = br.ReadInt32();

                    if (file_header != TR2_HEADER)
                    {

                        throw new FileLoadException($"This is Not TR2 File!");

                    }

                    file_header_magic = br.ReadInt32();


                    long offset = br.BaseStream.Position;


                    this.table_name = StreamUtils.readNullterminated(br);


                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                    br.BaseStream.Seek(48, SeekOrigin.Current);


                    table_column_infromation_offset = br.ReadInt32();

                    table_column_infromation_count = br.ReadInt32();

                    table_column_infromation = new TR2ColumnInformation[table_column_infromation_count];


                    br.BaseStream.Seek(table_column_infromation_offset, SeekOrigin.Begin);


                    offset = ReadColumnInformtaion(br);

                    switch (this.TR2_VERSION)
                    {
                        case TR2Version.SONY:
                            BuildColumnBinaryData_SonyA();
                            break;
                        default:
                            ReadColumnCounter(br);
                            BuildColumnBinaryData();
                            break;
                    }

                    //if(this.TR2_VERSION == TR2Version.PC)
                    //{
                    //    offset = ReadColumnCounter(br);
                    //}

                    
                    //BuildColumnBinaryData();


                }


            }






        }

        private TR2Reader()
        {

        }

        public int GetColumnCounterIndexById(int id)
        {

            for (int i = 0; i < this.column_counter.id.Length; i++)
            {

                if (this.column_counter.id[i] == id)
                {
                    return i;
                }


            }

            throw new ArgumentException($"{id} Not In Column Counter!");
        }


        public bool SetDataByIdNameTypeArrayIndexAndDataIdWithParseStringData(int id, string name, string type, byte index, int data_id, string input_data)
        {

            data_id = GetColumnCounterIndexById(data_id);

            foreach (var i in this.table_column_infromation)
            {

                if (i.id == id && i.column_data.column_name.Equals(name) && i.column_data.column_type.Equals(type))
                {

                    //i.column_data.column_data_list[data_id].column_data[index].value_hex_view;


                    if (i.column_data.column_data_list[data_id].IsInVaildOffset || i.column_data.column_data_list[data_id].column_data[index].IsInVaildArrayOffset)
                    {
                        return true;
                    }



                    switch (type)
                    {
                        case "INT8":
                            var sb = Convert.ToSByte(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = sb;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = sb.ToString("X2");
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = sb.ToString();
                            break;
                        case "UINT8":
                            var b = Convert.ToByte(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = b;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = b.ToString("X2");
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = b.ToString();
                            break;
                        case "INT16":
                            var i16 = Convert.ToInt16(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = i16;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(i16));
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = i16.ToString();
                            break;
                        case "UINT16":
                            var ui16 = Convert.ToUInt16(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = ui16;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(ui16));
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = ui16.ToString();
                            break;
                        case "INT32":
                            var i32 = Convert.ToInt32(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = i32;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(i32));
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = i32.ToString();
                            break;
                        case "UINT32":
                            var ui32 = Convert.ToUInt32(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = ui32;
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = ui32.ToString();
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(ui32));
                            break;
                        case "FLOAT32":
                            var float32 = Convert.ToSingle(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = float32;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(BitConverter.GetBytes(float32));
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = float32.ToString();
                            break;
                        default:
                            throw new InvalidCastException($"What is {type}?");
                    }



                    return true;
                }

            }




            return false;
        }



        public bool SetDataByIdNameTypeArrayIndexAndDataIdWithParseBytes(int id, string name, string type, byte index, int data_id, byte[] input_data)
        {
            data_id = GetColumnCounterIndexById(data_id);

            foreach (var i in this.table_column_infromation)
            {

                if (i.id == id && i.column_data.column_name.Equals(name) && i.column_data.column_type.Equals(type))
                {

                    //i.column_data.column_data_list[data_id].column_data[index].value_hex_view;


                    if (i.column_data.column_data_list[data_id].IsInVaildOffset || i.column_data.column_data_list[data_id].column_data[index].IsInVaildArrayOffset)
                    {
                        return true;
                    }



                    switch (type)
                    {
                        case "ASCII":
                            var ascii = Encoding.ASCII.GetString(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = ascii;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = ascii;
                            break;
                        case "UTF-8":
                            var utf8 = Encoding.UTF8.GetString(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = utf8;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = utf8;
                            break;
                        case "UTF-16":
                            var utf16 = Encoding.Unicode.GetString(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = utf16;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = utf16;
                            break;
                        case "UTF-16LE":
                            var utf16le = Encoding.Unicode.GetString(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value = utf16le;
                            i.column_data.column_data_list[data_id].column_data[index].value_hex_view = FileUtils.GetByteArrayString(input_data);
                            i.column_data.column_data_list[data_id].column_data[index].value_string_view = utf16le;
                            break;
                        default:
                            throw new InvalidCastException($"What is {type}?");
                    }



                    return true;
                }

            }




            return false;
        }

        public long ReadColumnInformtaion(BinaryReader br)
        {

            for (int i = 0; i < table_column_infromation.Length; i++)
            {

                var data = new TR2ColumnInformation();

                data.id = br.ReadInt32();
                data.offset = br.ReadInt32();
                data.magic = br.ReadInt32();
                data.csize = br.ReadInt32();
                data.usize = br.ReadInt32();

                Console.WriteLine($"TS2 DATA INFORMATION:\nID:{data.id}\noffset:{data.offset}\nmagic:{data.magic}\ncsize:{data.csize}\nusize:{data.usize}");

                long offset = br.BaseStream.Position;

                br.BaseStream.Seek(data.offset, SeekOrigin.Begin);

                data.column_data = new TR2ColumnData();

                data.column_data.bin_data = br.ReadBytes(data.csize);

                table_column_infromation[i] = data;

                br.BaseStream.Seek(offset, SeekOrigin.Begin);

            }




            return br.BaseStream.Position;

        }

        public long ReadColumnCounter(BinaryReader br)
        {

            column_counter = new TR2ColumnCounter();

            column_counter.count = br.ReadInt32();

            column_counter.id = new int[column_counter.count];


            for (int i = 0; i < column_counter.count; i++)
            {

                column_counter.id[i] = br.ReadInt32();




            }




            return br.BaseStream.Position;
        }


        public static bool IsStringFormat(string type)
        {

            if (type == "ASCII" || type == "UTF-16LE" || type == "UTF-8" || type == "UTF-16")
            {
                return true;
            }
            return false;

        }

        public void BuildColumnBinaryData()
        {

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

                        column_data.column_data_list = new TR2ColumnDataList[column_data.data_7C_7F_column_data_count];

                        List<int> duplicate_check_list = new List<int>();
                        for (int si = 0; si < column_data.column_data_list.Length; si++)
                        {




                            int data_arr_offset = br.ReadInt32();



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


                                column_data.column_data_list[si].column_data = new TR2ColumnDataArray[column_data.data_76_array_size];
                                Console.WriteLine($"{si + 1} Data Every Cell Have {column_data.column_data_list[si].column_data.Length} Object.");
                                br.BaseStream.Seek(data_arr_offset, SeekOrigin.Begin);


                                if (IsStringFormat(column_data.column_type) && column_data.column_data_list[si].column_data.Length > 1)
                                {
                                    Console.WriteLine($"{si + 1} Special Text Array Format:{column_data.column_type} with {column_data.column_data_list[si].column_data.Length} Object.");


                                    //List<Int16>  SpecialTextCursorList = new List<Int16>();
                                    Int16[] SpecialTextCursorList = new Int16[column_data.column_data_list[si].column_data.Length];

                                    Console.WriteLine($"Special Base Offset:{br.BaseStream.Position.ToString("X")}.");
                                    long Current_Speical_Position = br.BaseStream.Position;



                                    UInt16 Cursor_FFFF = br.ReadUInt16();

                                    if (Cursor_FFFF != 0xFFFF)
                                    {
                                        throw new InvalidDataException($"0x{br.BaseStream.Position.ToString("X")} Value {Cursor_FFFF.ToString("X")} Should be 0xFFFF For Cursor Header, Current:{Cursor_FFFF.ToString("X")}");
                                    }


                                    for (int ssi = 0; ssi < column_data.column_data_list[si].column_data.Length; ssi++)
                                    {
                                        var data_arr_list_arr = new TR2ColumnDataArray();



                                        Console.WriteLine($"Special Object {ssi} Data Offset:{br.BaseStream.Position.ToString("X")}.");

                                        Int16 Cursor_Read = br.ReadInt16();

                                        Console.WriteLine($"Special Object {ssi} Data Value:{Cursor_Read.ToString("X")}.");

                                        SpecialTextCursorList[ssi] = Cursor_Read;



                                    }

                                    UInt16 Text_FFFF = br.ReadUInt16();

                                    if (Text_FFFF != 0xFFFF)
                                    {
                                        throw new InvalidDataException($"0x{br.BaseStream.Position.ToString("X")} Value {Cursor_FFFF.ToString("X")} Should be 0xFFFF For Text Header, Current:{Cursor_FFFF.ToString("X")}");
                                    }




                                    for (int ssi = 0; ssi < column_data.column_data_list[si].column_data.Length; ssi++)
                                    {


                                        Int16 speical_cursor_item = SpecialTextCursorList[ssi];
                                        br.BaseStream.Position = Current_Speical_Position + speical_cursor_item;
                                        var data_arr_list_arr = new TR2ColumnDataArray();

                                        Console.WriteLine($"Text Object {ssi} Data Cursor:{br.BaseStream.Position.ToString("X")}.");
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
                                                throw new InvalidCastException($"Special Text Array:What is {column_data.column_type}?");
                                        }

                                        Console.WriteLine($"Object {ssi} Read Data:{data_arr_list_arr.value_string_view}");



                                        column_data.column_data_list[si].column_data[ssi] = data_arr_list_arr;


                                    }




                                }
                                else
                                {




                                    for (int ssi = 0; ssi < column_data.column_data_list[si].column_data.Length; ssi++)
                                    {
                                        var data_arr_list_arr = new TR2ColumnDataArray();



                                        Console.WriteLine($"Object {ssi} Data Cursor:{br.BaseStream.Position.ToString("X")}.");
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



                                        column_data.column_data_list[si].column_data[ssi] = data_arr_list_arr;


                                    }
                                }
                                br.BaseStream.Seek(br_position, SeekOrigin.Begin);
                            }





                        }
                        column_data.last_mark = br.ReadInt32();




                    }


                }








                this.table_column_infromation[i].column_data = column_data;

            }






        }

        public string SaveAsXml()
        {

            return XmlUtils.Save<TR2Reader>(this);

        }



    }





}
