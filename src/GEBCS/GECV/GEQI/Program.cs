using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using static RETAEDOG.Log;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using System.Reflection;

namespace RETAEDOG
{
    internal class Program
    {

        public static DirectoryInfo RootDirectory = new DirectoryInfo(Environment.CurrentDirectory);

        static DirectoryInfo ContentDirectory = new DirectoryInfo(RootDirectory.FullName+"\\"+"retaedog");

        static DirectoryInfo WorkDirectory = new DirectoryInfo(ContentDirectory.FullName + "\\" + "release");

        static DirectoryInfo DataDirectory = new DirectoryInfo(ContentDirectory.FullName + "\\" + "project");

        static DirectoryInfo PluginDirectory = new DirectoryInfo(ContentDirectory.FullName + "\\" + "plugins");

        static DirectoryInfo ExtractDirectory = new DirectoryInfo(ContentDirectory.FullName + "\\" + "extract");

        static string GameFile;

        static bool IsGodEater2;

        static Stopwatch sw = Stopwatch.StartNew();

        static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> ProjectDataIndexMap = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        //static Dictionary<string, string> ProjectDataIndexMapBin = new Dictionary<string, string>();
        //static Dictionary<string,string> ProjectDataIndexMapBinPatch = new Dictionary<string, string>();
        //static Dictionary<string, string> ProjectDataIndexMapData = new Dictionary<string, string>();
        //static Dictionary<string, string> ProjectDataIndexMapConf = new Dictionary<string, string>();


        static Dictionary<string,string> GameDataMap = new Dictionary<string,string>();


        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }


