using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GECV;
using MiniExcelLibs;
using ProtoBuf.Data;

namespace CODEEATER
{
    internal class Program
    {

        public static FileInfo SourceFile;
        public static DirectoryInfo TargetDirectory;

        public static LinkedList<string> QPCKFileList = new LinkedList<string>();

        public static DataTable Blz4Table;
        public static DataTable GnfTable;
        public static DataTable PresTable;
        public static Dictionary<string, DataTable> PresTableMap;
        public static DataTable GlobalPresTable;
        



        static void Main(string[] args)
        {
            

            Log.Info("CODE EATER 噬神者资源映射 解包器 BY 兰德里奥（HaoJun0823）");
            Log.Info("https://blog.haojun0823.xyz/");
            Log.Info("https://github.com/HaoJun0823/GECV");
            Log.Info("参考：https://github.com/mhvuze/GEUndub/ By mhvuze");
            Log.Info(@"License: MiniExcel,Zlib.Net,Protobuf-net,Protobuf-net-data");



            if (args.Length >= 2)
            {
                Log.Info(string.Format("QPCK文件：{0}", args[0]));
                Log.Info(string.Format("解包目录：{0}", args[1]));

                if (File.Exists(args[0]))
                {
                    SourceFile = new FileInfo(args[0]);

                    TargetDirectory = new DirectoryInfo(args[1]);

                    GECV.Log.SetLogFolder(TargetDirectory);


                    TargetDirectory.Create();
                    Directory.CreateDirectory(TargetDirectory.FullName + "_BASE\\");
                    Directory.CreateDirectory(TargetDirectory.FullName + "_EXTRA\\");
                    Directory.CreateDirectory(TargetDirectory.FullName + "_UNPACK\\");
                    Directory.CreateDirectory(TargetDirectory.FullName + "_UNPACK_RES\\");
                    Directory.CreateDirectory(TargetDirectory.FullName + "_UNPACK_RES_REAL\\");
                    Directory.CreateDirectory(TargetDirectory.FullName + "_UNPACK_PRES_REAL\\");
                    Directory.CreateDirectory(TargetDirectory.FullName + "_UNPACK_QPCK_REAL\\");
                    Directory.CreateDirectory(TargetDirectory.FullName + "_UNPACK_QPCK_REAL_TYPE\\");
                    Directory.CreateDirectory(TargetDirectory.FullName + "_CUSTOM_GNF\\");

                    Console.WriteLine("开始处理文件");

                    int input = -1;
                    while (input != 0)
                    {
                        Log.Info($"处理{args[0]}的文件到{args[1]}");
                        Log.Info($"1.处理QPCK");
                        Log.Info($"2.处理PRES");
                        Log.Info($"3.处理BLZ4");
                        Log.Info($"4.处理GNF");
                        Log.Info($"5.读取PRES的src_voiceevent_text_字符串（测试用，慢，请用6）");
                        Log.Info($"6.多线程读取PRES的src_voiceevent_text_字符串");
                        Log.Info($"7.多线程读取输入的字符串查询PRES");
                        Log.Info($"8.处理存在_CUSTOM_GNF的自定义GNF");
                        Log.Info($"9.根据MAGIC给_UNPACK_QPCK_REAL分类到_UNPACK_QPCK_REAL_TYPE");
                        Log.Info($"10.处理RES");
                        Log.Info($"11.对_BASE的TR2进行分类");
                        Log.Info($"12.处理PRES第七分区");
                        Log.Info($"0.退出");

                        try
                        {
                            input = Convert.ToInt32(Console.ReadLine());
                        }
                        catch (Exception ex)
                        {
                            input = -1;
                            Log.Error(ex.ToString());
                            Log.Error("你输入了个啥？");
                        }




                        switch (input)
                        {
                            case 1:
                                ProcessQPCK();
                                break;
                            case 2:
                                ProcessPRES();
                                break;
                            case 3:
                                ProcessBLZ4();
                                break;
                            case 4:
                                ProcessGNF();
                                break;
                            case 5:
                                ProcessPresSRC(Utils.GetStringBytes("src_voiceevent_text_"));
                                break;
                            case 6:
                                ProcessPresSRCParallel(Utils.GetStringBytes("src_voiceevent_text_"));
                                break;
                            case 7:
                                Log.Info($"请输入要搜索的内容，不要输入宽字符（2字节字符）：");
                                string search = Console.ReadLine();
                                ProcessPresSRCParallel(Utils.GetStringBytes(search));
                                break;
                            case 8:
                                ProcessCustomGNF();
                                break;
                            case 9:
                                ProcessQPCKRealMagic();
                                break;
                            case 10:
                                ProcessRES();
                                break;
                            case 11:
                                ProcessTR2Type();
                                break;
                            case 12:
                                ProcessPRES_7();
                                break;
                            case 0:
                                input = 0;
                                break;
                            default:
                                input = -1;
                                break;
                        }



                    }





                }
                else
                {
                    Log.Error("qpck文件不存在啊！");
                }



            }
            else
            {
                Log.Error("错误，你需要两个输入：1.原始qpck文件，2.解包后目标文件夹地址。");
            }

            //Utils.WriteListToFile(Log.LogRecord,TargetDirectory.FullName+"\\CODEEATER.log");
            Log.flush();
            Log.Info("按任意键退出……");
            Console.ReadKey();


        }



        static void ProcessTR2Type()
        {

            var files = Directory.GetFiles(TargetDirectory.FullName + "_BASE\\", "*.tr2",SearchOption.AllDirectories);

            DirectoryInfo dir = new DirectoryInfo(TargetDirectory.FullName + "_BASE_TR2\\");

            Log.Info($"有{files.Length}个文件，分类到{dir.FullName}");


            Parallel.ForEach<string>(files, (str) => {

                Log.Info($"开始读取{str}");
                using (FileStream fs = File.OpenRead(str))
                {
                    using(BinaryReader br = new BinaryReader(fs))
                    {
                        br.BaseStream.Seek(8,SeekOrigin.Begin);

                        Log.Info($"位置定位在{br.BaseStream.Position}");
                        string result = Utils.readNullterminated(br);
                        Log.Info($"结果：{result}，指针结束于:{br.BaseStream.Position}");
                        string target = str.Replace("_BASE", "_BASE_TR2\\" + result + "\\");
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        File.Copy(str, target,true);
                    }

                    



                }


            
            });




        }

        static void ProcessQPCKRealMagic()
        {
            //Directory.CreateDirectory(TargetDirectory.FullName + "_UNPACK_QPCK_REAL\\");
            //Directory.CreateDirectory(TargetDirectory.FullName + "_UNPACK_QPCK_REAL_TYPE\\");


            foreach(var kv in Define.extension_ext)
            {
                Log.Info($"目前存储的魔法码：{kv.Key.ToString("X8")}={kv.Value}");
            }

            //Console.ReadLine();

            var files = Directory.GetFiles(TargetDirectory.FullName + "_UNPACK_QPCK_REAL\\", "*.*", SearchOption.AllDirectories);

            Log.Info($"获取到：{files.Length}个文件。");


            Parallel.ForEach<string>(files, (str) =>
            {

                Utils.ReadMagicAndSaveByType(File.ReadAllBytes(str),Path.GetFileName(str), TargetDirectory.FullName + "_UNPACK_QPCK_REAL_TYPE\\");





            });



        }


