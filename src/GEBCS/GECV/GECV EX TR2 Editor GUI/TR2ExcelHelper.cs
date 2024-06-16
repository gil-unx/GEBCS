using GECV_EX.TR2;
using GECV_EX.Utils;
using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX_TR2_Editor_GUI
{
    public static class TR2ExcelHelper
    {

        public static string ParseHex(string hex, string data_type)
        {

            Console.WriteLine($"Excel Parsing:{hex}");
            var bytes = FileUtils.GetBytesByHexString(hex);


            byte[] data_bytes;
            string utf16_data;
            char[] chars;

            switch (data_type)
            {
                case "ASCII":
                    data_bytes = Encoding.Convert(Encoding.ASCII, Encoding.Unicode, bytes);
                    //chars = new char[Encoding.Unicode.GetCharCount(data_bytes, 0, data_bytes.Length)];
                    //utf16_data = new string(chars);
                    utf16_data = Encoding.Unicode.GetString(data_bytes);
                    break;
                case "UTF-16LE":
                    utf16_data = Encoding.Unicode.GetString(bytes);
                    break;
                case "UTF-8":
                    data_bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);
                    //chars = new char[Encoding.Unicode.GetCharCount(data_bytes, 0, data_bytes.Length)];
                    //utf16_data = new string(chars);
                    utf16_data = Encoding.Unicode.GetString(data_bytes);
                    break;
                case "UTF-16":
                    utf16_data = Encoding.Unicode.GetString(bytes);
                    break;
                default:
                    throw new ArgumentException($"What is {data_type}?");
            }


            Console.WriteLine($"Return UTF-16 String:{utf16_data}");

            //MessageBox.Show($"{hex}:{xdata}:{bytes.Length}");
            return utf16_data;
        }

        public static string GetHex(string str, string data_type)
        {

            Console.WriteLine($"Excel Parsing:{str}");

            byte[] bytes;

            switch (data_type)
            {
                case "ASCII":
                    bytes = Encoding.ASCII.GetBytes(str);
                    break;
                case "UTF-16LE":
                    bytes = Encoding.Unicode.GetBytes(str);
                    break;
                case "UTF-8":
                    bytes = Encoding.UTF8.GetBytes(str);
                    break;
                case "UTF-16":
                    bytes = Encoding.Unicode.GetBytes(str);
                    break;
                default:
                    throw new ArgumentException($"What is {data_type}?");
            }



            string hex = FileUtils.GetByteArrayString(bytes); ;
            //MessageBox.Show($"{hex}:{xdata}:{bytes.Length}");
            Console.WriteLine($"Return UTF-16 Hex:{hex}");
            return hex;
        }


        public static void Save(DataTable view_table, DataTable shadow_Table, string file_path)
        {
            List<TR2Excel> tr2list = new List<TR2Excel>();


            for (int row = 0; row < view_table.Rows.Count; row++)
            {
                DataRow view_dr = view_table.Rows[row];
                DataRow shadow_dr = shadow_Table.Rows[row];

                for (int col = 4; col < view_table.Columns.Count; col++)
                {

                    TR2Excel excel = new TR2Excel();

                    excel.Id = Convert.ToInt32(view_dr["Id"]);
                    excel.Name = view_dr["Name"].ToString();
                    excel.Type = view_dr["Type"].ToString();
                    excel.Index = Convert.ToInt32(view_dr["Index"]);
                    excel.Column = Convert.ToInt32(view_table.Columns[col].ColumnName);


                    Console.WriteLine($"Id:{excel.Id},Name:{excel.Name},Type:{excel.Type},Index:{excel.Index},Column:{excel.Column}");


                    if (TR2Reader.IsStringFormat(excel.Type))
                    {
                        Console.WriteLine($"Text Value:{shadow_dr[view_table.Columns[col].ColumnName].ToString()}");
                        string origin = ParseHex(shadow_dr[view_table.Columns[col].ColumnName].ToString(), excel.Type);
                        excel.Hex = shadow_dr[view_table.Columns[col].ColumnName].ToString();
                        excel.Value = origin;
                        excel.Import = 2;
                    }
                    else
                    {
                        Console.WriteLine($"Data Value:{view_dr[view_table.Columns[col].ColumnName].ToString()}");
                        excel.Value = view_dr[view_table.Columns[col].ColumnName].ToString();
                        excel.Hex = "";
                        excel.Import = 1;

                    }

                    if(excel.Value == "[[GECV-EDITOR::NULL]]")
                    {
                        excel.Import = 0;
                    }

                    tr2list.Add(excel);

                }

            }

            if (File.Exists(file_path))
            {
                File.Delete(file_path);
            }

            using (FileStream fs = new FileStream(file_path, FileMode.Create, FileAccess.Write))
            {
                MiniExcel.SaveAs(fs, tr2list, true, view_table.TableName, ExcelType.XLSX, null);
            }





        }


        public static void Load(ref DataTable view_table, ref DataTable shadow_Table, string file_path)
        {

            using(var stream = File.OpenRead(file_path))
            {
                var excel_data = stream.Query<TR2Excel>().ToList();



                foreach(var excel in excel_data)
                {
                    Console.WriteLine($"Get {excel.Id}-{excel.Name} Type is {excel.Type}.");
                    if(excel.Import == 0)
                    {
                        Console.WriteLine("No need import.");
                        continue;
                    }else if(excel.Import <0 || excel.Import > 2)
                    {
                        Console.WriteLine($"Wrong import Data:{excel.Import}.");
                    }

                    else
                    {
                        if(TR2Reader.IsStringFormat(excel.Type))
                        {
                            UpdateRow(ref shadow_Table, excel);
                            continue;
                        }
                        else
                        {
                            UpdateRow(ref view_table, excel);
                            continue;
                        }
                    }
                    
                    

                }



            }



        }


        private static void UpdateRow(ref DataTable dt, TR2Excel excel) { 
        
        
            foreach(DataRow row in dt.Rows)
            {
                string dt_id = row["Id"].ToString();
                string dt_name = row["Name"].ToString();
                string dt_type = row["Type"].ToString();
                string dt_index = row["Index"].ToString();
                

                if(excel.Id.ToString() == dt_id && excel.Name == dt_name && excel.Type == dt_type && excel.Index.ToString() == dt_index)
                {
                    Console.WriteLine($"Get Row:{excel.GetImportLog()}");

                    
                    if(excel.Import==1 && !TR2Reader.IsStringFormat(excel.Type))
                    {
                        Console.WriteLine($"Import {excel.Value} To Top Table.");
                        row[excel.Column.ToString()] = excel.Value;
                        continue;
                    }

                    if (excel.Import == 2 && !TR2Reader.IsStringFormat(excel.Type))
                    {
                        Console.WriteLine($"{excel.Value} Should not import by hex.");
                        continue;
                    }

                    if (excel.Import == 1 && TR2Reader.IsStringFormat(excel.Type))
                    {
                        row[excel.Column.ToString()] = GetHex(excel.Value,excel.Type);
                        Console.WriteLine($"Import {excel.Value} To Shadow Table.");
                        continue;
                    }

                    if (excel.Import == 2 && TR2Reader.IsStringFormat(excel.Type))
                    {
                        //row[excel.Column.ToString()] = Form_HexEditor.ParseHex(excel.Hex,excel.Type);
                        row[excel.Column.ToString()] = excel.Hex;
                        Console.WriteLine($"Import {excel.Value} To Shadow Table.");
                        continue;
                    }

                    Console.Write($"Not Found.");

                }


                
            }
        
        }

        



    }
}
