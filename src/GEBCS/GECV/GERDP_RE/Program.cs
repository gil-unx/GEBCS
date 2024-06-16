using GECV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GECV.Log;

namespace GERDP_RE
{
    internal class Program
    {

        public static FileInfo SystemRES;
        public static FileInfo SystemUpdateRES;
        public static FileInfo DataRDP;
        public static FileInfo PackageRDP;
        public static FileInfo PatchRDP;

        public static DirectoryInfo TargetDirectiory;
        static DirectoryInfo SourceDirectiory;

        static bool IsPS4;

        private static Res SR;

        static void Main(string[] args)
        {

            Info("CODE EATER 噬神者 RDP 解包器 BY 兰德里奥（HaoJun0823）");
            Info("https://blog.haojun0823.xyz/");
            Info("https://github.com/HaoJun0823/GECV");





            if (args.Length < 3)
            {
                Info($"你输入的参数数量不对：第一个参数：原始数据文件夹，第二个参数：解包文件夹，第三个参数：ps4/psv");
            }


            SourceDirectiory = new DirectoryInfo(args[0]);
            TargetDirectiory = new DirectoryInfo(args[1]);
            GECV.Log.SetLogFolder(TargetDirectiory);

            if (args[2].ToLower().Equals("ps4"))
            {
                IsPS4 = true;
            }
            else if (args[2].ToLower().Equals("psv"))
            {
                IsPS4 = false;
            }
            else
            {
                Info($"{args[2]}是个什么玩意啊？请输入ps4或者psv");
                return;
            }

            SystemRES = new FileInfo(SourceDirectiory.FullName + "\\system.res");
            SystemUpdateRES = new FileInfo(SourceDirectiory.FullName + "\\system_update.res");
            DataRDP = new FileInfo(SourceDirectiory.FullName + "\\data.rdp");
            PackageRDP = new FileInfo(SourceDirectiory.FullName + "\\package.rdp");
            PatchRDP = new FileInfo(SourceDirectiory.FullName + "\\patch.rdp");

            Info($"=====");
            PrintFileStatus(SystemRES);
            PrintFileStatus(SystemUpdateRES);
            PrintFileStatus(DataRDP);
            PrintFileStatus(PackageRDP);
            PrintFileStatus(PatchRDP);
            Info($"=====");
            PrintFileStatus(SourceDirectiory);
            PrintFileStatus(TargetDirectiory);
            Info($"=====");
            Info($"当前数据类型：{(IsPS4 ? "PS4" : "PSV")}");
            Info($"请核实这些数据，以免发生意外，按任意键开始删除{TargetDirectiory.FullName}并解包！");
            Console.ReadKey();

            TargetDirectiory.Delete(true);

            TargetDirectiory.Create();

            Info($"请输入选项，输入1处理system.res，输入2处理system_update.res，如果输入错误程序会退出请重新打开再进行。\n注意新的解包会覆盖日志和原始文件，你需要注意这一点！");
            var input = Console.ReadLine();

            

            switch (input)
            {
                case "1":
                    SR = new Res("system", SystemRES, IsPS4, TargetDirectiory.FullName);
                    break;
                case "2":
                    SR = new Res("system_update", SystemUpdateRES, IsPS4,TargetDirectiory.FullName);
                    break;
                default:
                    Info($"你还真就输错了！{input}是什么？");
                    Console.WriteLine("处理完毕！");
                    Console.ReadKey();
                    return;




            }

            var main_task = Task.Run(() => {

                if (args.Length<=4 && args[3].ToLower().Equals("nosr"))
                {
                    return;
                }

                SR.DecodeAll() ;





            }).ContinueWith(t => { Parallel.Invoke(() => { DoRtblSet(SR.res_file); }, () => { DoRtblSet(DataRDP); }, () => { DoRtblSet(PackageRDP); }, () => { DoRtblSet(PatchRDP); }); });



            Console.ReadKey();
            Log.flush();
            Info($"完成！");
            Console.ReadKey();







        }
        static void PrintFileStatus(FileSystemInfo info)
        {

            if (info.Exists)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Info($"数据项：{info.FullName}，状态：{(info.Exists ? "存在" : "缺失")}");

            Console.ResetColor();

        }

        public static string GetRelativePath(string root, string file)
        {



            if (root[root.Length - 1] == '\\')
            {
                return file.Substring(root.Length);
            }
            else
            {
                return file.Substring(root.Length + 1);
            }





        }

        public static void DoRtblSet(FileInfo file)
        {

            if(file.Exists)
            {
                Rtbl r = new Rtbl(file, IsPS4);

                r.folder_name = TargetDirectiory.FullName + "\\" + SR.title;

                r.Decode();
            }
            
            
            


        }

    }
        
}