        static void Main(string[] args)
        {
            Info($"RETAEDOG By:HaoJun0823 https://www.haojun0823.xyz/ Version:{Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            Info("If you encounter problems, please contact email@haojun0823.xyz and upload your error log.");
            Log.tw = File.CreateText(ContentDirectory.FullName + "\\retaedog_info.log");

            

            Info($"Get Root Directory:{RootDirectory.FullName}");
            Info($"Get Content Directory:{ContentDirectory.FullName}");
            Info($"Get Game Work Directory:{WorkDirectory.FullName}");
            Info($"Get Mod Data Directory:{DataDirectory.FullName}");
            Info($"Get Plugins Directory:{PluginDirectory.FullName}");

            if (!ContentDirectory.Exists)
            {
                var error = $"Cannot Found {ContentDirectory}, Please Copy Retaedog Content To Game Direcotry.";
                Error(error);
                Info($"Press Any Key To Continue!(Cost:{sw.ElapsedMilliseconds})");
                Console.ReadKey();
                throw new FileNotFoundException(error);

            }

            if (!SetGodEaterVersion())
            {
                var error = "Cannot Found GE2RB.exe Or GER.exe, Please Copy Retaedog To Game Direcotry.";
                Error(error);
                Info($"Press Any Key To Continue!(Cost:{sw.ElapsedMilliseconds})");
                Console.ReadKey();
                throw new FileNotFoundException(error);

            }

            //RootDirectory.Create();

            ContentDirectory.Create();
            WorkDirectory.Create();
            DataDirectory.Create();
            PluginDirectory.Create();

            Info($"Init Cost:{sw.ElapsedMilliseconds}ms.");

            if (args.Length != 0 && args[0].ToLower().Equals("install"))
            {
                Info($"Install Patch.");
                Log.tw = File.CreateText(ContentDirectory.FullName+ "\\retaedog_install.log");
                Install();
                Info("Install Done!");
                sw.Stop();
                Info($"Press Any Key To Continue!(Cost:{sw.ElapsedMilliseconds})");
                Console.ReadKey();
            }if(args.Length!= 0 && args[0].ToLower().Equals("extract"))
            {

            }
            else
            {
                Info($"Run Game.");
                RunGame(args);
            }
            Info($"RETAEDOG By:HaoJun0823 https://www.haojun0823.xyz/ Version:{Assembly.GetExecutingAssembly().GetName().Version.ToString()}");




        }


        static void Install()
        {
            WorkDirectory.Delete(true);
            
            Parallel.Invoke(() => { CreateProjectIndex(); }, () => { InitGameQpck(); });
            
            BuildGameQpck();
            File.WriteAllText(WorkDirectory.FullName + "\\retaedog.bin", RootDirectory.FullName);

            


        }

        static void SteamBuild()
        {

            var txt = WorkDirectory + "\\steam_appid.txt";

            if (IsGodEater2)
            {
                File.WriteAllText(txt,"438490");
            }
            else
            {
                File.WriteAllText(txt,"460870");
            }


            txt = RootDirectory + "\\steam_appid.txt";

            if (IsGodEater2)
            {
                File.WriteAllText(txt, "438490");
            }
            else
            {
                File.WriteAllText(txt, "460870");
            }

        }



        static void RunGame(string[] args)
        {
            CopyGodEater();
            SteamBuild();
            //CopyDll();
            List<string> list = new List<string>();


            list.Add(WorkDirectory + "\\data");
            list.Add(WorkDirectory + "\\bin.qpck");
            list.Add(WorkDirectory + "\\bin_patch.qpck");
            list.Add(WorkDirectory + "\\conf.qpck");
            list.Add(WorkDirectory + "\\data.qpck");
            list.Add(WorkDirectory + "\\xinput1_3.dll");
            list.Add(WorkDirectory + "\\steam_api.dll");
            list.Add(WorkDirectory + "\\retaedog.bin");


            foreach (var i in list)
            {
                if (File.Exists(i) || Directory.Exists(i))
                {
                    Info($"Vaild {i}");
                }
                else
                {
                    var error = $"Cannot Vaild {i}, Please \"retaedog.exe install\" To Install Mod Game.";
                    Error(error);
                    Info($"Press Any Key To Continue!(Cost:{sw.ElapsedMilliseconds})");
                    Console.ReadKey();
                    throw new FileNotFoundException(error);
                }
            }

            Process process = new Process();
            process.StartInfo.FileName = GameFile;
            process.StartInfo.WorkingDirectory = WorkDirectory.FullName;

            process.StartInfo.UseShellExecute = false;

            StringBuilder stringBuilder = new StringBuilder();
            foreach(var i in args)
            {
                stringBuilder.Append(i);
                stringBuilder.Append(' ');
            }
            
            process.StartInfo.Arguments = stringBuilder.ToString();


            Info($"Game Mod:{process.StartInfo.WorkingDirectory},Args:{process.StartInfo.Arguments}");
            
            process.Start();

            while (true)
            {
                try
                {
                    var time = process.StartTime;
                    break;
                }
                catch(Exception e) { 
                    Info("Waiting Game Running...");
                }
            }

                Info($"Game Running!");
                //Info($"Game Running! Console Close After 20s!");
                //Thread.Sleep(20000);

                //if (!SetWindowTextW(process.MainWindowHandle, GetTitle()))
                //{
                //    Info($"Set Title Wrong:{GetTitle()}");
                //}
                //else
                //{
                //    //Info($"Set Title:{GetTitle()}");
                //}
            

            
            


            sw.Stop();
            Info($"Game Starting!(Cost:{sw.ElapsedMilliseconds})");
            
        }

        static bool SetGodEaterVersion()
        {

            var result = false;

            var GE2 = RootDirectory.FullName + "\\" + "GE2RB.exe";
            var GE1 = RootDirectory.FullName + "\\" + "GER.exe";
            var GEC = ContentDirectory.FullName + "\\" + "GE.dat";

                        

            if (File.Exists(GE2))
            {
                Info($"Set Game Version = God Eater 2:{GE2}");
                IsGodEater2 = true;
                GameFile = GE2;
                result = true;
                
            }

            if (File.Exists(GE1))
            {
                Info($"Set Game Version = God Eater 1:{GE1}");
                IsGodEater2 = false;
                GameFile = GE1;
                result = true;
                
            }

            if (File.Exists(GEC))
            {
                Info($"Set Game Version = God Eater Custom:{GEC}");
                GameFile = GEC;
                result = true;
            }

            return result;




        }


        static void CopyGodEater()
        {
            //var new_gec = RootDirectory + "\\" + "GE.dat;

            var new_gec = WorkDirectory.FullName;

            if (IsGodEater2)
            {
                new_gec += "\\GE2RB.exe";
            }
            else
            {
                new_gec += "\\GER.exe";
            }



            File.Copy(GameFile, new_gec, true );
            Info($"Copy GameFile {GameFile} => {new_gec}");

            GameFile = new_gec;
            Info($"Set GameFile = {new_gec}");

            

        }

        static void CopyDll()
        {
            //return;
            var files = RootDirectory.GetFiles("*.dll",SearchOption.TopDirectoryOnly);

            //FileInfo steam_file = new FileInfo(RootDirectory.FullName+"\\steam_api.dll");
            //FileInfo xinput_file = new FileInfo(RootDirectory.FullName + "\\xinput1_3.dll");

            //FileInfo[] files = { steam_file, xinput_file };

            foreach(var i in files)
            {
                var target = WorkDirectory+"\\"+ i.Name;

                Info($"Copy {i.FullName} => {target}.");

                File.Copy(i.FullName, target,true);
            }

        }

        static void CopyPlugins()
        {

            DirectoryInfo[] dirs = PluginDirectory.GetDirectories("*",SearchOption.TopDirectoryOnly);

            
            foreach(var i in dirs)
            {

                

            }
            

            
            


        }



        static void CreateProjectIndex()
        {
            Info($"[Project]Build Project Index:{DataDirectory.FullName}");

            DirectoryInfo[] dirs = DataDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly);

            Info($"[Project]Project Patch Count:{dirs.Length}");

            Dictionary<Int64, DirectoryInfo> IndexDirectoryMap = new Dictionary<long, DirectoryInfo>();

            foreach ( DirectoryInfo dir in dirs )
            {
                Int64 order = Convert.ToInt64(dir.Name);
                Info($"[Project]Add {dir.Name} Order {order} ({dir.FullName})");
                IndexDirectoryMap.Add(order, dir);

                


            }

            var desc_map = IndexDirectoryMap.OrderByDescending( kv => kv.Key);


            ProjectDataIndexMap.TryAdd("bin.qpck",new ConcurrentDictionary<string, string>());
            ProjectDataIndexMap.TryAdd("bin_patch.qpck", new ConcurrentDictionary<string, string>());
            ProjectDataIndexMap.TryAdd("data.qpck", new ConcurrentDictionary<string, string>());
            ProjectDataIndexMap.TryAdd("conf.qpck", new ConcurrentDictionary<string, string>());

            foreach (var kv in desc_map) {


                Info($"[Project]Working {kv.Key}. ({kv.Value.FullName})");


                DirectoryInfo bin = new DirectoryInfo(kv.Value.FullName + "\\bin.qpck");
                DirectoryInfo bin_patch = new DirectoryInfo(kv.Value.FullName + "\\bin_patch.qpck");
                DirectoryInfo conf = new DirectoryInfo(kv.Value.FullName + "\\conf.qpck");
                DirectoryInfo data = new DirectoryInfo(kv.Value.FullName + "\\data.qpck");
                DirectoryInfo gamedata = new DirectoryInfo(kv.Value.FullName + "\\data");


                Info($"[Project]bin:{bin.FullName}");
                Info($"[Project]bin_patch:{bin_patch.FullName}");
                Info($"[Project]conf:{conf.FullName}");
                Info($"[Project]data:{data.FullName}");

                bin.Create();
                bin_patch.Create();
                conf.Create();
                data.Create();
                gamedata.Create();

                Parallel.Invoke(() => { BuildProjectIndex("bin.qpck", bin); }, () => { BuildProjectIndex("bin_patch.qpck", bin_patch); }, () => { BuildProjectIndex("conf.qpck",conf); }, () => { BuildProjectIndex("data.qpck", data); }, () => { BuildGameDataIndex(gamedata); });
                

            }

            BuildGameDataIndex(new DirectoryInfo(RootDirectory.FullName + "\\data")); //要把原版游戏文件复制进来。








        }

