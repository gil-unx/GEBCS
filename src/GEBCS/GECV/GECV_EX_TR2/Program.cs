using GECV_EX.Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GECV_EX_TR2
{
    internal class Program
    {

        public static DirectoryInfo ToDir;
        public static DirectoryInfo FromDir;


        static void Main(string[] args)
        {
            Console.WriteLine("GECV EX TR2 BY RANDERION(HAOJUN0823)");
            Console.WriteLine("https://blog.haojun0823.xyz/");
            Console.WriteLine("https://github.com/HaoJun0823/GECV");

            Console.WriteLine("This Program has been canceled beacause haojun0823 design a full GUI editor:\nSee https://github.com/HaoJun0823/GECV/");

            return;

            if (args.Length < 2 && args.Length > 3)
            {


                Console.WriteLine("Need 3 Args:1.{unpack}\n2.Origin Folder\n3.Target Folder");
                Console.WriteLine("Need 2 Args:1.{pack}\n2.Xml Folder");
                Console.WriteLine("Press Any Key To Exit");
                Console.ReadKey();



            }
            else
            {

                if (Directory.Exists(args[1]))
                {

                    FromDir = new DirectoryInfo(args[1]);

                }
                else
                {
                    throw new FileNotFoundException(args[1]);
                }



                if (args[0].ToLower().Equals("unpack")&& args.Length>2)
                {

                    if (Directory.Exists(args[2]))
                    {

                        ToDir = new DirectoryInfo(args[2]);

                    }
                    else
                    {
                        throw new FileNotFoundException(args[2]);
                    }


                    FileInfo[] files = FromDir.GetFiles("*.tr2", SearchOption.AllDirectories);


                    foreach (FileInfo file in files)
                    {

                        string xml_path = Path.GetDirectoryName(file.FullName) + "\\" + Path.GetFileNameWithoutExtension(file.FullName);

                        string r_path = Path.GetRelativePath(FromDir.FullName, xml_path);


                        string save_path = ToDir.FullName + "\\" + r_path + ".xml";

                        Directory.CreateDirectory(Path.GetDirectoryName(save_path));


                        Console.WriteLine($"Unpack {file.FullName} To {save_path}.");
                        GECV_EX.TR2.TR2Reader tr2;

                        //try
                        //{



                            tr2 = new GECV_EX.TR2.TR2Reader(File.ReadAllBytes(file.FullName));
                            string xml = tr2.SaveAsXml();

                            File.WriteAllText(save_path, xml);

                            //for(int i= 0;i< tr2.data_info_set.Length;i++)
                            //{
                            //    var data = tr2.data_info_set[i];
                            //    File.WriteAllBytes(ToDir.FullName+"\\"+r_path+"\\"+data.id.ToString().PadLeft(8,'0')+"_"+data.name+".bin",data.bin_data);
                            //}

                            //File.Create(ToDir.FullName + "\\" + r_path + "\\"+tr2.header_inf+".cfg");

                            //File.WriteAllBytes(ToDir.FullName + "\\" + r_path + "\\conf.dat",tr2.conf_data);



                        //}
                        //catch (Exception e)
                        //{

                        //    Console.WriteLine($"Pass Invaild File:{file.FullName}.Beacause:{e.Message}");
                            

                        //}





                    }






                }

                if (args[0].ToLower().Equals("pack"))
                {

                    FileInfo[] files = FromDir.GetFiles("*.xml", SearchOption.AllDirectories);


                    foreach (FileInfo file in files)
                    {


                        string tr2_file = Path.GetDirectoryName(file.FullName) + "\\" + Path.GetFileNameWithoutExtension(file.FullName) + ".tr2";


                        //Console.WriteLine($"Pack {file.FullName} To {tr2_file}.");
                        

                        //OldTR2Packer packer = new OldTR2Packer(file.FullName);

                        //byte[] data =packer.SaveAsTR2();


                        //File.WriteAllBytes(tr2_file, data);

                        //string[] str = packer.GetBookInformation().ToArray();

                        //File.WriteAllLines(tr2_file+".log", str);




                    }


                }

            }




        }
    }
}