        static void ProcessPresSRCParallel(byte[] DEFAULT_STR)
        {
            DataTable table = new DataTable();

            table.Columns.Add("file_name", typeof(string));
            table.Columns.Add("text_offset", typeof(Int32));
            table.Columns.Add("file_text", typeof(string));




            DirectoryInfo directory = new DirectoryInfo(TargetDirectory.FullName + "_BASE\\");

            var files = directory.GetFiles("*.pres", SearchOption.AllDirectories);
            Parallel.For(0, files.Length, (i) =>
            {

                Log.Info($"正在处理{i}/{files.Length}：{files[i].FullName}");

                //byte[] DEFAULT_STR = {  0x73, 0x72, 0x63, 0x5F };

                int index = 0;



                using (BinaryReader br = new BinaryReader(files[i].OpenRead()))
                {

                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        if (index == DEFAULT_STR.Length)
                        {

                            lock (table)
                            {
                            //Log.Info($" 发现目标 src_");
                            DataRow dr = table.NewRow();
                            dr["file_name"] = files[i].Name;
                            dr["text_offset"] = br.BaseStream.Position - (DEFAULT_STR.Length-1)*-1;
                            br.BaseStream.Seek((DEFAULT_STR.Length-1)*-1, SeekOrigin.Current);
                            Log.Info($" 重定位指针到{br.BaseStream.Position}");
                            string str = Utils.readNullterminated(br);
                            dr["file_text"] = str;
                            Log.Info($" 读取到有效的{str}");
                            table.Rows.Add(dr);
                            index = 0;
                            }
                        }
                        else
                        {
                            byte r = br.ReadByte();


                            if (r == DEFAULT_STR[index])
                            {
                                //Log.Info($" 检测到第{index + 1}个合适的字符：{read}");
                                index++;
                            }
                            else
                            {
                                //Log.Info($" 检测到第{index + 1}个字符不合法，跳出：{read}");
                                index = 0;
                            }

                        }
                    }






                }


            });

            FileInfo excel = new FileInfo(TargetDirectory + "src_parallel.xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, table);
            }

        }

        static void ProcessPresSRC(byte[] DEFAULT_STR)
        {
            DataTable table = new DataTable();

            table.Columns.Add("file_name", typeof(string));
            table.Columns.Add("text_offset", typeof(Int32));
            table.Columns.Add("file_text", typeof(string));




            DirectoryInfo directory = new DirectoryInfo(TargetDirectory.FullName + "_BASE\\");

            var files = directory.GetFiles("*.pres", SearchOption.AllDirectories);
            Log.Info($"处理全部文件，一共有{files.Length}。");
            for (int i = 0; i < files.Length; i++)
            {

                Log.Info($"正在处理{i}/{files.Length}：{files[i].FullName}");

                //byte[] DEFAULT_STR = { 0x73, 0x72, 0x63, 0x5F };

                int index = 0;


                Log.Info($"文件大小：{files[i].Length},{files[i].Length.ToString("X")}");

                using (BinaryReader br = new BinaryReader(files[i].OpenRead()))
                {

                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        if (index == DEFAULT_STR.Length)
                        {
                            //Log.Info($" 发现目标 src_");
                            DataRow dr = table.NewRow();
                            dr["file_name"] = files[i].Name;
                            dr["text_offset"] = br.BaseStream.Position - (DEFAULT_STR.Length - 1) * -1;
                            br.BaseStream.Seek((DEFAULT_STR.Length - 1) * -1, SeekOrigin.Current);
                            Log.Info($" 重定位指针到{br.BaseStream.Position}");
                            string str = Utils.readNullterminated(br);
                            dr["file_text"] = str;
                            Log.Info($" 读取到有效的{str}");
                            table.Rows.Add(dr);
                            index = 0;
                        }
                        else
                        {



                            byte r = br.ReadByte();
                            


                            if (r == DEFAULT_STR[index])
                            {
                                Log.Info($" 检测到第{index + 1}个合适的字符：{(char)r}，位于:{br.BaseStream.Position.ToString("X")}");
                                index++;
                                
                            }
                            else
                            {
                                Log.Info($" 检测到第{index + 1}个字符不合法，跳出：{(char)r}，位于:{br.BaseStream.Position.ToString("X")}");
                                index = 0;
                            }

                        }
                    }






                }


            }