        static void BuildGameDataIndex(DirectoryInfo dir)
        {
            var files = dir.GetFiles("*.*", SearchOption.AllDirectories);
            Info($"[ProjectGameDataBuilder]{dir.FullName} Redirect Files:{files.Length+1}.");

            foreach(var file in files)
            {

                var path_key = file.FullName.Substring(dir.FullName.Length);
                Info($"[ProjectGameDataBuilder]FilePath:{path_key}.");

                if (GameDataMap.ContainsKey(path_key))
                {
                    Info($"[ProjectGameDataBuilder]Already Have :{path_key}.");
                }
                else
                {
                    Info($"[ProjectGameDataBuilder]Add :{path_key}, Path:{file.FullName}");
                    GameDataMap.Add(path_key, file.FullName);
                }


                //if(GameDataMap.ContainsKey(file.FullName))
            }

        }

        static void BuildProjectIndex(string qpck_name,DirectoryInfo dir)
        {

            var files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            Info($"[ProjectBuilder]{qpck_name} {dir.FullName} Redirect Files:{files.Length}.");


            foreach(var file in files)
            {


                var filename = Path.GetFileNameWithoutExtension(file.Name);
                Info($"[ProjectBuilder]{qpck_name} FileName:{filename}.");

                if (ProjectDataIndexMap[qpck_name].ContainsKey(filename))
                {
                    Info($"[ProjectBuilder]{qpck_name} Already Have :{filename}.");
                }
                else
                {

                    if (ProjectDataIndexMap[qpck_name].TryAdd(filename,file.FullName))
                    {
                        Info($"[ProjectBuilder]{qpck_name} Add :{filename}, Path:{file.FullName}");
                    }
                    else
                    {
                        throw new Exception($"Add Error:{filename},{file.FullName}");
                    }
                    
                }
            }



        }

       


