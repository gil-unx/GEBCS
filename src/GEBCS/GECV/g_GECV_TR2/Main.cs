using GECV_EX.TR2;
using GECV_EX.Utils;
using MiniExcelLibs;
using System.Configuration;
using System.Data;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;

namespace GECV_EX_TR2_Editor_GUI
{
    //public partial class Main : Form
   public class GECV_EX_TR2_Editor
    {

        public static DataTable System_DataTable;
        public static DataTable System_DataTable_Hex;
        public static TR2Reader System_TR2;
        public static string tr2Name;
        public static string excelName;
        public static string original_title;

        public static string input_file_name;
        public GECV_EX_TR2_Editor(string tr2name)
        {
            tr2Name = tr2name;
            excelName = Path.ChangeExtension(tr2name, "xlsx");
        }
        public void ExportExcel()
        {
            AutoOpenTR2();
            TR2ExcelHelper.Save(System_DataTable, System_DataTable_Hex, excelName);
            

        }
        public void ImportExcel()
        {
            AutoOpenTR2();

            TR2ExcelHelper.Load(ref System_DataTable, ref System_DataTable_Hex, excelName);
            UpdateTR2Reader();
            BuildDataTable(System_TR2);
            TR2Writer writer;
            int new_year = TR2Writer.ShortYearToIntYear(2015);
            writer = new TR2Writer(System_TR2, TR2Version.PC, new_year);
            File.WriteAllBytes(tr2Name, writer.GetTr2Data());

           // File.WriteAllLines(tr2Name+".log", writer.GetBookInformation());


        }
        public void AutoOpenTR2()
        {
            string select_file = tr2Name;


            string ext = Path.GetExtension(select_file);

            input_file_name = Path.GetFileNameWithoutExtension(select_file);

            try
            {


                if (ext.ToLower() == ".tr2")
                {

                    byte[] file_bytes = File.ReadAllBytes(select_file);

                    TR2Version tr2v = TR2Reader.ThinkTR2Version(file_bytes);

                    System_TR2 = new TR2Reader(file_bytes, tr2v);

                    string version_text;// = "Version:(Unknown)";

                    switch (tr2v)
                    {
                        case TR2Version.SONY:
                            version_text = "(Auto Ver.1)";
                            break;
                        case TR2Version.PC:
                            version_text = "(Auto Ver.2)";
                            break;
                        default:
                            version_text = "(Auto Ver.Unknown)";
                            break;
                    }

                    short year = TR2Reader.GetYearFromHeader(System_TR2.file_header_magic);

                    version_text += $"-{year}";

                   // this.Text = original_title + $" Building {System_TR2.table_name}({year}) Table, Please Wait... " + version_text;
                    BuildDataTable(System_TR2, true);
                   // RefreshDataTable();
                   // this.Text = original_title + " " + version_text + " " + select_file;
                   // SetMenuStatus(true);
                    return;
                }



            }
            catch (Exception ex)
            {
               // MessageBox.Show($"{select_file} Open Error:\n{ex.Message}\n{ex.StackTrace}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
               // input_file_name = "";
               // this.Text = original_title;
               //SetMenuStatus(false);
                return;
            }
           // MessageBox.Show($"{select_file} Is not supported file!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        public void BuildDataTable(TR2Reader tr2data, bool check = false)
        {
            DataTable dt = new DataTable();

            dt.TableName = tr2data.table_name;



            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("Index", typeof(int));
            //dt.Columns.Add("Array", typeof(byte));
            //dt.Columns.Add("Editor Mode", typeof(EditorDataModeEnum));





            //DataColumn row_col = new DataColumn("Row ID");
            //dt.Columns.Add(row_col);

            //for (int i = 0; i < tr2data.table_column_infromation.Length; i++)
            //{
            //    var tr2inf = tr2data.table_column_infromation[i];
            //    DataColumn d_col = new DataColumn(tr2inf.GetTableNameForEditor());
            //    dt.Columns.Add(d_col);


            //}


            //for(int i = 0; i < tr2data.column_counter.id.Length; i++)
            //{

            //    DataRow dr =dt.NewRow();








            //}

            List<string> Dulpicate_list = new List<string>();

            foreach (DataColumn dc in dt.Columns)
            {

                //if (dc.ColumnName == "Editor Mode")
                //{
                //    dc.ReadOnly = false;
                //}
                //else
                //{
                //    dc.ReadOnly = true;
                //}

                dc.ReadOnly = true;



            }



            for (int i = 0; i < tr2data.column_counter.id.Length; i++)
            {

                DataColumn id_col = new DataColumn(tr2data.column_counter.id[i].ToString(), typeof(string));


                dt.Columns.Add(id_col);

            }

            DataTable dt_hex = dt.Clone();


            for (int i = 0; i < tr2data.table_column_infromation.Length; i++)
            {
                var tr2data_inf = tr2data.table_column_infromation[i];


                for (int si = 0; si < tr2data.column_counter.id.Length; si++)
                {
                    Console.WriteLine($"Debug Build Table 76:{tr2data_inf.id}-{tr2data_inf.column_data.column_name}-{si}(Array Length:{tr2data_inf.column_data.column_data_list.Length})");

                    var data_arr = tr2data_inf.column_data;


                    //DataRow dr = dt.NewRow();
                    //dr["Id"] = tr2data_inf.id;
                    //dr["Name"] = tr2data_inf.column_data.column_name;
                    //dr["Type"] = tr2data_inf.column_data.column_type;
                    ////dr["Index"] = ssi;

                    //DataRow dr_hex = dt_hex.NewRow();
                    //dr_hex["Id"] = tr2data_inf.id;
                    //dr_hex["Name"] = tr2data_inf.column_data.column_name;
                    //dr_hex["Type"] = tr2data_inf.column_data.column_type;
                    ////dr_hex["Index"] = ssi;

                    //dt.Rows.Add(dr);
                    //dt_hex.Rows.Add(dr_hex);

                    //StringBuilder sb = new StringBuilder();

                    if (tr2data_inf.column_data.column_data_list[si].DulpicatedObjectOffset != 0)
                    {
                        Dulpicate_list.Add($"{tr2data_inf.id}-{tr2data_inf.column_data.column_name}-{tr2data_inf.column_data.column_type}-{tr2data.column_counter.id[si].ToString()}.");
                    }

                    for (byte ssi = 0; ssi < data_arr.data_76_array_size; ssi++)
                    {
                        Console.WriteLine($"Debug Build Table 76 Data:{tr2data_inf.id}-{tr2data_inf.column_data.column_name}-{si}-{ssi}(Array Length:{data_arr.data_76_array_size})");

                        DataRow dr = GetDatRowFromTable(ref dt, tr2data_inf.id, tr2data_inf.column_data.column_name, tr2data_inf.column_data.column_type, ssi);
                        DataRow dr_hex = GetDatRowFromTable(ref dt_hex, tr2data_inf.id, tr2data_inf.column_data.column_name, tr2data_inf.column_data.column_type, ssi);

                        if (tr2data_inf.column_data.column_data_list[si].IsInVaildOffset || tr2data_inf.column_data.column_data_list[si].column_data[ssi].IsInVaildArrayOffset)
                        {
                            dr[tr2data.column_counter.id[si].ToString()] = "[[GECV-EDITOR::NULL]]";
                            dr_hex[tr2data.column_counter.id[si].ToString()] = "[[GECV-EDITOR::NULL]]";
                        }
                        else
                        {




                            var data_arr_data = tr2data_inf.column_data.column_data_list[si].column_data[ssi];

                            //sb.Append(ssi);
                            //sb.Append(":{{");
                            //sb.Append(data_arr_data.value_string_view.ToString());
                            //sb.Append("}}");






                            dr[tr2data.column_counter.id[si].ToString()] = data_arr_data.value_string_view.ToString();
                            dr_hex[tr2data.column_counter.id[si].ToString()] = data_arr_data.value_hex_view.ToString();



                            //dr["Editor Mode"] = GetModeByDataType(dr["Type"].ToString());

                        }



                    }




                }






            }

            StringBuilder sb = new StringBuilder();

            foreach (var str in Dulpicate_list) { sb.Append(str); sb.Append('\n'); };

            if (check && Dulpicate_list.Count != 0)
            {

               // MessageBox.Show($"There are {Dulpicate_list.Count} data is dulpicate object:\n\n{sb.ToString()}\nThe source project will merge the same data to the same address (For optimization and publishing, since the data does not need to be modified again.), which is not possible to edit in the table.\nThe editor will allocate new pointers for each data, even though they all have the same data.\nYou need to know this. ", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }





            System_DataTable = dt;
            System_DataTable_Hex = dt_hex;
        }


        public DataRow GetDatRowFromTable(ref DataTable dt, int id, string column_name, string column_type, byte index)
        {

            //DataRow dr = dt.NewRow();
            //dr["Id"] = tr2data_inf.id;
            //dr["Name"] = tr2data_inf.column_data.column_name;
            //dr["Type"] = tr2data_inf.column_data.column_type;
            ////dr["Index"] = ssi;

            //DataRow dr_hex = dt_hex.NewRow();
            //dr_hex["Id"] = tr2data_inf.id;
            //dr_hex["Name"] = tr2data_inf.column_data.column_name;
            //dr_hex["Type"] = tr2data_inf.column_data.column_type;
            ////dr_hex["Index"] = ssi;

            //dt.Rows.Add(dr);
            //dt_hex.Rows.Add(dr_hex);


            if (dt.Rows != null && dt.Rows.Count != 0)
            {

                foreach (DataRow dr in dt.Rows)
                {

                    if (dr["Id"].ToString() == id.ToString() && dr["Name"].ToString() == column_name.ToString() && dr["Type"].ToString() == column_type.ToString() && dr["Index"].ToString() == index.ToString())
                    {

                        return dr;

                    }


                }
            }

            DataRow dr2 = dt.NewRow();

            dr2["Id"] = id;
            dr2["Name"] = column_name;
            dr2["Type"] = column_type;
            dr2["Index"] = index;

            dt.Rows.Add(dr2);

            return dr2;




        }

        private void UpdateTR2Reader()
        {


            foreach (DataRow row in System_DataTable.Rows)
            {

                int id = Convert.ToInt32(row["Id"].ToString());
                string name = row["Name"].ToString();

                string type = row["Type"].ToString();
                byte arr_index = Convert.ToByte(row["Index"].ToString());
                //NEXT =4;


                Dictionary<int, string> map = new Dictionary<int, string>();

                for (int cell_index = 4; cell_index < row.ItemArray.Length; cell_index++)
                {

                    var cell = row.ItemArray[cell_index];

                    map.Add(Convert.ToInt32(System_DataTable.Columns[cell_index].ColumnName), cell.ToString());



                }


                foreach (var kv in map)
                {

                    if (!type.Equals("ASCII") && !type.Equals("UTF-8") && !type.Equals("UTF-16") && !type.Equals("UTF-16LE")) // So I Can Do This At  string type = row["Type"].ToString();
                    {



                        if (!System_TR2.SetDataByIdNameTypeArrayIndexAndDataIdWithParseStringData(id, name, type, arr_index, kv.Key, kv.Value))
                        {
                           // if (MessageBox.Show($"String Converter:Set {id}-{name}-{type}-{arr_index}-{kv.Key}-{kv.Value} Error!\nContinue?", "Error!", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop) == DialogResult.Cancel)
                          //  {
                          //      break;
                          //  }
                        }
                    }



                }







            }


            foreach (DataRow row in System_DataTable_Hex.Rows)
            {

                int id = Convert.ToInt32(row["Id"].ToString());
                string name = row["Name"].ToString();

                string type = row["Type"].ToString();
                byte arr_index = Convert.ToByte(row["Index"].ToString());
                //NEXT =4;


                Dictionary<int, string> map = new Dictionary<int, string>();

                for (int cell_index = 4; cell_index < row.ItemArray.Length; cell_index++)
                {

                    var cell = row.ItemArray[cell_index];

                    map.Add(Convert.ToInt32(System_DataTable.Columns[cell_index].ColumnName), cell.ToString());



                }


                foreach (var kv in map)
                {

                    if (type.Equals("ASCII") || type.Equals("UTF-8") || type.Equals("UTF-16") || type.Equals("UTF-16LE")) // So I Can Do This At  string type = row["Type"].ToString();
                    {

                        try
                        {

                            byte[] input_byte = FileUtils.GetBytesByHexString(kv.Value);


                            if (!System_TR2.SetDataByIdNameTypeArrayIndexAndDataIdWithParseBytes(id, name, type, arr_index, kv.Key, input_byte))
                            {
                               // if (MessageBox.Show($"Byte Converter:Set {id}-{name}-{type}-{arr_index}-{kv.Key}-{kv.Value} Error!\nContinue?", "Error!", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop) == DialogResult.Cancel)
                              //  {
                              //      break;
                              //  }
                            }
                        }
                        catch (Exception ex)
                        {
                          //  MessageBox.Show($"String Converter:Set {id}-{name}-{type}-{arr_index}-{kv.Key}-{kv.Value} Error!\nData:{kv.Value}", "Error!", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);
                        }


                    }



                }







            }




        }


    }
}