            FileInfo excel = new FileInfo(TargetDirectory + "src_.xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, table);
            }
        }


        static void ProcessCustomGNF()
        {
            GnfTable = new DataTable();

            GnfTable.Columns.Add("file_name", typeof(string));
            GnfTable.Columns.Add("file_desc", typeof(string));
            GnfTable.Columns.Add("file_size", typeof(Int32));
            GnfTable.Columns.Add("gnf_count", typeof(Int32));
            GnfTable.Columns.Add("dds_index", typeof(Int32));
            GnfTable.Columns.Add("dds_size", typeof(Int32));
            GnfTable.Columns.Add("dds_magic", typeof(Int32));
            GnfTable.Columns.Add("dds_name", typeof(string));
            GnfTable.Columns.Add("dds_path", typeof(string));



            DirectoryInfo qpck_directory = new DirectoryInfo(TargetDirectory.FullName + "_CUSTOM_GNF\\");


            var files = qpck_directory.GetFiles("*.gnf", SearchOption.TopDirectoryOnly);
            Log.Info($"处理_CUSTOM_GNF的gnf文件，一共有{files.Length}。");

            for (int i = 0; i < files.Length; i++)
            {


                Log.Info($"正在处理{i}/{files.Length}：");
                GNF_NEW(files[i]);





            }

            FileInfo excel = new FileInfo(TargetDirectory + "gnf_custom.xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, GnfTable);
            }

            using (FileStream fs = File.OpenWrite(TargetDirectory + "\\gnf_custom.bin"))
            {
                using (IDataReader dr = GnfTable.CreateDataReader())
                {
                    DataSerializer.Serialize(fs, dr);
                }
            }

        }

        static void ProcessGNF()
        {
            GnfTable = new DataTable();

            GnfTable.Columns.Add("file_name", typeof(string));
            GnfTable.Columns.Add("file_desc", typeof(string));
            GnfTable.Columns.Add("file_size", typeof(Int32));
            GnfTable.Columns.Add("gnf_count", typeof(Int32));
            GnfTable.Columns.Add("dds_index", typeof(Int32));
            GnfTable.Columns.Add("dds_size", typeof(Int32));
            GnfTable.Columns.Add("dds_magic", typeof(Int32));
            GnfTable.Columns.Add("dds_name", typeof(string));
            GnfTable.Columns.Add("dds_path", typeof(string));



            DirectoryInfo pres_directory = new DirectoryInfo(TargetDirectory.FullName + "_UNPACK_PRES_REAL\\");
            DirectoryInfo qpck_directory = new DirectoryInfo(TargetDirectory.FullName + "_UNPACK_QPCK_REAL\\");

            var files = pres_directory.GetFiles("*.gnf", SearchOption.AllDirectories);
            Log.Info($"处理解包gnf文件，一共有{files.Length}。");
            for (int i = 0; i < files.Length; i++)
            {

                Log.Info($"正在处理{i}/{files.Length}：");
                GNF_NEW(files[i]);

            }

            files = qpck_directory.GetFiles("*.gnf", SearchOption.AllDirectories);
            Log.Info($"处理qpck的gnf文件，一共有{files.Length}。");

            for (int i = 0; i < files.Length; i++)
            {


                Log.Info($"正在处理{i}/{files.Length}：");
                GNF_NEW(files[i]);





            }

            FileInfo excel = new FileInfo(TargetDirectory + "gnf_new.xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, GnfTable);
            }


            using (FileStream fs = File.OpenWrite(TargetDirectory + "\\gnf_new.bin"))
            {
                using (IDataReader dr = GnfTable.CreateDataReader())
                {
                    DataSerializer.Serialize(fs, dr);
                }
            }


        }




        static void GNF_NEW(FileInfo file)
        {

            

            //dr["file_desc"] = "？";

            if (file.Length <= 3)
            {
                Log.Info($"跳过无用文件。");
                File.WriteAllText(file.FullName + ".null.log", $"极小长度文件：{file.Length}");
                //dr["file_name"] = file.Name;
                //dr["file_size"] = file.Length;
                //dr["file_desc"] = $"极小长度文件：{file.Length}";

                return;
            }


            using (BinaryReader br = new BinaryReader(file.OpenRead()))
            {

                Log.Info($"处理GNF(NEW):{file.FullName}，大小{file.Length}");

                int number = br.ReadInt32();






                Log.Info($"有{number}个dds");

                int[] size = new int[number];

                for (int i = 0; i < number; i++)
                {
                    size[i] = br.ReadInt32();
                    Log.Info($"第{i + 1}个dds大小为:{size[i]}");
                }


                for (int i = 0; i < size.Length; i++)
                {

                    DataRow dr = GnfTable.NewRow();
                    dr["file_name"] = file.FullName;
                    dr["file_size"] = file.Length;
                    dr["file_desc"] = $"正常";

                    dr["gnf_count"] = number;
                    dr["dds_index"] = i;
                    dr["dds_size"] = size[i];


                    int dds_magic = br.ReadInt32();
                    dr["dds_magic"] = dds_magic;

                    br.BaseStream.Position = br.BaseStream.Position - 4;

                    if (dds_magic != 542327876)
                    {
                        Log.Info($"     非法的GNF文件：DDS存储大小：{size[i]}，DDS魔术码：{dds_magic}。");
                        File.WriteAllText(file.FullName + ".err.log", $"非法的GNF文件：DDS存储大小：{size}，DDS魔术码：{dds_magic}（DDS=542327876）。");
                        byte[] bytes = br.ReadBytes(size[i]);
                        File.WriteAllBytes(file.FullName + i + ".err.dds", bytes);
                        //dr["file_desc"] = $"非法的GNF文件：DDS存储大小：{size}，DDS魔术码：{dds_magic}（DDS=542327876）。";
                        //dr["dds_name"] = file.Name + i + ".err.dds";
                        //dr["dds_path"] = file.FullName + i + ".err.dds";
                    }
                    else
                    {
                        byte[] bytes = br.ReadBytes(size[i]);

                        if (!Directory.Exists(file.DirectoryName))
                        {
                            Directory.CreateDirectory(file.DirectoryName);
                            Log.Info($"创建需要的目录{file.DirectoryName}");
                        }


                        File.WriteAllBytes(file.FullName + "_" + i + "_.dds", bytes);
                        Log.Info($"     GDDS存储大小：{size[i]}，DDS魔术码：{dds_magic}，保存在:{file.FullName}_{i}_.dds。");
                        dr["file_desc"] = $"GDDS存储大小：{size[i]}，DDS魔术码：{dds_magic}，保存在:{file.FullName}_{i}_.dds。";
                        dr["dds_name"] = $"{file.Name}_{i}_.dds";
                        dr["dds_path"] = $"{file.FullName}_{i}_.dds";
                    }
                    GnfTable.Rows.Add(dr);
                }



                
















            }




        }

        static void ProcessQPCK()
        {
            DataTable QpckTable = new DataTable();
            QpckTable.Columns.Add("offset", typeof(Int64));
            QpckTable.Columns.Add("hash", typeof(Int64));
            QpckTable.Columns.Add("size", typeof(Int32));
            QpckTable.Columns.Add("file_sha256", typeof(string));
            QpckTable.Columns.Add("file_magic", typeof(UInt32));
            QpckTable.Columns.Add("file_magic_desc", typeof(string));
            QpckTable.Columns.Add("file_name", typeof(string));


            using (BinaryReader br = new BinaryReader(SourceFile.Open(FileMode.Open)))
            {

                int magic = br.ReadInt32();
                int count = 0;
                if (magic != Define.QPCK_MAGIC)
                {
                    Log.Expection();
                    throw new Exception(string.Format("QPCK文件校验错误：头{0}应该是{1}！", magic, Define.QPCK_MAGIC));
                }
                else
                {
                    Log.Pass();
                    Log.Info(string.Format("QPCK文件校验结果：读取结果{0}等于{1}。", magic, Define.QPCK_MAGIC));





                }

                Log.Info(string.Format("文件总数{0}", count = br.ReadInt32()));

                long base_offset = 0;
                for (int i = 0; i < count; i++)
                {

                    long offset = br.ReadInt64();
                    long hash = br.ReadInt64();
                    int size = br.ReadInt32();
                    Log.Info($"第{i + 1}/{count}个文件位于偏移{offset}，文件大小：{size}字节，哈希校验：{hash}");

                    base_offset = br.BaseStream.Position;

                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                    uint type_magic = br.ReadUInt32();
                    Log.Info($" 文件魔术码：UINT{type_magic}");

                    br.BaseStream.Seek(-4, SeekOrigin.Current);

                    byte[] bytes = br.ReadBytes(size);
                    string filename = (i + 1).ToString("D8") + "_" + hash.ToString("X16") + Define.GetExtension(type_magic);

                    QPCKFileList.AddLast(filename);

                    string sha256 = CryptUtils.GetSHA256HashFromBytes(bytes);

                    File.WriteAllBytes(TargetDirectory.FullName + "_BASE\\" + filename, bytes);


                    DataRow dr = QpckTable.NewRow();

                    dr["offset"] = offset;
                    dr["hash"] = hash;
                    dr["size"] = size;
                    dr["file_sha256"] = sha256;
                    dr["file_magic"] = type_magic;
                    dr["file_magic_desc"] = Define.GetExtension(type_magic);
                    dr["file_name"] = filename;

                    QpckTable.Rows.Add(dr);

                    Log.Info($" 提取{i + 1}/{count}到{TargetDirectory.FullName}_BASE\\{filename}，大小{bytes.LongLength}字节。");

                    br.BaseStream.Position = base_offset;

                }

                FileInfo excel = new FileInfo(TargetDirectory + "qpck.xlsx");
                if (excel.Exists)
                {

                    excel.Delete();
                }

                using (var stream = excel.OpenWrite())
                {
                    MiniExcel.SaveAs(stream, QpckTable);
                }


            }


        }

        static void ProcessRES()
        {

            PresTableMap = new Dictionary<string, DataTable>();

            PresTable = new DataTable();


            PresTable.Columns.Add("file_name", typeof(string));
            PresTable.Columns.Add("file_path", typeof(string));
            PresTable.Columns.Add("file_magic", typeof(Int32));
            PresTable.Columns.Add("file_size", typeof(Int32));
            PresTable.Columns.Add("magic_1", typeof(Int32));
            PresTable.Columns.Add("magic_2", typeof(Int32));
            PresTable.Columns.Add("magic_3", typeof(Int32));
            PresTable.Columns.Add("offset_data", typeof(Int32));
            PresTable.Columns.Add("count_set", typeof(Int32));

            PresTable.Columns.Add("set_index", typeof(Int32));
            PresTable.Columns.Add("set_offset", typeof(Int32));
            PresTable.Columns.Add("set_length", typeof(Int32));

            PresTable.Columns.Add("set_data_3_start_offset", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_count", typeof(Int32));

            PresTable.Columns.Add("set_data_3_file_index", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_address", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_offset", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_offset_real", typeof(string));
            PresTable.Columns.Add("set_data_3_size", typeof(Int32));
            PresTable.Columns.Add("set_data_3_size_16", typeof(Int32));
            PresTable.Columns.Add("set_data_3_name_off_file", typeof(Int32));
            PresTable.Columns.Add("set_data_3_name_elements_file", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_unk1", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_unk2", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_unk3", typeof(Int32));
            PresTable.Columns.Add("set_data_3_usize", typeof(Int32));

            PresTable.Columns.Add("set_data_3_name", typeof(string));
            PresTable.Columns.Add("set_data_3_ext", typeof(string));
            PresTable.Columns.Add("set_data_3_folder", typeof(string));
            PresTable.Columns.Add("set_data_3_complete", typeof(string));

            PresTable.Columns.Add("set_data_3_file_magic", typeof(string));

            PresTable.Columns.Add("set_data_3_file_sha", typeof(string));

            GlobalPresTable = PresTable.Clone();

            DirectoryInfo base_directory = new DirectoryInfo(TargetDirectory.FullName + "_UNPACK\\");
            DirectoryInfo target_directory = new DirectoryInfo(TargetDirectory.FullName + "_UNPACK_RES\\");


            var files = base_directory.GetFiles("*.res", SearchOption.AllDirectories);
            Log.Info($"处理RES文件，一共有{files.Length}。");
            int count = 0;
            for (int i = 0; i < files.Length; i++)
            {

                Log.Info($"正在处理{i}/{files.Length}：");
                PRES(files[i], target_directory.FullName);
                count++;

                if (count == 10000)
                {

                    int file_number = i + 1;

                    FileInfo excelT = new FileInfo(TargetDirectory + "res-" + file_number + ".xlsx");
                    if (excelT.Exists)
                    {

                        excelT.Delete();
                    }

                    using (var stream = excelT.OpenWrite())
                    {
                        MiniExcel.SaveAs(stream, PresTable);
                    }



                    GlobalPresTable.Merge(PresTable);
                    PresTable.Rows.Clear();

                    count = 0;
                }

            }



            FileInfo excel = new FileInfo(TargetDirectory + "res-" + files.Length + ".xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, PresTable);
            }

            GlobalPresTable.Merge(PresTable);
            PresTable.Rows.Clear();

            count = 0;

            foreach (var kv in PresTableMap)
            {
                FileInfo excelI = new FileInfo(TargetDirectory + "res_" + kv.Key + "_.xlsx");
                if (excelI.Exists)
                {

                    excelI.Delete();
                }

                using (var stream = excelI.OpenWrite())
                {
                    MiniExcel.SaveAs(stream, kv.Value);
                }
            }


            FileInfo global = new FileInfo(TargetDirectory + "res.bin");

            if (global.Exists)
            {
                global.Delete();
            }






            using (var stream = global.OpenWrite())
            {

                using (IDataReader dr = GlobalPresTable.CreateDataReader())
                {
                    DataSerializer.Serialize(stream, dr);
                }




            }


            //FileInfo global2 = new FileInfo(TargetDirectory + "pres.xml");

            //if (global2.Exists)
            //{
            //    global2.Delete();
            //}


            //using (var stream = global2.OpenWrite())
            //{


            //    GlobalPresTable.WriteXml(stream);



            //}


        }

        static void ProcessPRES()
        {

            PresTableMap = new Dictionary<string, DataTable>();

            PresTable = new DataTable();


            PresTable.Columns.Add("file_name", typeof(string));
            PresTable.Columns.Add("file_path", typeof(string));
            PresTable.Columns.Add("file_magic", typeof(Int32));
            PresTable.Columns.Add("file_size", typeof(Int32));
            PresTable.Columns.Add("magic_1", typeof(Int32));
            PresTable.Columns.Add("magic_2", typeof(Int32));
            PresTable.Columns.Add("magic_3", typeof(Int32));
            PresTable.Columns.Add("offset_data", typeof(Int32));
            PresTable.Columns.Add("count_set", typeof(Int32));

            PresTable.Columns.Add("set_index", typeof(Int32));
            PresTable.Columns.Add("set_offset", typeof(Int32));
            PresTable.Columns.Add("set_length", typeof(Int32));

            PresTable.Columns.Add("set_data_3_start_offset", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_count", typeof(Int32));

            PresTable.Columns.Add("set_data_3_file_index", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_address", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_offset", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_offset_real", typeof(string));
            PresTable.Columns.Add("set_data_3_size", typeof(Int32));
            PresTable.Columns.Add("set_data_3_size_16", typeof(Int32));
            PresTable.Columns.Add("set_data_3_name_off_file", typeof(Int32));
            PresTable.Columns.Add("set_data_3_name_elements_file", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_unk1", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_unk2", typeof(Int32));
            PresTable.Columns.Add("set_data_3_file_unk3", typeof(Int32));
            PresTable.Columns.Add("set_data_3_usize", typeof(Int32));

            PresTable.Columns.Add("set_data_3_name", typeof(string));
            PresTable.Columns.Add("set_data_3_ext", typeof(string));
            PresTable.Columns.Add("set_data_3_folder", typeof(string));
            PresTable.Columns.Add("set_data_3_complete", typeof(string));

            PresTable.Columns.Add("set_data_3_file_magic", typeof(string));

            PresTable.Columns.Add("set_data_3_file_sha", typeof(string));

            GlobalPresTable = PresTable.Clone();

            DirectoryInfo base_directory = new DirectoryInfo(TargetDirectory.FullName + "_BASE\\");
            DirectoryInfo target_directory = new DirectoryInfo(TargetDirectory.FullName + "_UNPACK\\");


            var files = base_directory.GetFiles("*.pres", SearchOption.AllDirectories);
            Log.Info($"处理PRES文件，一共有{files.Length}。");
            int count = 0;
            for (int i = 0; i < files.Length; i++)
            {

                Log.Info($"正在处理{i}/{files.Length}：");
                PRES(files[i], target_directory.FullName);
                count++;

                if (count == 10000)
                {

                    int file_number = i + 1;

                    FileInfo excelT = new FileInfo(TargetDirectory + "pres-" + file_number + ".xlsx");
                    if (excelT.Exists)
                    {

                        excelT.Delete();
                    }

                    using (var stream = excelT.OpenWrite())
                    {
                        MiniExcel.SaveAs(stream, PresTable);
                    }



                    GlobalPresTable.Merge(PresTable);
                    PresTable.Rows.Clear();

                    count = 0;
                }

            }



            FileInfo excel = new FileInfo(TargetDirectory + "pres-" + files.Length + ".xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, PresTable);
            }

            GlobalPresTable.Merge(PresTable);
            PresTable.Rows.Clear();

            count = 0;

            foreach (var kv in PresTableMap)
            {
                FileInfo excelI = new FileInfo(TargetDirectory + "pres_" + kv.Key + "_.xlsx");
                if (excelI.Exists)
                {

                    excelI.Delete();
                }

                using (var stream = excelI.OpenWrite())
                {
                    MiniExcel.SaveAs(stream, kv.Value);
                }
            }


            FileInfo global = new FileInfo(TargetDirectory + "pres.bin");

            if (global.Exists)
            {
                global.Delete();
            }






            using (var stream = global.OpenWrite())
            {

                using (IDataReader dr = GlobalPresTable.CreateDataReader())
                {
                    DataSerializer.Serialize(stream, dr);
                }




            }


            //FileInfo global2 = new FileInfo(TargetDirectory + "pres.xml");

            //if (global2.Exists)
            //{
            //    global2.Delete();
            //}


            //using (var stream = global2.OpenWrite())
            //{


            //    GlobalPresTable.WriteXml(stream);



            //}


        }


        /*
         * 
         * PRES文件结构
         * 头 四字节 PRES = 0x73657250
         * 魔法码 十二字节 未知(都一样）
         * 四字节 集合数据偏移 指针
         * 八字节 00
         * 四字节 集合数量
         * 
         * 从集合数据偏移开始：
         * 每八字节一组，前四字节是这个集合数据信息的开始指针，后四字节是这个集合数据的长度
         * 
         * 集合数据开始：
         * 
         *      第一集合和第三集合数量一致：
         *      第三集合：
         *          4字节 文件偏移
         *          4字节 文件大小
         *          4字节 文件名字地址
         *          4字节 文件名字数量
         *          4字节 未知1
         *          4字节 未知2
         *          4字节 未知3
         *          4字节 实际文件大小
         *    从第三集合 文件名字地址 跳转 第一集合：
         *          4字节 文件名
         *          4字节 扩展名
         *          4字节 文件路径
         *          4字节 最终路径
         *      
         * 
         * 
         */

        static void PRES(FileInfo file, string workdirectory)
        {



            Log.Info($"读取PRES文件：{file.FullName},大小：{file.Length}");

            if (file.Length <= 3)
            {
                Log.Info($"跳过无用文件。");
                return;
            }

            using (BinaryReader br = new BinaryReader(file.Open(FileMode.Open)))
            {

                int magic = br.ReadInt32();

                if (magic == Define.PRES_MAGIC)
                {
                    Log.Info($" PRES文件：{magic}等于{Define.PRES_MAGIC}");
                }
                else
                {
                    Log.Expection();
                    throw new Exception($"错误，{file.Name}不是一个正确的PRES文件，{magic}不等于{Define.PRES_MAGIC},确定这是对的？");

                }

                int magic_1 = br.ReadInt32();
                int magic_2 = br.ReadInt32();
                int magic_3 = br.ReadInt32();
                int offset_data = br.ReadInt32();
                long zerozero = br.ReadInt64();
                int count_set = br.ReadInt32();

                long index_root = br.BaseStream.Position;
                long index_set = 0;
                long index_file = 0;

                Log.Info($" 数据偏移：{offset_data}，集合数量：{count_set}");

                for (int i = 0; i < count_set; i++)
                {

                    int set_offset = 0;
                    int set_length = 0;

                    if (count_set > 1)
                    {
                        set_offset = br.ReadInt32();
                        set_length = br.ReadInt32();
                        index_set = br.BaseStream.Position;
                        br.BaseStream.Seek(set_offset, SeekOrigin.Begin);
                        Log.Info($" 第{i + 1}/{count_set}集合在{set_offset}，集合大小：{set_length}");
                    }
                    else
                    {
                        Log.Info($" 集合数量是1，所以直接读取。");
                    }

                    // Read set info
                    int names_off = br.ReadInt32();
                    int names_elements = br.ReadInt32();

                    int set_unk1 = br.ReadInt32();
                    int set_unk2 = br.ReadInt32();

                    //3 real
                    int info_off = br.ReadInt32();
                    int count_file = br.ReadInt32();


                    int set_unk3 = br.ReadInt32();
                    int set_unk4 = br.ReadInt32();

                    int set_unk5 = br.ReadInt32();
                    int set_unk6 = br.ReadInt32();

                    int set_unk7 = br.ReadInt32();
                    int set_unk8 = br.ReadInt32();

                    //7 src
                    int set_unk9 = br.ReadInt32();
                    int set_unk10 = br.ReadInt32();



                    int set_unk11 = br.ReadInt32();
                    int set_unk12 = br.ReadInt32();




                    Log.Info($" 集合{i + 1}/{count_set}的文件偏移在{info_off}，数量为{count_file}");

                    br.BaseStream.Seek(info_off, SeekOrigin.Begin);
                    for (int fi = 0; fi < count_file; fi++)
                    {

                        int set_data_3_file_address = Convert.ToInt32(br.BaseStream.Position);

                        int offset_file = br.ReadInt32();
                        int csize_file = br.ReadInt32();
                        int name_off_file = br.ReadInt32();
                        int name_elements_file = br.ReadInt32();
                        int file_unk1 = br.ReadInt32();
                        int file_unk2 = br.ReadInt32();
                        int file_unk3 = br.ReadInt32();
                        int usize_file = br.ReadInt32();

                        index_file = br.BaseStream.Position;

                        // Get individual file name info
                        br.BaseStream.Seek(name_off_file, SeekOrigin.Begin);
                        int name_off_final = br.ReadInt32();
                        int ext_off_final = br.ReadInt32();
                        int folder_off_final = br.ReadInt32();
                        int complete_off_final = br.ReadInt32();

                        // Get individual file path
                        string str_name_final = "";
                        string str_ext_final = "";
                        string str_folder_final = "";
                        string str_complete_final = "";
                        if (name_elements_file >= 1) { br.BaseStream.Seek(name_off_final, SeekOrigin.Begin); str_name_final = Utils.readNullterminated(br); }
                        if (name_elements_file >= 2) { br.BaseStream.Seek(ext_off_final, SeekOrigin.Begin); str_ext_final = Utils.readNullterminated(br); }
                        if (name_elements_file >= 3) { br.BaseStream.Seek(folder_off_final, SeekOrigin.Begin); str_folder_final = Utils.readNullterminated(br); }
                        if (name_elements_file >= 4) { br.BaseStream.Seek(complete_off_final, SeekOrigin.Begin); str_complete_final = Utils.readNullterminated(br); }


                        Log.Info($"     文件{fi + 1}/{count_file}");


                        string real_offset_file = offset_file.ToString("X8");

                        int real_offset = Convert.ToInt32(real_offset_file.Substring(1), 16);

                        Log.Info($"     文件偏移：{real_offset_file}");
                        Log.Info($"     文件类型：{real_offset_file[0]}");
                        Log.Info($"     文件真实偏移原始数据：{real_offset_file.Substring(1)}");
                        Log.Info($"     文件真实偏移：{real_offset}");
                        Log.Info($"     文件名字：{str_name_final}");
                        Log.Info($"     文件扩展名：{str_ext_final}");
                        Log.Info($"     文件夹：{str_folder_final}");
                        Log.Info($"     最终输出：{str_complete_final}");



                        string output_path = str_complete_final;

                        output_path = output_path.Replace("/", "\\");

                        Log.Info($"     矫正输出为Windows地址:{output_path}");

                        //int shifted_offset = offset_file & ((1 << (32 - 4)) - 1);
                        int shifted_offset = real_offset;
                        br.BaseStream.Seek(shifted_offset, SeekOrigin.Begin);
                        byte[] data = br.ReadBytes(csize_file);

                        Log.Info($"     文件原始偏移：{offset_file},文件修正偏移：{shifted_offset},文件大小:{csize_file}");
                        Log.Info($"     写出文件：{workdirectory + output_path}");

                        string target_directory = Path.GetDirectoryName(workdirectory + output_path);


                        Log.Info($"     给微软擦屁股:{target_directory}");

                        if (!Directory.Exists(target_directory))
                        {
                            Log.Info($"     创建目录{target_directory}");
                            Directory.CreateDirectory(target_directory);
                        }
                        StringBuilder file_magic = new StringBuilder();
                        if (output_path.Length <= 0)
                        {
                            Log.Info($"     这个文件不合法：{fi + 1}/{count_file}，位于集合{i + 1}/{count_set}，从属于{file.Name}文件，因为没有目录。");
                            file_magic.Append("NO_DIR");
                        }
                        else
                        {


                            if (real_offset_file[0] == 'B')
                            {
                                Log.Info($"     这个文件是虚拟的：{fi + 1}/{count_file}，位于集合{i + 1}/{count_set}，从属于{file.Name}文件，因为头偏移是B不是F，不输出。");
                                file_magic.Append("B_FILE");
                            }
                            else if (data.Length <= 0)
                            {
                                Log.Info($"     这个文件是空的：{fi + 1}/{count_file}，位于集合{i + 1}/{count_set}，从属于{file.Name}文件，因为没有目录，不输出。");
                                file_magic.Append("0_LENGTH_FILE");
                            }
                            else
                            {




                                if (data.Length >= 4)
                                {
                                    file_magic.Append((char)data[0]);
                                    file_magic.Append((char)data[1]);
                                    file_magic.Append((char)data[2]);
                                    file_magic.Append((char)data[3]);




                                    Log.Info($"     输出文件的魔术码{file_magic.ToString()}");

                                }
                                File.WriteAllBytes(workdirectory + output_path, data);



                            }







                        }

                        DataRow dr = PresTable.NewRow();

                        dr["file_name"] = file.Name;
                        dr["file_path"] = file.FullName;
                        dr["file_magic"] = magic;
                        dr["file_size"] = file.Length;
                        dr["magic_1"] = magic_1;
                        dr["magic_2"] = magic_2;
                        dr["magic_3"] = magic_3;

                        dr["offset_data"] = offset_data;
                        dr["count_set"] = count_set;

                        dr["set_index"] = i;
                        dr["set_offset"] = set_offset;
                        dr["set_length"] = set_length;

                        dr["set_data_3_start_offset"] = info_off;
                        dr["set_data_3_file_count"] = count_file;


                        dr["set_data_3_file_index"] = fi;
                        dr["set_data_3_file_address"] = set_data_3_file_address;
                        dr["set_data_3_file_offset"] = offset_file;
                        dr["set_data_3_file_offset_real"] = real_offset_file;


                        dr["set_data_3_size"] = csize_file;

                        if (csize_file % 16 != 0)
                        {
                            dr["set_data_3_size_16"] = ((csize_file / 16) + 1) * 16;
                        }
                        else
                        {
                            dr["set_data_3_size_16"] = (csize_file / 16) * 16;
                        }



                        dr["set_data_3_name_off_file"] = name_off_file;
                        dr["set_data_3_name_elements_file"] = name_elements_file;
                        dr["set_data_3_file_unk1"] = file_unk1;
                        dr["set_data_3_file_unk2"] = file_unk2;
                        dr["set_data_3_file_unk3"] = file_unk3;
                        dr["set_data_3_usize"] = usize_file;

                        dr["set_data_3_name"] = str_name_final;
                        dr["set_data_3_ext"] = str_ext_final;
                        dr["set_data_3_folder"] = str_folder_final;
                        dr["set_data_3_complete"] = str_complete_final;

                        dr["set_data_3_file_magic"] = file_magic.ToString();

                        dr["set_data_3_file_sha"] = CryptUtils.GetSHA256HashFromBytes(data);

                        PresTable.Rows.Add(dr);




                        if (!PresTableMap.ContainsKey(str_ext_final))
                        {
                            PresTableMap.Add(str_ext_final, GlobalPresTable.Clone());
                        }

                        PresTableMap[str_ext_final].ImportRow(dr);



                        br.BaseStream.Position = index_file;
                    }



                    br.BaseStream.Position = index_set;

                }



            }
        }

        static void ProcessPRES_7()
        {

            PresTableMap = new Dictionary<string, DataTable>();

            PresTable = new DataTable();


            PresTable.Columns.Add("file_name", typeof(string));
            PresTable.Columns.Add("file_path", typeof(string));
            PresTable.Columns.Add("file_magic", typeof(Int32));
            PresTable.Columns.Add("file_size", typeof(Int32));
            PresTable.Columns.Add("magic_1", typeof(Int32));
            PresTable.Columns.Add("magic_2", typeof(Int32));
            PresTable.Columns.Add("magic_3", typeof(Int32));
            PresTable.Columns.Add("offset_data", typeof(Int32));
            PresTable.Columns.Add("count_set", typeof(Int32));

            PresTable.Columns.Add("set_index", typeof(Int32));
            PresTable.Columns.Add("set_offset", typeof(Int32));
            PresTable.Columns.Add("set_length", typeof(Int32));

            PresTable.Columns.Add("set_data_7_A", typeof(Int32));
            PresTable.Columns.Add("set_data_7_B", typeof(Int32));

            PresTable.Columns.Add("set_data_7_index", typeof(Int32));
            PresTable.Columns.Add("set_data_7_offset", typeof(Int32));

            PresTable.Columns.Add("set_data_7_data_offset", typeof(Int32));
            PresTable.Columns.Add("set_data_7_data_offset_real", typeof(Int32));
            PresTable.Columns.Add("set_data_7_data_length", typeof(Int32));
            PresTable.Columns.Add("set_data_7_data", typeof(string));

            PresTable.Columns.Add("set_data_7_header_offset", typeof(Int32));
            PresTable.Columns.Add("set_data_7_header_data", typeof(Int32));

            PresTable.Columns.Add("set_data_7_header_offset_offset", typeof(Int32));
            PresTable.Columns.Add("set_data_7_header_offset_offset_data", typeof(string));

            PresTable.Columns.Add("set_data_7_data_new", typeof(string));

            GlobalPresTable = PresTable.Clone();

            DirectoryInfo base_directory = new DirectoryInfo(TargetDirectory.FullName + "_BASE\\");
            DirectoryInfo target_directory = new DirectoryInfo(TargetDirectory.FullName + "_UNPACK\\");


            var files = base_directory.GetFiles("*.pres", SearchOption.AllDirectories);
            Log.Info($"处理PRES文件，一共有{files.Length}。(7)");
            int count = 0;
            for (int i = 0; i < files.Length; i++)
            {

                Log.Info($"正在处理{i}/{files.Length}：");
                PRES_7(files[i], target_directory.FullName);
                count++;

                if (count == 10000)
                {

                    int file_number = i + 1;

                    FileInfo excelT = new FileInfo(TargetDirectory + "pres_7-" + file_number + ".xlsx");
                    if (excelT.Exists)
                    {

                        excelT.Delete();
                    }

                    using (var stream = excelT.OpenWrite())
                    {
                        MiniExcel.SaveAs(stream, PresTable);
                    }



                    GlobalPresTable.Merge(PresTable);
                    PresTable.Rows.Clear();

                    count = 0;
                }

            }



            FileInfo excel = new FileInfo(TargetDirectory + "pres_7-" + files.Length + ".xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, PresTable);
            }

            GlobalPresTable.Merge(PresTable);
            PresTable.Rows.Clear();

            count = 0;

            foreach (var kv in PresTableMap)
            {
                FileInfo excelI = new FileInfo(TargetDirectory + "pres_7-" + kv.Key + "_.xlsx");
                if (excelI.Exists)
                {

                    excelI.Delete();
                }

                using (var stream = excelI.OpenWrite())
                {
                    MiniExcel.SaveAs(stream, kv.Value);
                }
            }


            DataTable temp = GlobalPresTable.Clone();

            excel = new FileInfo(TargetDirectory + "pres_7_builder.xlsx");

            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, temp);
            }

            //FileInfo global = new FileInfo(TargetDirectory + "pres_7.bin");

            //if (global.Exists)
            //{
            //    global.Delete();
            //}






            //using (var stream = global.OpenWrite())
            //{

            //    using (IDataReader dr = GlobalPresTable.CreateDataReader())
            //    {
            //        DataSerializer.Serialize(stream, dr);
            //    }




            //}


            //FileInfo global2 = new FileInfo(TargetDirectory + "pres.xml");

            //if (global2.Exists)
            //{
            //    global2.Delete();
            //}


            //using (var stream = global2.OpenWrite())
            //{


            //    GlobalPresTable.WriteXml(stream);



            //}


        }

        // int32 data_offset 4bytes -> (F???????) -> (0???????)
        // int32 data_length 4bytes
        // int32 header_offset 4bytes
        // int32 header_? 4bytes

        // 16 ZERO

        //data_offset
        //End with data_length

        //header_offset 
        //int32 real_header_offset 4bytes

        //real_header_offset
        //End with 0


        static void PRES_7(FileInfo file, string workdirectory)
        {



            Log.Info($"读取PRES文件：{file.FullName},大小：{file.Length}");

            if (file.Length <= 3)
            {
                Log.Info($"跳过无用文件。");
                return;
            }

            using (BinaryReader br = new BinaryReader(file.Open(FileMode.Open)))
            {

                int magic = br.ReadInt32();

                if (magic == Define.PRES_MAGIC)
                {
                    Log.Info($" PRES文件：{magic}等于{Define.PRES_MAGIC}");
                }
                else
                {
                    Log.Expection();
                    throw new Exception($"错误，{file.Name}不是一个正确的PRES文件，{magic}不等于{Define.PRES_MAGIC},确定这是对的？");

                }

                int magic_1 = br.ReadInt32();
                int magic_2 = br.ReadInt32();
                int magic_3 = br.ReadInt32();
                int offset_data = br.ReadInt32();
                long zerozero = br.ReadInt64();
                int count_set = br.ReadInt32();

                long index_root = br.BaseStream.Position;
                long index_set = 0;
                long index_file = 0;

                Log.Info($" 数据偏移：{offset_data}，集合数量：{count_set}");

                for (int i = 0; i < count_set; i++)
                {

                    int set_offset = 0;
                    int set_length = 0;

                    if (count_set > 1)
                    {
                        set_offset = br.ReadInt32();
                        set_length = br.ReadInt32();
                        index_set = br.BaseStream.Position;
                        br.BaseStream.Seek(set_offset, SeekOrigin.Begin);
                        Log.Info($" 第{i + 1}/{count_set}集合在{set_offset}，集合大小：{set_length}");
                    }
                    else
                    {
                        Log.Info($" 集合数量是1，所以直接读取。");
                    }

                    // Read set info
                    int names_off = br.ReadInt32();
                    int names_elements = br.ReadInt32();

                    int set_unk1 = br.ReadInt32();
                    int set_unk2 = br.ReadInt32();

                    //3 real
                    int info_off = br.ReadInt32();
                    int count_file = br.ReadInt32();


                    int set_unk3 = br.ReadInt32();
                    int set_unk4 = br.ReadInt32();

                    int set_unk5 = br.ReadInt32();
                    int set_unk6 = br.ReadInt32();

                    int set_unk7 = br.ReadInt32();
                    int set_unk8 = br.ReadInt32();

                    //7 src
                    int set_unk9 = br.ReadInt32();
                    int set_unk10 = br.ReadInt32();



                    int set_unk11 = br.ReadInt32();
                    int set_unk12 = br.ReadInt32();




                    Log.Info($" 集合{i + 1}/{count_set}的文件A区是{set_unk9}，B区是{set_unk10}");





                    br.BaseStream.Seek(set_unk9, SeekOrigin.Begin);


                    for (int fi = 0; fi < set_unk10; fi++)
                    {

                        int set_data_7_address = Convert.ToInt32(br.BaseStream.Position);

                        int set_data_7_data_offset = br.ReadInt32();
                        int set_data_7_data_offset_real = Convert.ToInt32(set_data_7_data_offset.ToString("X8").Substring(1),16);
                        int set_data_7_data_length = br.ReadInt32();
                        int set_data_7_header_offset = br.ReadInt32();
                        int set_data_7_header_data = br.ReadInt32();

                        Log.Info($"     偏移：{set_data_7_data_offset.ToString("X8")}");
                        Log.Info($"     真实偏移：{set_data_7_data_offset_real}");
                        Log.Info($"     长度：{set_data_7_data_length}");
                        Log.Info($"     头偏移：{set_data_7_header_offset}");
                        Log.Info($"     头数据？：{set_data_7_header_data}");

                        //if (set_data_7_data_offset > file.Length || set_data_7_header_offset > file.Length)
                        //{
                        //    Log.Info($"{file.Name}的第七区偏移大于整个文件，这是有效的文件？");
                        //    continue;
                        //}

                        index_file = br.BaseStream.Position;

                        br.BaseStream.Seek(set_data_7_header_offset, SeekOrigin.Begin);

                        int set_data_7_header_offset_offset= br.ReadInt32();
                        br.BaseStream.Seek(set_data_7_header_offset_offset, SeekOrigin.Begin);
                        string set_data_7_header_offset_offset_data = Utils.readNullterminated(br);

                        br.BaseStream.Seek(set_data_7_data_offset_real, SeekOrigin.Begin);

                        //StringBuilder strbuild = new StringBuilder();

                        //for (int si = 0; si < set_data_7_data_length; si++)
                        //{
                        //    strbuild.Append(Convert.ToChar(br.ReadByte()));
                        //}

                        //string set_data_7_data =strbuild.ToString();

                        byte[] utf_bytes = new byte[set_data_7_data_length];

                        for(int si=0;si < utf_bytes.Length; si++)
                        {
                            utf_bytes[si] = br.ReadByte();
                        }

                        string set_data_7_data = new UTF8Encoding().GetString(utf_bytes);


                        Log.Info($"     数据{fi + 1}/{count_file}");



                        Log.Info($"     偏移：{set_data_7_data_offset.ToString("X8")}");
                        Log.Info($"     真实偏移：{set_data_7_data_offset_real}");
                        Log.Info($"     长度：{set_data_7_data_length}");
                        Log.Info($"     数据：{set_data_7_data}");

                        Log.Info($"     头偏移：{set_data_7_header_offset}");
                        Log.Info($"     头数据？：{set_data_7_header_data}");

                        Log.Info($"     头偏移的偏移：{set_data_7_header_offset_offset}");
                        Log.Info($"     头偏移的偏移的数据：{set_data_7_header_offset_offset_data}");


                        DataRow dr = PresTable.NewRow();

                        dr["file_name"] = file.Name;
                        dr["file_path"] = file.FullName;
                        dr["file_magic"] = magic;
                        dr["file_size"] = file.Length;
                        dr["magic_1"] = magic_1;
                        dr["magic_2"] = magic_2;
                        dr["magic_3"] = magic_3;

                        dr["offset_data"] = offset_data;
                        dr["count_set"] = count_set;

                        dr["set_index"] = i;
                        dr["set_offset"] = set_offset;
                        dr["set_length"] = set_length;

                        dr["set_data_7_A"] = set_unk9;
                        dr["set_data_7_B"] = set_unk10;

                        dr["set_data_7_index"] = fi;
                        dr["set_data_7_offset"] = set_data_7_address;

                        dr["set_data_7_data_offset"] = set_data_7_data_offset;
                        dr["set_data_7_data_offset_real"] = set_data_7_data_offset_real;
                        dr["set_data_7_data_length"] = set_data_7_address;
                        dr["set_data_7_data"] = set_data_7_data;


                        dr["set_data_7_header_offset"] = set_data_7_header_offset;
                        
                        dr["set_data_7_header_data"] = set_data_7_header_data;


                        dr["set_data_7_header_offset_offset"] = set_data_7_header_offset_offset;
                        dr["set_data_7_header_offset_offset_data"] = set_data_7_header_offset_offset_data;


                        PresTable.Rows.Add(dr);

                        br.BaseStream.Position = index_file;
                        br.BaseStream.Seek(16, SeekOrigin.Current);
                    }
                    br.BaseStream.Position = index_set;




                }



            }
        }

        static void ProcessBLZ4()
        {

            Blz4Table = new DataTable();

            Blz4Table.Columns.Add("file_path", typeof(string));
            Blz4Table.Columns.Add("file_name", typeof(string));
            Blz4Table.Columns.Add("file_magic", typeof(Int32));
            Blz4Table.Columns.Add("file_size", typeof(Int32));

            Blz4Table.Columns.Add("data_chunk_count", typeof(Int32));
            Blz4Table.Columns.Add("data_all_chunk_size", typeof(Int32));

            Blz4Table.Columns.Add("data_size_ratio", typeof(double));

            Blz4Table.Columns.Add("data_unpacked_size", typeof(Int32));
            Blz4Table.Columns.Add("data_md5", typeof(string));
            Blz4Table.Columns.Add("data_md5_check", typeof(string));

            Blz4Table.Columns.Add("data_file_path", typeof(string));
            Blz4Table.Columns.Add("data_file_name", typeof(string));


            DirectoryInfo base_directory = new DirectoryInfo(TargetDirectory.FullName + "_BASE\\");
            DirectoryInfo unpack_directory = new DirectoryInfo(TargetDirectory.FullName + "_UNPACK\\");
            DirectoryInfo unpack_res_directory = new DirectoryInfo(TargetDirectory.FullName + "_UNPACK_RES\\");

            var files = unpack_directory.GetFiles("*", SearchOption.AllDirectories);
            Log.Info($"处理解包BLZ4文件，一共有{files.Length}。");
            for (int i = 0; i < files.Length; i++)
            {

                Log.Info($"正在处理{i}/{files.Length}：");
                BLZ4(files[i], files[i].FullName.Replace("_UNPACK", "_UNPACK_PRES_REAL"));

            }

            files = unpack_res_directory.GetFiles("*", SearchOption.AllDirectories);
            Log.Info($"处理RES解包BLZ4文件，一共有{files.Length}。");
            for (int i = 0; i < files.Length; i++)
            {

                Log.Info($"正在处理{i}/{files.Length}：");
                BLZ4(files[i], files[i].FullName.Replace("_UNPACK_RES", "_UNPACK_RES_REAL"));

            }

            files = base_directory.GetFiles("*.blz4", SearchOption.AllDirectories);
            Log.Info($"处理qpck的BLZ4文件，一共有{files.Length}。");
            for (int i = 0; i < files.Length; i++)
            {

                Log.Info($"正在处理{i}/{files.Length}：");
                BLZ4(files[i], files[i].FullName.Replace("_BASE", "_UNPACK_QPCK_REAL"));

            }

            FileInfo excel = new FileInfo(TargetDirectory + "blz4.xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, Blz4Table);
            }

        }


        static void BLZ4(FileInfo file, string targetPath)
        {

            Log.Info($"读取BLZ4文件：{file.FullName},大小：{file.Length}");

            DataRow dr = Blz4Table.NewRow();

            dr["file_path"] = file.FullName;
            dr["file_name"] = file.Name;


            if (file.Length <= 3)
            {
                Log.Info($"跳过无用文件。");
                dr["file_magic"] = -1;

                Blz4Table.Rows.Add(dr);

                return;
            }

            using (BinaryReader br = new BinaryReader(file.Open(FileMode.Open)))
            {

                int magic = br.ReadInt32();

                dr["file_magic"] = magic;
                dr["file_size"] = file.Length;

                if (magic == Define.BLZ4_MAGIC)
                {
                    Log.Info($" BLZ4文件：{magic}等于{Define.BLZ4_MAGIC}");
                }
                else
                {
                    Log.Expection();
                    Log.Error($"错误，{file.Name}不是一个正确的BLZ4文件，{magic}不等于{Define.BLZ4_MAGIC},确定这是对的？");
                    return;
                }

                int unpacked_size = br.ReadInt32();

                dr["data_unpacked_size"] = unpacked_size;

                Log.Info($" 解压后大小:{unpacked_size}B");

                //我认为zerozero应该是4字节，unpacked_size是8字节，因为没有文件大于int32.max，所以这么取也可以。

                long zerozero = br.ReadInt64();

                byte[] md5 = br.ReadBytes(16);

                Log.Info($"MD5：");
                dr["data_md5"] = Utils.PrintByteArray(md5);


                List<byte[]> list = new List<byte[]>();
                LinkedList<byte[]> real_list = new LinkedList<byte[]>();

                int chunk = 0;

                bool is_uncompress = false;

                while (br.BaseStream.Position < file.Length)
                {

                    int chunk_size = br.ReadUInt16();

                    if (chunk_size == 0)
                    {
                        is_uncompress = true;
                        int uncompress_length = Convert.ToInt32(file.Length - br.BaseStream.Position);

                        chunk_size = uncompress_length;

                        Log.Info($"{file.FullName}是没有被压缩的blz4文件！直接取出结果，真实文件长度：（{uncompress_length}）");
                    }


                    byte[] data = new byte[chunk_size];
                    data = br.ReadBytes(chunk_size);




                    Log.Info($"读取了第{chunk}个区块，大小为:{chunk_size}，读取到的内容大小：{data.Length}");
                    Log.Info($"目前指针：{br.BaseStream.Position},文件长度：{file.Length}");

                    if (data.Length > 0)
                    {
                        list.Add(data);
                    }
                    else
                    {
                        Log.Info($"这个文件不合法！{file.FullName},因为他的{chunk}区块大小为:{data.Length}");
                        File.WriteAllText(TargetDirectory.FullName + "\\" + file.Name + ".log", $"这个文件不合法！{file.FullName},因为他的{chunk}区块大小为:{data.Length}");
                        return;
                    }


                    chunk++;
                }

                int all_chunk_size = 0;

                for (int i = 1; i < list.Count; i++)
                {
                    real_list.AddLast(list[i]);
                    all_chunk_size += list[i].Length;
                    Log.Info($"添加{i + 1}个区块到真实列表，大小为:{list[i].Length}");
                }

                //real_list.AddFirst(list[list.Count - 1]);

                if (list.Count > 1)
                {
                    real_list.AddLast(list[0]);
                    all_chunk_size += list[0].Length;
                    Log.Info($"添加第一个区块到真实列表尾部，大小为:{list[0].Length}");
                }
                else
                {
                    real_list.AddLast(list[0]);
                    all_chunk_size += list[0].Length;
                    Log.Info($"添加唯一区块，大小为:{list[0].Length}");
                }



                Log.Info($"总区块大小:{all_chunk_size}");

                dr["data_all_chunk_size"] = all_chunk_size;
                dr["data_chunk_count"] = list.Count;

                byte[] all_chunk = new byte[unpacked_size];



                LinkedList<byte[]> file_block_list = new LinkedList<byte[]>();

                if (!is_uncompress)
                {
                    foreach (byte[] data in real_list)
                    {



                        MemoryStream stream = new MemoryStream(data);
                        byte[] out_file;
                        BLZ4Utils.DecompressData(data, out out_file);

                        file_block_list.AddLast(out_file);


                    }
                }
                else
                {
                    Log.Info($"未压缩，略过解压阶段。");

                    foreach (byte[] data in real_list)
                    {
                        file_block_list.AddLast(data);
                        Log.Info($"添加未压缩的{data.Length}长度内容到列表中。");

                    }


                }


                int index = 0;
                foreach (byte[] data in file_block_list)
                {

                    foreach (byte b in data)
                    {
                        all_chunk[index++] = b;
                    }


                }


                Log.Info($"给微软擦屁股:{Path.GetDirectoryName(targetPath)}");

                if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
                {
                    Log.Info($"创建目录{Path.GetDirectoryName(targetPath)}");
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                }

                //byte[] result = BLZ4Utils.ZLibDecompress(all_chunk, unpacked_size);

                File.WriteAllBytes(targetPath, all_chunk);

                dr["data_md5_check"] = CryptUtils.GetMD5HashFromBytes(all_chunk);

                dr["data_file_path"] = targetPath;
                dr["data_size_ratio"] = (double)all_chunk_size / unpacked_size;
                dr["data_file_name"] = Path.GetFileName(targetPath);

                Blz4Table.Rows.Add(dr);


            }


        }

    }
}
