using GECV_EX.Shared;
using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.TR2
{
    public partial class TR2Writer
    {

        private BinaryBooker booker;

        private TR2Reader tr2data;



        private TR2Version tr2version;


        public int new_year = 0;


        public TR2Writer(TR2Reader tr2data, TR2Version tR2Version = TR2Version.PC,int new_year = 0)
        {
            this.tr2data = tr2data;
            this.new_year = new_year;
            tr2version = tR2Version;
            this.booker = new BinaryBooker();
            Build();
        }


        private void Build()
        {

            int offset = 0;
            booker.SetBookMark("file_header", offset);
            offset += 4;
            booker.SetBookMark("file_header_magic", offset);
            offset += 4;
            byte[] table_name_data = Get48ByteLengthStringData(tr2data.table_name);

            booker.SetBookMark("table_name", offset);
            offset += 48;

            booker.SetBookMark("table_column_infromation_offset", offset);
            offset += 4;
            booker.SetBookMark("table_column_infromation_count", offset);

            booker.WriteData("file_header", tr2data.file_header);

            if (new_year != 0)
            {
                booker.WriteData("file_header_magic", new_year);
                Console.WriteLine($"Is Custom Year:{new_year.ToString("X8")}");
            }
            else
            {
                booker.WriteData("file_header_magic", tr2data.file_header_magic);
                Console.WriteLine($"Not Custom Year:{tr2data.file_header_magic.ToString("X8")}");
            }


            //switch (tr2version)
            //{
            //    case TR2Version.PC:
            //        booker.WriteData("file_header_magic", TR2Reader.NEW_VERSION_HEADER);
            //        break;
            //    case TR2Version.SONY:
            //        booker.WriteData("file_header_magic", TR2Reader.OLD_VERSION_HEADER);
            //        break;
            //    case TR2Version.SONY_B:
            //        booker.WriteData("file_header_magic", TR2Reader.OLD_OLD_VERSION_HEADER);
            //        break;
            //    default:
            //        throw new InvalidDataException("Which Version? Please Contact Developer.");
            //}

            //booker.WriteData("file_header_magic", tr2data.file_header_magic);

            booker.WriteData("table_name", table_name_data);



            booker.WriteData("table_column_infromation_offset", 0x40);

            booker.WriteData("table_column_infromation_count", tr2data.table_column_infromation.Length);

            offset = 64;


            for (int i = 0; i < tr2data.table_column_infromation.Length; i++) {

                booker.SetBookMark("table_column_infromation_id_" + i, offset);
                booker.WriteData("table_column_infromation_id_" + i, tr2data.table_column_infromation[i].id);
                offset += 4;
                booker.SetBookMark("table_column_infromation_offset_" + i, offset); //BuildBin
                offset += 4;
                booker.SetBookMark("table_column_infromation_magic_" + i, offset);
                booker.WriteData("table_column_infromation_magic_" + i, tr2data.table_column_infromation[i].magic);
                offset += 4;
                booker.SetBookMark("table_column_infromation_csize_" + i, offset); //BuildBin
                offset += 4;
                booker.SetBookMark("table_column_infromation_usize_" + i, offset); //BuildBin

                offset += 4;


            }

            if (this.tr2version == TR2Version.PC) {

                booker.SetBookMark("column_data_count", offset);
                offset += 4;
                booker.WriteData("column_data_count", tr2data.column_counter.id.Length);


                for (int i = 0; i < tr2data.column_counter.id.Length; i++)
                {
                    booker.SetBookMark("column_data_count_data_" + i, offset);
                    offset += 4;
                    booker.WriteData("column_data_count_data_" + i, tr2data.column_counter.id[i]);
                }


            }


            booker.SetBookMark("tr2_header_inf_zero16", offset);
            int zero_offset = offset % 16;

            if (zero_offset != 0)
            {

                booker.WriteData("tr2_header_inf_zero16", new byte[16 - zero_offset]);
                offset += 16 - zero_offset;
            }


            switch (this.tr2version)
            {
                case TR2Version.SONY:
                    offset = BuildBin_SONY_A(offset);
                    break;
                default:
                    offset = BuildBin(offset);
                    break;
            }

            //offset = BuildBin(offset);


        }

        public static int ShortYearToIntYear(short year)
        {

            String hex_str = year.ToString("X4");

            hex_str = hex_str.PadRight(8, '0');
            
            Console.WriteLine($"Convert short to int {year.ToString("X4")} to {hex_str}");

            if (year == 2015)
            {
                hex_str = hex_str.Substring(0, 7) + '2';
            }
           

            int result = Convert.ToInt32(hex_str, 16);
            Console.WriteLine($"Result:{result.ToString("X8")} is {result}");

            return result;

        }


        private int BuildBin(int offset)
        {

            int task_offset = offset;

            for (int i = 0; i < tr2data.table_column_infromation.Length; i++)
            {
                booker.WriteData("table_column_infromation_offset_" + i, offset);

                booker.SetBookMark($"table_column_information_{i}_data_column_name", task_offset);
                byte[] array_name = Get48ByteLengthStringData(tr2data.table_column_infromation[i].column_data.column_name);
                booker.WriteData($"table_column_information_{i}_data_column_name", array_name);
                task_offset += 48;

                booker.SetBookMark($"table_column_information_{i}_data_column_serial_left", task_offset);
                booker.WriteData($"table_column_information_{i}_data_column_serial_left", tr2data.table_column_infromation[i].column_data.column_serial_left);
                task_offset += 8;
                booker.SetBookMark($"table_column_information_{i}_data_column_serial_right", task_offset);
                booker.WriteData($"table_column_information_{i}_data_column_serial_right", tr2data.table_column_infromation[i].column_data.column_serial_right);
                task_offset += 5;

                booker.SetBookMark($"table_column_information_{i}_data_column_serial_right_version", task_offset);
                booker.WriteData($"table_column_information_{i}_data_column_serial_right_version",  new byte[] { 0x02,0xDF,0x07 });
                task_offset += 3;


                booker.SetBookMark($"table_column_information_{i}_data_column_type", task_offset);
                byte[] column_type = Get48ByteLengthStringData(tr2data.table_column_infromation[i].column_data.column_type);
                booker.WriteData($"table_column_information_{i}_data_column_type", column_type);
                task_offset += 48;

                

                booker.SetBookMark($"table_column_information_{i}_data_70_73", task_offset);
                booker.WriteData($"table_column_information_{i}_data_70_73", tr2data.table_column_infromation[i].column_data.data_70_73);
                task_offset += 4;

                booker.SetBookMark($"table_column_information_{i}_data_74", task_offset);
                booker.WriteData($"table_column_information_{i}_data_74", tr2data.table_column_infromation[i].column_data.data_74);
                task_offset += 1;

                booker.SetBookMark($"table_column_information_{i}_data_75", task_offset);
                booker.WriteData($"table_column_information_{i}_data_75", tr2data.table_column_infromation[i].column_data.data_75);
                task_offset += 1;
                booker.SetBookMark($"table_column_information_{i}_data_76_array_size", task_offset);
                booker.WriteData($"table_column_information_{i}_data_76_array_size", tr2data.table_column_infromation[i].column_data.data_76_array_size);
                task_offset += 1;
                booker.SetBookMark($"table_column_information_{i}_data_77", task_offset);
                booker.WriteData($"table_column_information_{i}_data_77", tr2data.table_column_infromation[i].column_data.data_77);
                task_offset += 1;

                booker.SetBookMark($"table_column_information_{i}_data_78_7B", task_offset);
                booker.WriteData($"table_column_information_{i}_data_78_7B", tr2data.table_column_infromation[i].column_data.data_78_7B);
                task_offset += 4;

                booker.SetBookMark($"table_column_information_{i}_data_7C_7F_column_data_count", task_offset);
                booker.WriteData($"table_column_information_{i}_data_7C_7F_column_data_count", tr2data.table_column_infromation[i].column_data.column_data_list.Length);
                task_offset += 4;

                //header end 0x80

                List<string> vaild_offset_list = new List<string>();
                List<string> invaild_offset_list = new List<string>();

                for(int si=0; si<tr2data.table_column_infromation[i].column_data.column_data_list.Length; si++)
                {

                    var parent = tr2data.table_column_infromation[i].column_data.column_data_list[si];
                    if (parent.IsInVaildOffset)
                    {
                        string invaild = $"table_column_information_{i}_column_data_list_{si}_offset_invaild";
                        invaild_offset_list.Add(invaild);
                        booker.SetBookMark(invaild, task_offset);
                    }
                    else
                    {
                        string vaild = $"table_column_information_{i}_column_data_list_{si}_offset";
                        vaild_offset_list.Add(vaild);
                        booker.SetBookMark(vaild, task_offset);
                        
                    }

                    task_offset += 4;

                }

                booker.SetBookMark($"table_column_information_{i}_column_data_list_lastmark", task_offset);
                //booker.WriteData($"table_column_information_{i}_column_data_list_lastmark", tr2data.table_column_infromation[i].column_data.last_mark);
                task_offset += 4;
                booker.SetBookMark($"table_column_information_{i}_column_data_list_lastmark_zero4", task_offset);
                booker.WriteData($"table_column_information_{i}_column_data_list_lastmark_zero4", new byte[4]);
                task_offset += 4;
                //booker.SetBookMark($"table_column_information_{i}_column_data_list_zero16", task_offset);

                //int zero_offset = task_offset % 16;
                //if (zero_offset != 0)
                //{
                //    booker.WriteData($"table_column_information_{i}_column_data_list_zero16", new byte[16 - zero_offset]);
                //    task_offset += 16 - zero_offset;
                //}

                for (int si = 0; si < tr2data.table_column_infromation[i].column_data.column_data_list.Length; si++)
                {

                    var parent = tr2data.table_column_infromation[i].column_data.column_data_list[si];

                    

                    if (parent.IsInVaildOffset)
                    {
                        continue;
                    }
                    else
                    {
                        booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}", task_offset);
                        booker.WriteData($"table_column_information_{i}_column_data_list_{si}_offset",task_offset - offset);
                    }

                    if (TR2Reader.IsStringFormat(tr2data.table_column_infromation[i].column_data.column_type) && tr2data.table_column_infromation[i].column_data.column_data_list[si].column_data.Length > 1)
                    {

                        byte[] Byte_FFFF = { 0xff, 0xff, 0xff, 0xff };

                        booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_Cursor_FFFF_Header", task_offset);
                        booker.WriteData($"table_column_information_{i}_column_data_list_{si}_Cursor_FFFF_Header", Byte_FFFF);
                        long current_special_offset = task_offset;
                        task_offset += 2;
                        for (int ssi = 0; ssi < parent.column_data.Length; ssi++)
                        {

                            booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_{ssi}_Cursor_FFFF", task_offset);
                            task_offset += 2;
                        }

                        booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_Text_FFFF_Header", task_offset);
                        booker.WriteData($"table_column_information_{i}_column_data_list_{si}_Text_FFFF_Header", Byte_FFFF);
                        task_offset += 2;


                        for (int ssi = 0; ssi < parent.column_data.Length; ssi++)
                        {

                            var child = parent.column_data[ssi];

                            byte[] input_data = FileUtils.GetBytesByHexString(child.value_hex_view);
                            ResizeArrayWithStringType(ref input_data, tr2data.table_column_infromation[i].column_data.column_type);
                            booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_{ssi}", task_offset);
                            Int16 FFFF_cursor = Convert.ToInt16(task_offset - current_special_offset);

                            booker.WriteData($"table_column_information_{i}_column_data_list_{si}_{ssi}_Cursor_FFFF", BitConverter.GetBytes(FFFF_cursor));
                            task_offset += input_data.Length;
                            booker.WriteData($"table_column_information_{i}_column_data_list_{si}_{ssi}", input_data);
                            
                        }

                    }
                    else
                    {

                        for (int ssi = 0; ssi < parent.column_data.Length; ssi++)
                        {

                            var child = parent.column_data[ssi];

                            byte[] input_data = FileUtils.GetBytesByHexString(child.value_hex_view);
                            ResizeArrayWithStringType(ref input_data, tr2data.table_column_infromation[i].column_data.column_type);
                            booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_{ssi}", task_offset);
                            task_offset += input_data.Length;
                            booker.WriteData($"table_column_information_{i}_column_data_list_{si}_{ssi}", input_data);
                        }

                    }

                    // booker.GetBookMark($"table_column_information_{i}_column_data_list_{si}");





                }
                booker.WriteData($"table_column_information_{i}_column_data_list_lastmark", task_offset- offset);




                booker.SetBookMark($"table_column_information_{i}_end_zero16", task_offset);
                int zero_offset = task_offset % 16;
                if (zero_offset != 0)
                {
                    booker.WriteData($"table_column_information_{i}_end_zero16", new byte[16 - zero_offset]);

                    task_offset += 16 - zero_offset;
                }

                

                if(task_offset % 16 != 0)
                {
                    throw new InvalidDataException($"{task_offset} % 16 != 0");
                }


                foreach(var item in invaild_offset_list)
                {

                    booker.WriteData(item,task_offset - offset);
                }
                booker.WriteData("table_column_infromation_csize_" + i, task_offset- offset);
                booker.WriteData("table_column_infromation_usize_" + i, task_offset - offset);





                offset = task_offset;
            }


            return offset;

        }

        public bool CheckNullString(byte[] array)
        {

            if(array.Length == 1 && array[0]==0)
            {
                return true;
            }else if (array.Length == 2 && array[0] == 0 && array[1] == 0)
            {
                return true;
            }


            return false;
        }


        public void ResizeArrayWithStringType(ref byte[] array,string type)
        {

            //if (array.Length == 1)
            //{
            //    return;
            //}

            if (CheckNullString(array))
            {
                return;
            }
            

            

            switch (type)
            {
                case "ASCII":
                    Array.Resize<byte>(ref array, array.Length + 1);
                    break;
                case "UTF-8":
                    Array.Resize<byte>(ref array, array.Length + 1);
                    break;
                case "UTF-16":
                    Array.Resize<byte>(ref array, array.Length + 2);
                    break;
                case "UTF-16LE":
                    Array.Resize<byte>(ref array, array.Length + 2);
                    break;
                default:
                    return;
            }


        }

        public List<string> GetBookInformation()
        {
            return booker.GetBookInformation();
        }

        public byte[] GetTr2Data()
        {
            return booker.GetAllData();
        }


        private byte[] Get48ByteLengthStringData(string str)
        {
            byte[] header_inf_byte = Encoding.UTF8.GetBytes(str);

            if (header_inf_byte.Length > 48)
            {

                throw new OverflowException($"{str} bytes over 48 length! Current{header_inf_byte.Length}.");

            }

            Array.Resize<byte>(ref header_inf_byte, 48);

            return header_inf_byte;

        }

    }
}

