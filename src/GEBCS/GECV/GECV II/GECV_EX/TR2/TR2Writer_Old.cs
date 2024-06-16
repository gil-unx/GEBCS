using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.TR2
{
    public partial class TR2Writer
    {

        private int BuildBin_SONY_A(int offset)
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

                booker.SetBookMark($"table_column_information_{i}_data_column_serial_right_version_old", task_offset);
                booker.WriteData($"table_column_information_{i}_data_column_serial_right_version_old", new byte[] { 0x00, 0xCF, 0x07 });
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

                task_offset += 4;

                List<string> vaild_offset_list = new List<string>();
                List<string> invaild_offset_list = new List<string>();
                if (TR2Reader.IsStringFormat(tr2data.table_column_infromation[i].column_data.column_type) && tr2data.table_column_infromation[i].column_data.data_76_array_size > 1)
                {
                    booker.WriteData($"table_column_information_{i}_data_7C_7F_column_data_count", tr2data.table_column_infromation[i].column_data.column_data_list.Length * tr2data.table_column_infromation[i].column_data.data_76_array_size);
                    for (int si = 0; si < tr2data.table_column_infromation[i].column_data.column_data_list.Length; si++)
                    {
                        var parent = tr2data.table_column_infromation[i].column_data.column_data_list[si];
                        for (int ssi = 0; ssi < parent.column_data.Length; ssi++)
                        {
                            booker.SetBookMark($"table_column_information_{i}_column_data_list_special_{si}_{ssi}_id", task_offset);
                            booker.WriteData($"table_column_information_{i}_column_data_list_special_{si}_{ssi}_id", tr2data.column_counter.id[si]);
                            task_offset += 4;
                            booker.SetBookMark($"table_column_information_{i}_column_data_list_special_{si}_{ssi}_cursor", task_offset);
                            task_offset += 4;
                            booker.SetBookMark($"table_column_information_{i}_column_data_list_special_{si}_{ssi}_length", task_offset);
                            task_offset += 4;
                        }
                    }

                    for (int si = 0; si < tr2data.table_column_infromation[i].column_data.column_data_list.Length; si++)
                    {
                        var parent = tr2data.table_column_infromation[i].column_data.column_data_list[si];
                        for (int ssi = 0; ssi < parent.column_data.Length; ssi++)
                        {
                            var child = parent.column_data[ssi];
                            byte[] input_data = FileUtils.GetBytesByHexString(child.value_hex_view);
                            ResizeArrayWithStringType(ref input_data, tr2data.table_column_infromation[i].column_data.column_type);
                            booker.WriteData($"table_column_information_{i}_column_data_list_special_{si}_{ssi}_cursor", task_offset - offset);
                            booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_{ssi}", task_offset);
                            task_offset += input_data.Length;
                            
                            booker.WriteData($"table_column_information_{i}_column_data_list_{si}_{ssi}", input_data);

                            if (tr2data.table_column_infromation[i].column_data.column_type == "UTF-16" || tr2data.table_column_infromation[i].column_data.column_type == "UTF-16LE")
                            {
                                booker.WriteData($"table_column_information_{i}_column_data_list_special_{si}_{ssi}_length", input_data.Length - 2);
                                //Console.WriteLine($"OLD TR2 UTF16 LENGTH:{input_data.Length - 2}");
                            }
                            else
                            {
                                booker.WriteData($"table_column_information_{i}_column_data_list_special_{si}_{ssi}_length", input_data.Length - 1);
                                //Console.WriteLine($"OLD TR2 TEXT LENGTH:{input_data.Length - 1}");
                            }


                        }
                    }

                    booker.SetBookMark($"table_column_information_{i}_end_zero16", task_offset);
                    int zero_offset = task_offset % 16;
                    if (zero_offset != 0)
                    {
                        booker.WriteData($"table_column_information_{i}_end_zero16", new byte[16 - zero_offset]);

                        task_offset += 16 - zero_offset;
                    }



                    if (task_offset % 16 != 0)
                    {
                        throw new InvalidDataException($"{task_offset} % 16 != 0");
                    }

                    booker.WriteData("table_column_infromation_csize_" + i, task_offset - offset);
                    booker.WriteData("table_column_infromation_usize_" + i, task_offset - offset);


                }
                else
                {
                    booker.WriteData($"table_column_information_{i}_data_7C_7F_column_data_count", tr2data.table_column_infromation[i].column_data.column_data_list.Length);
                    //header end 0x80


                    for (int si = 0; si < tr2data.table_column_infromation[i].column_data.column_data_list.Length; si++)
                    {

                        var parent = tr2data.table_column_infromation[i].column_data.column_data_list[si];

                        booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_id", task_offset);
                        booker.WriteData($"table_column_information_{i}_column_data_list_{si}_id", tr2data.column_counter.id[si]);
                        task_offset += 4;

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


                        booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_length", task_offset);
                        task_offset += 4;

                    }

                    //booker.SetBookMark($"table_column_information_{i}_column_data_list_lastmark", task_offset);
                    ////booker.WriteData($"table_column_information_{i}_column_data_list_lastmark", tr2data.table_column_infromation[i].column_data.last_mark);
                    //task_offset += 4;
                    //booker.SetBookMark($"table_column_information_{i}_column_data_list_lastmark_zero4", task_offset);
                    //booker.WriteData($"table_column_information_{i}_column_data_list_lastmark_zero4", new byte[4]);
                    //task_offset += 4;

                    // OLD TR2 NO 16 ALIGN


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
                            booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}", task_offset);//object data
                            booker.WriteData($"table_column_information_{i}_column_data_list_{si}_offset", task_offset - offset);
                        }

                        int old_length = task_offset;

                        for (int ssi = 0; ssi < parent.column_data.Length; ssi++)
                        {

                            var child = parent.column_data[ssi];

                            byte[] input_data = FileUtils.GetBytesByHexString(child.value_hex_view);
                            ResizeArrayWithStringType(ref input_data, tr2data.table_column_infromation[i].column_data.column_type);
                            booker.SetBookMark($"table_column_information_{i}_column_data_list_{si}_{ssi}", task_offset);
                            task_offset += input_data.Length;
                            booker.WriteData($"table_column_information_{i}_column_data_list_{si}_{ssi}", input_data);
                        }

                        //Console.WriteLine($"OLD TR2 CHECK:{tr2data.table_column_infromation[i].column_data.column_type}");

                        if (TR2Reader.IsStringFormat(tr2data.table_column_infromation[i].column_data.column_type))
                        {

                            if (tr2data.table_column_infromation[i].column_data.column_type == "UTF-16" || tr2data.table_column_infromation[i].column_data.column_type == "UTF-16LE")
                            {
                                booker.WriteData($"table_column_information_{i}_column_data_list_{si}_length", task_offset - old_length - 2);
                                //Console.WriteLine($"OLD TR2 UTF16 LENGTH:{task_offset-old_length-2}");
                            }
                            else
                            {
                                booker.WriteData($"table_column_information_{i}_column_data_list_{si}_length", task_offset - old_length - 1);
                                //Console.WriteLine($"OLD TR2 TEXT LENGTH:{task_offset - old_length - 1}");
                            }


                        }
                        else
                        {
                            booker.WriteData($"table_column_information_{i}_column_data_list_{si}_length", task_offset - old_length);
                            //Console.WriteLine($"OLD TR2 NORMAL LENGTH:{task_offset - old_length}");
                        }





                        // booker.GetBookMark($"table_column_information_{i}_column_data_list_{si}");





                    }
                    //booker.WriteData($"table_column_information_{i}_column_data_list_lastmark", task_offset - offset);




                    booker.SetBookMark($"table_column_information_{i}_end_zero16", task_offset);
                    int zero_offset = task_offset % 16;
                    if (zero_offset != 0)
                    {
                        booker.WriteData($"table_column_information_{i}_end_zero16", new byte[16 - zero_offset]);

                        task_offset += 16 - zero_offset;
                    }



                    if (task_offset % 16 != 0)
                    {
                        throw new InvalidDataException($"{task_offset} % 16 != 0");
                    }


                    foreach (var item in invaild_offset_list)
                    {

                        booker.WriteData(item, task_offset - offset);
                    }
                    booker.WriteData("table_column_infromation_csize_" + i, task_offset - offset);
                    booker.WriteData("table_column_infromation_usize_" + i, task_offset - offset);





                    
                }
                offset = task_offset;
                //FOR BIN FILE END
            }
            
            return offset;

        }


    }
}
