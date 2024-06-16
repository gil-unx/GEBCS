using GECV_EX.PC;
using GECV_EX.PC.Packer;
using GECV_EX.Utils;
using System.Collections.Generic;
using System.IO;

namespace GECV_EX_PRES
{
    internal class Program
    {


        static void Main(string[] args)
        {


            Console.WriteLine("GECV EX PRES BY RANDERION(HAOJUN0823)");
            Console.WriteLine("https://blog.haojun0823.xyz/");
            Console.WriteLine("https://github.com/HaoJun0823/GECV");

            foreach(var i in args)
            {
                Console.WriteLine($"Args:{i}");
            }


            if(args.Length == 1)
            {

                var RootDir = new DirectoryInfo(args[0]);
                

                var files = RootDir.GetFiles("*.pres",SearchOption.TopDirectoryOnly);

                List<string> list = new List<string>();

                foreach(var f in files)
                {
                    try
                    {
                        byte[] b = File.ReadAllBytes(f.FullName);

                        Console.WriteLine($"Read{f.FullName}.Length:{b.Length}");

                        PresPC pres = new PresPC(b);
                        list.Add(f.FullName+",PASS");
                    }catch (Exception e)
                    {
                        Console.WriteLine($"Error:{f.FullName}:{e.Message}");
                        list.Add(f.FullName + $",ERROR:{e.Message}");
                    }
                }


                File.WriteAllLines(RootDir.FullName+"\\gecv_pres_vaild.log", list);

                

                //Console.WriteLine("Press Any Key To Exit.");
                //Console.ReadKey();
            }


            if (args.Length <3) {

                Console.WriteLine("Pack:You Need 3 Args:\n0.pack\n1.{Pres Xml Folder}\n2.{Target Directory}");
                Console.WriteLine("Unpack:You Need 3 Args:\n0.unpack\n1.{Pres Folder}\n2.{Target Directory}");
                Console.WriteLine("VAILD MODE:You Need 1 Args:\n1.{Target Directory}");
                Console.WriteLine("Args 4:{*}=Parallel");
                //Console.WriteLine("Press Any Key To Exit.");
                //Console.ReadKey();
                return;


            } if(args.Length <4) {



                if (args[0].ToLower().Equals("unpack"))
                {


                    DirectoryInfo from_dir = new DirectoryInfo(args[1]);
                    DirectoryInfo to_dir = new DirectoryInfo(args[2]);


                    var files = from_dir.GetFiles("*.pres", SearchOption.TopDirectoryOnly);

                    Parallel.ForEach<FileInfo>(files, f => {

                        Console.WriteLine(f.FullName);
                        string to_path = to_dir.FullName + "\\" + Path.GetFileNameWithoutExtension(f.Name);

                        Decode(f, to_path);

                    });



                }
                else if (args[0].ToLower().Equals("pack"))
                {


                    DirectoryInfo from_dir = new DirectoryInfo(args[1]);
                    DirectoryInfo to_dir = new DirectoryInfo(args[2]);


                    var dirs = from_dir.GetDirectories("*", SearchOption.TopDirectoryOnly);


                    Parallel.ForEach<DirectoryInfo>(dirs, d => {

                        Console.WriteLine(d.FullName);

                        Directory.CreateDirectory(to_dir.FullName);

                        Encode(d.FullName, to_dir + "\\" + d.Name + ".pres");

                    });


                }


            }
            else
            {

                if (args[0].ToLower().Equals("unpack"))
                {


                    DirectoryInfo from_dir = new DirectoryInfo(args[1]);
                    DirectoryInfo to_dir = new DirectoryInfo(args[2]);


                    var files = from_dir.GetFiles("*.pres", SearchOption.TopDirectoryOnly);


                    foreach (var f in files)
                    {
                        Console.WriteLine(f.FullName);
                        string to_path = to_dir.FullName + "\\" + Path.GetFileNameWithoutExtension(f.Name);

                        Decode(f, to_path);


                    }



                }
                else if (args[0].ToLower().Equals("pack"))
                {



                    DirectoryInfo from_dir = new DirectoryInfo(args[1]);
                    DirectoryInfo to_dir = new DirectoryInfo(args[2]);


                    var dirs = from_dir.GetDirectories("*", SearchOption.TopDirectoryOnly);


                    foreach (var d in dirs)
                    {

                        Console.WriteLine(d.FullName);
                        Encode(d.FullName, to_dir + "\\" + d.Name + ".pres");



                    }





                }
            }








            
            //Console.WriteLine("Press Any Key To Exit.");
            //Console.ReadKey();

        }


        static void Encode(string root_path,string target_path)
        {

            
            PresPacker pk = new PresPacker(root_path);

            byte[] result = pk.GetPresFileBytes();


            File.WriteAllBytes(target_path, result);

            File.WriteAllLines(root_path + "\\register_list.log", pk.GetRegisterText());
            File.WriteAllLines(root_path + "\\gecv_book_list.log", pk.GetBookInformation());


            PresPC p = new PresPC(result);


        }


        static void Decode(FileInfo file,string root_path)
        {


            DirectoryInfo root = new DirectoryInfo(root_path);


            byte[] b = File.ReadAllBytes(file.FullName);

            Console.WriteLine($"Read{file.FullName}.Length:{b.Length}");

            PresPC pres = new PresPC(b);

            pres.Unpack(root.FullName+"\\Data");

            //FileUtils.CreateSymbolLinkDosCommandFile(pres.symbol_map,RootDir.FullName);

            string mod_path = root.FullName + "\\Mod\\";

            if (Path.Exists(mod_path))
            {
                File.Delete(mod_path);
            }

            Directory.CreateDirectory(mod_path);

            foreach (var kv in pres.symbol_map)
            {
                string target = mod_path+ kv.Value;


                Directory.CreateDirectory(Path.GetDirectoryName(target));

                //File.CreateSymbolicLink(target,kv.Key);


                if (String.IsNullOrEmpty(kv.Value))
                {
                    continue;
                }

                if (!File.Exists(target))
                {
                    File.Copy(kv.Key, target,true);
                    File.Copy(Path.GetDirectoryName(kv.Key) + "\\" + Path.GetFileNameWithoutExtension(kv.Key) + ".xml", Path.GetDirectoryName(target) + "\\" + Path.GetFileNameWithoutExtension(target) + ".xml", true);
                }


                string r_o_p = Path.GetRelativePath(root.FullName+"\\Data",kv.Key);
                string r_t_p = Path.GetRelativePath (mod_path,target);

                File.AppendAllText(target + ".ini", $"{r_o_p}={r_t_p}\n");



            }




        }

    }
}
