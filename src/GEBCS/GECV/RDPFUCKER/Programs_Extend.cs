using GECV_Extend;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GECV.Log;

namespace RDPFUCKER
{
    internal class Programs_Extend
    {

        static FileInfo SystemRES;
        static FileInfo SystemUpdateRES;
        static FileInfo DataRDP;
        static FileInfo PackageRDP;
        static FileInfo PatchRDP;


        public static byte[] System;
        public static byte[] SystemUpdate;
        public static byte[] Data;
        public static byte[] Package;
        public static byte[] Patch;


        public static DirectoryInfo TargetDirectiory;
        static DirectoryInfo SourceDirectiory;

        static bool IsPS4;

        public static async Task Virtual_MainAsync(string[] args)
        {

            if (args.Length < 3)
            {
                Info($"你输入的参数数量不对：第一个参数：原始数据文件夹，第二个参数：解包文件夹，第三个参数：ps4/psv");
            }


            SourceDirectiory = new DirectoryInfo(args[0]);
            TargetDirectiory = new DirectoryInfo(args[1]);

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

            Info("请核实这些数据，以免发生意外，按任意键开始解包！");
            Console.ReadKey();

            Parallel.Invoke(() => { System = ReadAllBytes(SystemRES); }, () => { SystemUpdate = ReadAllBytes(SystemUpdateRES); }, () => { Data = ReadAllBytes(DataRDP); }, () => { Package = ReadAllBytes(PackageRDP); }, () => { Patch = ReadAllBytes(PatchRDP); });

            Info($"关联文件读取完毕！");

            ConsoleRES sRES = new ConsoleRES(System,IsPS4,"system.res");
            ConsoleRES suRES = new ConsoleRES(SystemUpdate, IsPS4, "system_update.res");



            sRES.FullDecode();

        }


        public static byte[] ReadAllBytes(FileInfo file)

        {

            byte[] result;
            if (file.Exists)
            {
                 result = File.ReadAllBytes(file.FullName);
            }
            else
            {
                result = new byte[0];
            }

            Info($"{file.Name}读取完毕！读取到的大小:{result.Length}");
            return result;
        }


        public static void PrintFileStatus(FileSystemInfo info)
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


        public string GetRelativePath(string root, string file)
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




    }
}
