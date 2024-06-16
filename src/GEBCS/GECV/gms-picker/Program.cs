using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gms_picker
{
    internal class Program
    {


        static DirectoryInfo dir;


        static DataTable dt;

        static string gms_mark = "assetPath";


        static void Main(string[] args)
        {

            if(args.Length != 1) {

                Console.WriteLine("You Need Input A Directory.");
                return;
            }


            dir = new DirectoryInfo(args[0]);

            dt = new DataTable();

            dt.Columns.Add("file",typeof(string));
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("data", typeof(string));



            var files = dir.GetFiles("*.*", SearchOption.AllDirectories);


            Parallel.ForEach<FileInfo>(files, file =>
            {


                GetFileGMSData(file);

            });


            File.Delete(dir.FullName+".xlsx");

            MiniExcel.SaveAs(dir.FullName+".xlsx",dt);



        }






        public static void GetFileGMSData(FileInfo file)
        {
            int id = 0;

            int count = 0;

            byte[] bytes = File.ReadAllBytes(file.FullName);


            StringBuilder stringBuilder = new StringBuilder();


            using(MemoryStream ms = new MemoryStream(bytes)) { 
                using(BinaryReader reader = new BinaryReader(ms))
                {

                    while(reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        char c = Convert.ToChar(reader.ReadByte());


                        if (c.Equals(gms_mark[count]))
                        {

                            count++;


                            if (count == gms_mark.Length)
                            {

                                while (reader.ReadByte() == 0x0 && reader.BaseStream.Position < bytes.Length) ;
                                reader.BaseStream.Seek(-1, SeekOrigin.Current);
                                string data = readNullterminated(reader);



                                lock (dt)
                                {
                                    DataRow dr = dt.NewRow();

                                    dr["file"] = file.FullName.Substring(dir.FullName.Length);
                                    dr["id"] = id++;
                                    dr["data"] = data;
                                    dt.Rows.Add(dr);
                                }

                                count = 0;
                            }


                        }
                        else
                        {
                            count = 0;
                        }
                    }


                }
            
            }
            





        }

        public static string readNullterminated(BinaryReader reader)
        {
            var char_array = new List<byte>();
            string str = "";
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                str = Encoding.UTF8.GetString(char_bytes2);
                return str;
            }
            byte b = reader.ReadByte();
            while ((b != 0x00) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                char_array.Add(b);
                b = reader.ReadByte();
            }
            byte[] char_bytes = char_array.ToArray();
            str = Encoding.UTF8.GetString(char_bytes);
            return str;
        }


    }
}