        static void InitGameQpck()
        {

            Parallel.Invoke( () => { Qpck.bin = new Qpck(new FileInfo(RootDirectory.FullName+"\\bin.qpck")); }, () => { Qpck.bin_patch = new Qpck(new FileInfo(RootDirectory.FullName + "\\bin_patch.qpck")); }, () => { Qpck.data = new Qpck(new FileInfo(RootDirectory.FullName + "\\data.qpck")); }, () => { Qpck.conf = new Qpck(new FileInfo(RootDirectory.FullName + "\\conf.qpck")); });

            

        }

        static void InitExtracterQpck()
        {

            Parallel.Invoke(() => { QpckExtracter.bin = new Qpck(new FileInfo(WorkDirectory.FullName + "\\bin.qpck")); }, () => { QpckExtracter.bin_patch = new Qpck(new FileInfo(WorkDirectory.FullName + "\\bin_patch.qpck")); }, () => { QpckExtracter.data = new Qpck(new FileInfo(WorkDirectory.FullName + "\\data.qpck")); }, () => { QpckExtracter.conf = new Qpck(new FileInfo(WorkDirectory.FullName + "\\conf.qpck")); });



        }

        static void BuildGameQpck()
        {
            Parallel.Invoke(() => { BuildGameQpck(Qpck.bin); }, () => { BuildGameQpck(Qpck.bin_patch); }, () => { BuildGameQpck(Qpck.data); }, () => { BuildGameQpck(Qpck.conf); }, () => { CopyData(); CopyDll(); SteamBuild(); });
        }

        static void CopyData()
        {

            Parallel.ForEach<KeyValuePair<string, string>>(GameDataMap, kv =>
            {
                var target = WorkDirectory + "\\data";
                Info($"[GameDataRirector]Copy {kv.Value} To {target}{kv.Key}");

                Directory.CreateDirectory(Path.GetDirectoryName($"{target}{kv.Key}"));
                File.Copy(kv.Value, $"{target}{kv.Key}", true);


            });
            


        }


        //原版QPCK做参数，写到release里
        static void BuildGameQpck(Qpck qpck)
        {
            var file = WorkDirectory.FullName + "\\" + qpck.Name;

            Info($"[GameBuilder]Build:{file}");

            

            if (ProjectDataIndexMap[qpck.Name].Count > 0)
            {
                QpckWriter writer = new QpckWriter(new FileInfo(file), qpck);
                writer.Merge(ProjectDataIndexMap[qpck.Name]);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.Copy(qpck.File.FullName,file,true);
                Info($"[GameBuilder]Copy:{qpck.File.FullName} To {file}");
            }

            

        }

        //XP不能用
        static void MakeSymbolLink(string origin, string target)
        {

            Directory.CreateDirectory(Path.GetDirectoryName(target));

            CreateSymbolicLink(origin,target,0);

            Info($"[VirtualSymbol] {origin} <=> {target}");

        }


    }





    internal class Qpck
    {

        public static Qpck bin;
        public static Qpck bin_patch;
        public static Qpck conf;
        public static Qpck data;

        public struct ContentIndex
        {
            public Int64 offset;
            public Int64 hash;
            public Int32 size;

        }

        public static int Magic = 0x37402858;
        public int Count = 0;

        public string Name;

        public Dictionary<string,  ContentIndex> ContentIndexMap = new Dictionary<string, ContentIndex>();

        public FileInfo File;
        BinaryReader Reader;





        public Qpck(FileInfo file)
        {
            this.File = file;
            this.Name = file.Name;

            if (File.Exists)
            {
                Reader = new BinaryReader(File.OpenRead());

                GetFileHeader();
                BuildIndexMap();
            }
            else
            {
                Info($"Warning:{file.FullName} Is Not Exists!");
            }



        }

        public void BuildIndexMap()
        {
            Reader.BaseStream.Seek(8, SeekOrigin.Begin);

            Info($"[QPCK]Seek Cursor To {Reader.BaseStream.Position}");

            for(int i = 0; i < this.Count; i++)
            {
                
                Int64 offset = Reader.ReadInt64();
                Int64 hash = Reader.ReadInt64();
                Int32 size = Reader.ReadInt32();



                string filename = (i + 1).ToString("D8") + "_" + hash.ToString("X16");

                
                Info($"[{this.Name}]No.{i}/{this.Count} File Offset:{offset},Size:{size},Hash:{hash},SetName={filename}");


                ContentIndex map_data= new ContentIndex();

                map_data.offset = offset;
                map_data.hash = hash;
                map_data.size = size;

                ContentIndexMap.Add(filename,map_data);

            }


        }

        

        public void GetFileHeader()
        {

            var read_header = Reader.ReadInt32();
            var read_count = Reader.ReadInt32();


            if(read_header != Magic)
            {
                var error = $"[QPCK]{this.Name} Header error:{read_header}!={Magic}";
                Error(error);
                throw new FileLoadException(error);
            }

            if(read_count <= 0)
            {
                var error = $"[QPCK]{this.Name} Count error:{read_header}";
                Error(error);
                throw new FileLoadException(error);
            }

            this.Count = read_count;

            Info($"[QPCK]{this.Name} Have {read_count} Files.");


        }

        public byte[] GetFileData(ContentIndex index)
        {

            Reader.BaseStream.Seek(index.offset, SeekOrigin.Begin);

            Info($"[QPCK]Seek Cursor To {Reader.BaseStream.Position}");

            return Reader.ReadBytes(index.size);

        }



    }

    internal class QpckWriter
    {


        BinaryWriter Writer;

        long data_cusror;

        Qpck originQpck;

        List<long> header = new List<long>();

        FileInfo targetFile;


        public QpckWriter(FileInfo file,Qpck qpck)
        {

            originQpck = qpck;
            targetFile = file;
            Info($"[QpckWriter]{file.Name} Will Write {qpck.Count} Files.");

            if (file.Exists)
            {
                file.Delete();
            }

            Directory.CreateDirectory(Path.GetDirectoryName(file.FullName));

            Writer = new BinaryWriter(file.OpenWrite());

            Writer.Write(Qpck.Magic);
            Writer.Write(qpck.Count);
            

            for(int i = 0; i < qpck.Count; i++)
            {
                header.Add(Writer.BaseStream.Position);
                Writer.Write(0L);//offset
                Writer.Write(0L);//hash
                Writer.Write(0);//size
            }

            data_cusror = Writer.BaseStream.Position;
            Info($"[QpckWriter]{file.Name} Header Length {data_cusror - 8}.");

        }

        public void Merge(ConcurrentDictionary<string,string> map)
        {

            long header_cursor = 8;

            long file_count = 0;
            foreach(var kv in originQpck.ContentIndexMap)
            {


                header_cursor = 8+ (file_count*20);
                Info($"[QpckWriter]{targetFile.Name} Header At{header_cursor} File Count {file_count}");

                byte[] data;

                if ((map.ContainsKey(kv.Key)))
                {
                    Info($"New:{map[kv.Key]}|{kv.Key} => {targetFile.FullName}|{kv.Key}");

                    data = File.ReadAllBytes(map[kv.Key]);
                }
                else
                {
                    Info($"Old:{originQpck.File.FullName}|{kv.Key} => {targetFile.FullName}|{kv.Key}");
                    data = originQpck.GetFileData(kv.Value);
                }
                
                Writer.BaseStream.Seek(header_cursor, SeekOrigin.Begin);
                Info($"[QpckWriter]Seek Cursor To {Writer.BaseStream.Position},Write:{data_cusror},{kv.Value.hash},{data.Length}");
                Writer.Write(data_cusror);
                Writer.Write(kv.Value.hash);
                Writer.Write(data.Length);
                Writer.BaseStream.Seek(data_cusror, SeekOrigin.Begin);
                Info($"[QpckWriter]Seek Cursor To {Writer.BaseStream.Position},Write Bytes Data.");
                Writer.Write(data);
                data_cusror = Writer.BaseStream.Position;

                Info($"[{originQpck.Name}]Current Progress:{file_count+1}/{originQpck.ContentIndexMap.Count}");

                file_count++;


            }




        }





    }

    internal class Log
    {


        public static TextWriter tw;
        

        public static void Info(string str)
        {

            string log = $"[Thead:{Thread.CurrentThread.ManagedThreadId}/INFO]{str}";

            Console.WriteLine(log);

            if(tw!= null)
            {
                tw.WriteLine(str);
                tw.Flush();
            }

            

        }


        public static void Error(string str)
        {
            string log = $"[Thead:{Thread.CurrentThread.ManagedThreadId}/ERROR]{str}\nPlease contact email@haojun0823.xyz and upload your error log.";

            Console.WriteLine(log);

            if (tw != null)
            {
                tw.WriteLine(str);
                tw.Flush();
            }
        }


    }

    internal class QpckExtracter
    {

        public static Qpck bin;
        public static Qpck bin_patch;
        public static Qpck conf;
        public static Qpck data;

        //static void ExtractNewFile(Qpck origin, Qpck target,Qpck )
        //{

        //    //if (origin.ContentIndexMap.Count != target.ContentIndexMap.Count)
        //    //{
        //    //    Error($"Origin {origin.ContentIndexMap.Count} != Target {target.ContentIndexMap.Count} (Index Count Error)");
        //    //    throw new FileLoadException($"Origin {origin.ContentIndexMap.Count} != Target {target.ContentIndexMap.Count} (Index Count Error)");
                
        //    //}


        //}


    }

}
