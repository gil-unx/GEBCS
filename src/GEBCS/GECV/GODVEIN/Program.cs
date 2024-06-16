using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GECV;
using MiniExcelLibs;
using ProtoBuf.Data;

namespace GODVEIN
{
    internal class Program
    {


        static DirectoryInfo RootDirectory;
        static DirectoryInfo BaseDirectory;
        static DirectoryInfo ExtraDirectory;
        static DirectoryInfo QpckBlz4Directory;

        static DirectoryInfo PresRealDirectory;
        static DirectoryInfo PresVirtualDirectory;


        static DirectoryInfo PresRealBLZ4Directory;
        //static DirectoryInfo PresRealNOBLZ4Directory;
        static DirectoryInfo ResRealBLZ4Directory;
        static DirectoryInfo ResRealDirectory;

        static DataTable PresTable = new DataTable();
        static DataTable ResTable = new DataTable();

        static bool UseSingleFileAppender = true;
        static bool OnlyUseCacheExt = true;

        static void Main(string[] args)
        {
            

            Log.Info("GOD VEIN 噬神者资源映射 打包器 BY 兰德里奥（HaoJun0823）");
            Log.Info("https://blog.haojun0823.xyz/");
            Log.Info("https://github.com/HaoJun0823/GECV");
            Log.Info("参考：https://github.com/mhvuze/GEUndub/ By mhvuze");
            Log.Info(@"License: MiniExcel,Zlib.Net,Protobuf-net,Protobuf-net-data");

            if (args.Length != 0)
            {

                RootDirectory = new DirectoryInfo(args[0]);
                GECV.Log.SetLogFolder(RootDirectory);
                BaseDirectory = new DirectoryInfo(RootDirectory.FullName + "\\_BASE");
                ExtraDirectory = new DirectoryInfo(RootDirectory.FullName + "\\_EXTRA");
                QpckBlz4Directory = new DirectoryInfo(RootDirectory.FullName + "\\_UNPACK_QPCK_REAL_EXTRA");

                PresVirtualDirectory = new DirectoryInfo(RootDirectory.FullName + "\\_UNPACK_PRES_REAL");
                PresRealDirectory = new DirectoryInfo(RootDirectory.FullName + "\\_UNPACK_PRES_REAL_EXTRA");
                PresRealBLZ4Directory = new DirectoryInfo(RootDirectory.FullName + "\\_UNPACK_PRES_REAL_EXTRA_BLZ4");

                ResRealDirectory = new DirectoryInfo(RootDirectory.FullName + "\\_UNPACK_RES_REAL_EXTRA");
                ResRealBLZ4Directory = new DirectoryInfo(RootDirectory.FullName + "\\_UNPACK_RES_REAL_EXTRA_BLZ4");

                //PresRealNOBLZ4Directory = new DirectoryInfo(RootDirectory.FullName + "\\_UNPACK_PRES_REAL_EXTRA_NO_BLZ4");


                PresRealDirectory.Create();
                QpckBlz4Directory.Create();
                PresRealBLZ4Directory.Create();

                ResRealDirectory.Create();
                ResRealBLZ4Directory.Create();


                int input = -1;
                while (input != 0)
                {



                    Log.Info($"工作目录：{RootDirectory.FullName}");
                    Log.Info($"1.打包QPCK，优先打包{ExtraDirectory.Name},其次打包{BaseDirectory.Name}，生成文件到{RootDirectory}\\pack.qpck");
                    Log.Info($"2.压缩散装BLZ4，压缩{QpckBlz4Directory.Name}，生成文件到{ExtraDirectory.Name}");
                    Log.Info($"3.压缩Pres解压的BLZ4，压缩{PresRealDirectory.Name}，生成文件到{PresRealBLZ4Directory.Name}，压缩压缩Res解压的BLZ4，压缩{ResRealDirectory.Name}，生成文件到{ResRealBLZ4Directory.Name}。");
                    Log.Info($"4.根据{PresRealBLZ4Directory.Name}生成对应的pres操作表。（可以把没有压缩过的文件也放在这里）");
                    Log.Info($"5.执行4生成的操作表{RootDirectory.Name}\\packer.bin，反打包到pres，存储于{ExtraDirectory.Name},pres拼合模式。（还会拼合pres_7_builder.xlsx文件的内容）");
                    Log.Info($"6.设置：pres和res反打包时，单set单file抹去末尾文件再拼合，目前设置选项{UseSingleFileAppender}。");
                    //Log.Info($"7.执行操作表{RootDirectory.FullName}\\packer.bin，反打包到pres，存储于{ExtraDirectory.FullName},pres拼合模式。（多线程测试）");

                    Log.Info($"8.从pres.bin建立缓存。");
                    Log.Info($"9.设置：只用缓存的文件类型，目前选项{OnlyUseCacheExt}。");
                    Log.Info($"10.处理res，打包{ResRealBLZ4Directory.Name}到{PresRealBLZ4Directory.Name}。");
                    Log.Info($"11.打包GNF，根据gnf_new.bin把_UNPACK_PRES_REAL的dds打包成gnf存在_UNPACK_PRES_REAL_EXTRA");
                    Log.Info($"12.打包自定义GNF，根据gnf_custom.bin把_CUSTOM_GNF的dds打包成gnf存在_CUSTOM_GNF\\PACKED");
                    Log.Info("0.退出");

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
                            QPCK();
                            break;
                        case 2:
                            ProcessQPCKBLZ4();
                            break;
                        case 3:
                            ProcessPRESBLZ4();
                            break;
                        case 4:
                            PreparingPRES();
                            break;
                        case 5:
                            AppendPRES();
                            break;
                        case 6:
                            UseSingleFileAppender = !UseSingleFileAppender;
                            break;
                        //case 7:
                        //    AppendPRESParallel();
                        //    break;
                        case 8:
                            BuildCache();
                            break;
                        case 9:
                            OnlyUseCacheExt = !OnlyUseCacheExt;
                            break;
                        case 10:
                            PreparingRES();
                            break;
                        case 11:
                            ProcessGNF("gnf_new.bin", "_UNPACK_PRES_REAL", "_UNPACK_PRES_REAL_EXTRA");
                            break;
                        case 12:
                            ProcessGNF("gnf_custom.bin", "_CUSTOM_GNF", "_CUSTOM_GNF\\_PACKED");
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
                throw new Exception("请带一个文件夹参数。");
            }

            //Utils.WriteListToFile(Log.LogRecord, RootDirectory.FullName + "\\GODVEIN.log");
            Log.flush();
            Log.Info("按任意键退出……");
            Console.ReadKey();

        }


        static void ProcessCustomGNF()
        {

        }

        static void ProcessGNF(string bin_name,string original_folder,string target_folder)
        {

            DataTable dt = new DataTable();

            using (FileStream fs = File.OpenRead(RootDirectory + "\\"+bin_name))
            {
                using (IDataReader dr = DataSerializer.Deserialize(fs))
                {
                    dt.Load(dr);
                }
            }

            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();


            foreach (DataRow i in dt.Rows)
            {
                if (!map.ContainsKey(i["file_name"].ToString()))
                {
                    map.Add(i["file_name"].ToString(), new List<string>());
                    Log.Info($"没有这个关键字：{i["file_name"].ToString()}，新建一个。");
                }

                map[i["file_name"].ToString()].Add(i["dds_path"].ToString());
                Log.Info($"添加关键字：{i["file_name"].ToString()}对象。");
            }

            Parallel.ForEach<KeyValuePair<string, List<string>>>(map, (kv) =>
            {
                string target = kv.Key.Replace(original_folder, target_folder);
                Log.Info($"矫正路径到：{target}，对象数量：{kv.Value.Count}");
                List<byte[]> dds_group = new List<byte[]>();
                foreach (var i in kv.Value)
                {
                    dds_group.Add(File.ReadAllBytes(i));
                    Log.Info($"读取了{i}！");
                }

                    using (FileStream fs = File.OpenWrite(target))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {

                        bw.Write(kv.Value.Count);
                        Log.Info($"写入，数量：{kv.Value.Count}");
                        
                        foreach(var i in dds_group)
                        {
                            bw.Write(i.Length);
                            Log.Info($"写入，大小：{i.Length}");
                        }

                        foreach (var i in dds_group)
                        {
                            bw.Write(i);
                            Log.Info($"写入，文件：{i.Length}");
                        }



                    }

                }


            });

        }




        public static void AppendPRES()
        {
            DataTable dt = new DataTable();

            using (FileStream fs = File.OpenRead(RootDirectory + "\\packer.bin"))
            {
                using (IDataReader dr = DataSerializer.Deserialize(fs))
                {
                    dt.Load(dr);
                }
            }

            Dictionary<string, PresAppender> file_maps = new Dictionary<string, PresAppender>();

            foreach (DataRow dr in dt.Rows)
            {
                string key = dr["file_name"].ToString();

                Log.Info($"现在读取{key}");

                if (file_maps.ContainsKey(key))
                {

                    file_maps[key].AppendNewFile(dr);
                    Log.Info($"字典有{key}，启动PDD系统，再次拼合文件。");



                }
                else
                {
                    Log.Info($"字典无{key}，读取新文件并存进去。");
                    byte[] read_bytes = File.ReadAllBytes(dr["file_path"].ToString());
                    Log.Info($"新文件读取到的大小为：{read_bytes.Length}。");
                    file_maps.Add(key, new PresAppender(read_bytes));

                    int count_set = Convert.ToInt32(dr["count_set"].ToString());
                    int set_data_3_file_count = Convert.ToInt32(dr["set_data_3_file_count"].ToString());

                    int pres_count = dt.Select($"file_name='{dr["file_name"].ToString()}'").Length;

                    Log.Info($"{dr["file_name"].ToString()}，有{count_set}个集合，集合{dr["set_index"].ToString()}有:{set_data_3_file_count}个文件。");
                    Log.Info($"这个Pres应该有多少个文件需要被打包？答案是：{pres_count}");

                    if (count_set == 1 && set_data_3_file_count == 1 && pres_count == 1 && UseSingleFileAppender)
                    {
                        Log.Info($"字典有{key}，而且有{count_set}个集合和{set_data_3_file_count}个文件，该文件也只有{pres_count}个需要被打包的文件，而且单文件去尾拼合的状态是：{UseSingleFileAppender}，所以启动YS系统，去末尾+拼合文件。");
                        file_maps[key].RemoveLastFileAndAppendNewFile(dr);

                    }
                    else
                    {
                        Log.Info($"启动PDD系统，拼合文件。");
                        file_maps[key].AppendNewFile(dr);

                    }
                }





            }

            using(FileStream fs = File.OpenRead(RootDirectory + "\\pres_7_builder.xlsx"))
            {
                DataTable pres7Table = MiniExcel.QueryAsDataTable(fs,true);


                foreach(DataRow dr in pres7Table.Rows)
                {

                    string key = dr["file_name"].ToString();

                    if (file_maps.ContainsKey(key))
                    {
                        file_maps[key].AppendPres7Data(dr);
                        Log.Info($"字典有{key}，启动PDD系统，再次拼合文件。");
                    }
                    else
                    {
                        Log.Info($"字典无{key}，读取新文件并存进去。");
                        byte[] read_bytes = File.ReadAllBytes(dr["file_path"].ToString());
                        Log.Info($"新文件读取到的大小为：{read_bytes.Length}。");
                        file_maps.Add(key, new PresAppender(read_bytes));

                        int count_set = Convert.ToInt32(dr["count_set"].ToString());
                        int set_data_3_file_count = Convert.ToInt32(dr["set_data_7_B"].ToString());

                        int pres_count = dt.Select($"file_name='{dr["file_name"].ToString()}'").Length;

                        Log.Info($"{dr["file_name"].ToString()}，有{count_set}个集合，集合{dr["set_index"].ToString()}有:{set_data_3_file_count}个文件。");
                        Log.Info($"这个Pres应该有多少个文件需要被打包？答案是：{pres_count}");

                            Log.Info($"启动PDD系统，拼合文件。");
                            file_maps[key].AppendPres7Data(dr);

                    }



                }



            }

            foreach (var i in file_maps)
            {

                i.Value.SaveFile(ExtraDirectory + "\\" + i.Key);


            }




        }

        public static void BuildCache()
        {
            PresTable = new DataTable();

            using (FileStream fs = File.OpenRead(RootDirectory + "\\pres.bin"))
            {
                using (IDataReader dr = DataSerializer.Deserialize(fs))
                {
                    PresTable.Load(dr);
                }
            }


            PresTable.Columns.Add("n_file_path", typeof(string));
            PresTable.Columns.Add("n_file_csize", typeof(Int32));
            PresTable.Columns.Add("n_file_csize_16", typeof(Int32));
            PresTable.Columns.Add("n_file_usize", typeof(Int32));

            Dictionary<string, DataTable> map = new Dictionary<string, DataTable>();

            foreach (DataRow dr in PresTable.Rows)
            {

                string ext = dr["set_data_3_ext"].ToString();

                Log.Info($"获取扩展名为：{ext}");


                if (!map.ContainsKey(ext))
                {
                    Log.Info($"扩展名：{ext}，无表，建表！");
                    map.Add(ext, PresTable.Clone());
                }
                map[ext].ImportRow(dr);
                Log.Info($"表：{ext}导入{dr["set_data_3_complete"]}");
            }

            foreach (var kv in map)
            {
                FileInfo global = new FileInfo(RootDirectory + $"\\CACHE\\{kv.Key}.dat");

                if (global.Exists)
                {
                    global.Delete();
                }

                Directory.CreateDirectory(Path.GetDirectoryName(global.FullName));

                using (var stream = global.OpenWrite())
                {

                    using (IDataReader dr = kv.Value.CreateDataReader())
                    {
                        DataSerializer.Serialize(stream, dr);
                    }

                }
            }


        }



        public static void PreparingRES()
        {
            ResTable = new DataTable();

            List<KeyValuePair<FileInfo, DataRow[]>> query_list = new List<KeyValuePair<FileInfo, DataRow[]>>();

            using (FileStream fs = File.OpenRead(RootDirectory + "\\res.bin"))
            {
                using (IDataReader dr = DataSerializer.Deserialize(fs))
                {
                    ResTable.Load(dr);
                }
            }

            ResTable.Columns.Add("n_file_path", typeof(string));
            ResTable.Columns.Add("n_file_csize", typeof(Int32));
            ResTable.Columns.Add("n_file_csize_16", typeof(Int32));
            ResTable.Columns.Add("n_file_usize", typeof(Int32));

            DataTable resultTable = ResTable.Clone();

            var files = ResRealBLZ4Directory.GetFiles("*.*", SearchOption.AllDirectories);
            //foreach (var i in files)
            Parallel.ForEach<FileInfo>(files, (i) =>
            {
                string pack_address = i.FullName.Substring(ResRealBLZ4Directory.FullName.Length + 1);
                Log.Info($"获取相对地址:{pack_address}");
                string game_pack_address = pack_address.Replace('\\', '/');
                Log.Info($"获取游戏存储地址:{game_pack_address}");

                string ext = Path.GetExtension(pack_address).Substring(1);
                Log.Info($"获取扩展名:{ext}");

                DataTable query_dt;


                Log.Info("总表读取:");
                query_dt = ResTable;
                Log.Info($"select * from restable where set_data_3_complete='{game_pack_address}'");


                DataRow[] query;


                query = query_dt.Select($"set_data_3_complete='{game_pack_address}'");

                Log.Info($"查找数量结果：{query.Length}");

                if (query.Length > 0)
                {

                    Log.Info($"找到{i.FullName}");
                    lock (query_list)
                    {
                        query_list.Add(new KeyValuePair<FileInfo, DataRow[]>(i, query));
                    }

                }
                else
                {
                    Log.Info($"没有在res.bin找到{i.FullName}");
                }
            });


            foreach (var query in query_list)
            {

                foreach (var item in query.Value)
                {
                    item["n_file_path"] = query.Key.FullName;

                    int n_file_csize = Convert.ToInt32(query.Key.Length);
                    int n_file_csize_16 = n_file_csize;
                    item["n_file_csize"] = n_file_csize_16;

                    if (n_file_csize_16 % 16 != 0)
                    {
                        n_file_csize_16 = n_file_csize_16 / 16;
                        n_file_csize_16 += 1;
                        n_file_csize_16 *= 16;
                    }
                    else
                    {
                        n_file_csize_16 = n_file_csize_16 / 16;
                        n_file_csize_16 *= 16; //我这写的什么垃圾代码，我看了我都想吐，真累啊，有没有大佬给弟弟做做汉化啊，我不想过年的时候都在做这个东西啊！ 不想动脑子了就这样吧毁灭吧！！！
                    }
                    Log.Info($"理论分配区域为：{n_file_csize_16}");
                    item["n_file_csize_16"] = n_file_csize_16;

                    using (BinaryReader br = new BinaryReader(query.Key.OpenRead()))
                    {
                        int magic = br.ReadInt32();

                        if (magic == Define.BLZ4_MAGIC)
                        {
                            int u_size = br.ReadInt32();

                            item["n_file_usize"] = u_size;
                            Log.Info($"是个BLZ4，因此文件大小是{item["n_file_usize"]}");
                        }
                        else
                        {
                            item["n_file_usize"] = n_file_csize;
                            Log.Info($"不是个BLZ4，因此文件大小{item["n_file_usize"]}和csize一样。");
                        }


                    }

                    resultTable.ImportRow(item);
                    Log.Info($"添加{item["file_name"]}");
                }
            }

            Dictionary<string, PresAppender> file_maps = new Dictionary<string, PresAppender>();
            Dictionary<string, string> file_path_maps = new Dictionary<string, string>();
            foreach (DataRow dr in resultTable.Rows)
            {
                string key = dr["file_name"].ToString();

                Log.Info($"现在读取{key}");

                if (file_maps.ContainsKey(key))
                {

                    file_maps[key].AppendNewFile(dr);
                    Log.Info($"字典有{key}，启动PDD系统，再次拼合文件。");



                }
                else
                {
                    Log.Info($"字典无{key}，读取新文件并存进去。");
                    byte[] read_bytes = File.ReadAllBytes(dr["file_path"].ToString());
                    Log.Info($"新文件读取到的大小为：{read_bytes.Length}。");
                    file_maps.Add(key, new PresAppender(read_bytes));
                    file_path_maps.Add(key, dr["file_path"].ToString());
                    int count_set = Convert.ToInt32(dr["count_set"].ToString());
                    int set_data_3_file_count = Convert.ToInt32(dr["set_data_3_file_count"].ToString());

                    int pres_count = resultTable.Select($"file_name='{dr["file_name"].ToString()}'").Length;

                    Log.Info($"{dr["file_name"].ToString()}，有{count_set}个集合，集合{dr["set_index"].ToString()}有:{set_data_3_file_count}个文件。");
                    Log.Info($"这个Pres应该有多少个文件需要被打包？答案是：{pres_count}");

                    if (count_set == 1 && set_data_3_file_count == 1 && pres_count == 1 && UseSingleFileAppender)
                    {
                        Log.Info($"字典有{key}，而且有{count_set}个集合和{set_data_3_file_count}个文件，该文件也只有{pres_count}个需要被打包的文件，而且单文件去尾拼合的状态是：{UseSingleFileAppender}，所以启动YS系统，去末尾+拼合文件。");
                        file_maps[key].RemoveLastFileAndAppendNewFile(dr);

                    }
                    else
                    {
                        Log.Info($"启动PDD系统，拼合文件。");
                        file_maps[key].AppendNewFile(dr);

                    }
                }





            }

            foreach (var i in file_maps)
            {

                string target = file_path_maps[i.Key];

                Log.Info($"原始res：{target}");
                target = target.Replace("_UNPACK", "_UNPACK_PRES_REAL_EXTRA_BLZ4");
                Log.Info($"矫正res：{target}");

                Directory.CreateDirectory(Path.GetDirectoryName(target));

                i.Value.SaveFile(target);


            }




        }

        public static void PreparingPRES()
        {

            var dats = Directory.GetFiles(RootDirectory + "\\CACHE", "*.dat", SearchOption.AllDirectories);
            Log.Info($"找到{dats.Length}个缓存。");


            Dictionary<string, DataTable> cache_map = new Dictionary<string, DataTable>();

            foreach (var dat in dats)
            {
                using (FileStream fs = File.OpenRead(dat))
                {
                    var key = Path.GetFileNameWithoutExtension(dat);
                    using (IDataReader dr = DataSerializer.Deserialize(fs))
                    {
                        DataTable temp = new DataTable();
                        temp.Load(dr);

                        cache_map.Add(key, temp);
                    }
                    Log.Info($"已读取缓存{dat}，对应键为：{key}");
                }
            }

            PresTable = new DataTable();

            using (FileStream fs = File.OpenRead(RootDirectory + "\\pres.bin"))
            {
                using (IDataReader dr = DataSerializer.Deserialize(fs))
                {
                    PresTable.Load(dr);
                }
            }

            PresTable.Columns.Add("n_file_path", typeof(string));
            PresTable.Columns.Add("n_file_csize", typeof(Int32));
            PresTable.Columns.Add("n_file_csize_16", typeof(Int32));
            PresTable.Columns.Add("n_file_usize", typeof(Int32));



            DataTable resultTable = PresTable.Clone();

            List<KeyValuePair<FileInfo, DataRow[]>> query_list = new List<KeyValuePair<FileInfo, DataRow[]>>();

            //var files_1 = PresRealBLZ4Directory.GetFiles("*.*",SearchOption.AllDirectories);
            //var files_2 = PresRealNOBLZ4Directory.GetFiles("*.*", SearchOption.AllDirectories);
            //Log.Info($"压缩过的文件：{files_1.Length}");
            //Log.Info($"未压缩过的文件：{files_2.Length}");

            //var files = files_1.Concat(files_2).ToArray();
            var files = PresRealBLZ4Directory.GetFiles("*.*", SearchOption.AllDirectories);
            //foreach (var i in files)
            Parallel.ForEach<FileInfo>(files, (i) =>
            {
                string pack_address = i.FullName.Substring(PresRealBLZ4Directory.FullName.Length + 1);
                Log.Info($"获取相对地址:{pack_address}");
                string game_pack_address = pack_address.Replace('\\', '/');
                Log.Info($"获取游戏存储地址:{game_pack_address}");

                string ext = Path.GetExtension(pack_address).Substring(1);
                Log.Info($"获取扩展名:{ext}");

                DataTable query_dt;

                if (cache_map.ContainsKey(ext))
                {
                    Log.Info("缓存读取:");
                    query_dt = cache_map[ext];
                    Log.Info($"select * from {ext} where set_data_3_complete='{game_pack_address}'");
                }
                else
                {
                    if (!OnlyUseCacheExt)
                    {
                        Log.Info("总表读取:");
                        query_dt = PresTable;
                        Log.Info($"select * from prestable where set_data_3_complete='{game_pack_address}'");
                    }
                    else
                    {
                        Log.Info($"{game_pack_address}好像不是个pres里的文件，跳过。");
                        return;
                    }


                }

                DataRow[] query;


                query = query_dt.Select($"set_data_3_complete='{game_pack_address}'");

                Log.Info($"查找数量结果：{query.Length}");

                if (query.Length > 0)
                {

                    Log.Info($"找到{i.FullName}");
                    lock (query_list)
                    {
                        query_list.Add(new KeyValuePair<FileInfo, DataRow[]>(i, query));
                    }

                }
                else
                {
                    Log.Info($"没有在pres.bin找到{i.FullName}");
                }
            });


            foreach (var query in query_list)
            {

                foreach (var item in query.Value)
                {
                    item["n_file_path"] = query.Key.FullName;

                    int n_file_csize = Convert.ToInt32(query.Key.Length);
                    int n_file_csize_16 = n_file_csize;
                    item["n_file_csize"] = n_file_csize_16;

                    if (n_file_csize_16 % 16 != 0)
                    {
                        n_file_csize_16 = n_file_csize_16 / 16;
                        n_file_csize_16 += 1;
                        n_file_csize_16 *= 16;
                    }
                    else
                    {
                        n_file_csize_16 = n_file_csize_16 / 16;
                        n_file_csize_16 *= 16; //我这写的什么垃圾代码，我看了我都想吐，真累啊，有没有大佬给弟弟做做汉化啊，我不想过年的时候都在做这个东西啊！ 不想动脑子了就这样吧毁灭吧！！！
                    }
                    Log.Info($"理论分配区域为：{n_file_csize_16}");
                    item["n_file_csize_16"] = n_file_csize_16;

                    using (BinaryReader br = new BinaryReader(query.Key.OpenRead()))
                    {
                        int magic = br.ReadInt32();

                        if (magic == Define.BLZ4_MAGIC)
                        {
                            int u_size = br.ReadInt32();

                            item["n_file_usize"] = u_size;
                            Log.Info($"是个BLZ4，因此文件大小是{item["n_file_usize"]}");
                        }
                        else
                        {
                            item["n_file_usize"] = n_file_csize;
                            Log.Info($"不是个BLZ4，因此文件大小{item["n_file_usize"]}和csize一样。");
                        }


                    }

                    resultTable.ImportRow(item);
                    Log.Info($"添加{item["file_name"]}");
                }
            }


            FileInfo excel = new FileInfo(RootDirectory + "\\packer.xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, resultTable);
            }

            using (FileStream fs = File.OpenWrite(RootDirectory + "\\packer.bin"))
            {
                using (IDataReader dr = resultTable.CreateDataReader())
                {
                    DataSerializer.Serialize(fs, dr);
                }
            }

            //PreparingFullPRES(resultTable);


        }

        public static void PreparingFullPRES(DataTable linkedDT)
        {

            DataTable resultTable = PresTable.Clone();

            HashSet<string> pres_paths = new HashSet<string>();

            foreach (DataRow row in linkedDT.Rows)
            {
                pres_paths.Add(row["file_name"].ToString());
                Log.Info($"哈希表添加：{row["file_name"].ToString()}");
            }

            foreach (var name in pres_paths)
            {

                Log.Info($"select * from prestable where file_name='{name}'");

                var query = PresTable.Select($"file_name='{name}'");
                if (query.Length > 0)
                {

                    Log.Info($"找到{query.Length}个pres数据条目");

                    foreach (var item in query)
                    {
                        resultTable.ImportRow(item);
                        Log.Info($"添加{item["file_name"]},对应文件:{item["set_data_3_complete"]}");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"反查询错误:{name}不在pres.bin的记录里！");
                }


            }



            FileInfo excel = new FileInfo(RootDirectory + "\\packer_full.xlsx");
            if (excel.Exists)
            {

                excel.Delete();
            }

            using (var stream = excel.OpenWrite())
            {
                MiniExcel.SaveAs(stream, resultTable);
            }


        }

        public static void ProcessPRESBLZ4()
        {
            var files = PresRealDirectory.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (var i in files)
            {

                string name = i.Name;

                byte[] data = BLZ4(i.FullName);

                string path = i.FullName.Replace("_UNPACK_PRES_REAL_EXTRA", "_UNPACK_PRES_REAL_EXTRA_BLZ4");

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllBytes(path, data);

            }

            files = ResRealDirectory.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (var i in files)
            {

                string name = i.Name;

                byte[] data = BLZ4(i.FullName);

                string path = i.FullName.Replace("_UNPACK_RES_REAL_EXTRA", "_UNPACK_RES_REAL_EXTRA_BLZ4");

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllBytes(path, data);

            }

        }


        public static void ProcessQPCKBLZ4()
        {
            var files = QpckBlz4Directory.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (var i in files)
            {

                string name = i.Name;

                byte[] data = BLZ4(i.FullName);

                File.WriteAllBytes(ExtraDirectory + "\\" + name, data);

            }



        }


        public static void QPCK()
        {

            var base_files = BaseDirectory.GetFiles("*.*", SearchOption.AllDirectories);
            var extra_files = ExtraDirectory.GetFiles("*.*", SearchOption.AllDirectories);

            Dictionary<string, string> map = new Dictionary<string, string>();


            FileInfo qpck = new FileInfo(RootDirectory.FullName + "\\pack.qpck");

            if (qpck.Exists)
            {

                qpck.Delete();

            }




            //foreach (var i in base_files)
            //{
            //    map.Add(i.Name, i.FullName);
            //    Log.Info($"原文件:{i.Name},地址：{i.FullName}");
            //}

            Parallel.ForEach<FileInfo>(base_files, i => {

                lock (map) { 
                map.Add(i.Name, i.FullName);
                }
                Log.Info($"原文件:{i.Name},地址：{i.FullName}");

            });

            //foreach (var i in extra_files)
            //{
            //    if (map.ContainsKey(i.Name))
            //    {
            //        map[i.Name] = i.FullName;
            //        Log.Info($"新文件:{i.Name},地址：{i.FullName}，劫持！");
            //    }
            //    else
            //    {
            //        throw new FileNotFoundException($"没找到{i.FullName}，你确定这个东西是游戏的一部分吗？");
            //    }


            //}

            Parallel.ForEach<FileInfo>(extra_files, i => {


                lock (map)
                {
                    if (map.ContainsKey(i.Name))
                    {
                        map[i.Name] = i.FullName;
                        Log.Info($"新文件:{i.Name},地址：{i.FullName}，劫持！");
                    }
                    else
                    {
                        throw new FileNotFoundException($"没找到{i.FullName}，你确定这个东西是游戏的一部分吗？");
                    }
                }


            
            });

            //long offset = br.ReadInt64();
            //long hash = br.ReadInt64();
            //int size = br.ReadInt32();
            //8header+8+8+4

            int files_count = base_files.Length;
            int info_chunk_size = files_count * 20;
            int data_chunk_size = 0;
            int header_size = 8;



            Log.Info($"虚拟文件：信息块大小：{info_chunk_size}");

            using (BinaryWriter bw = new BinaryWriter(qpck.OpenWrite()))
            {
                bw.Write(Define.QPCK_MAGIC);
                bw.Write(files_count);

                List<FileInfo> files = new List<FileInfo>();

                foreach (var kv in map)
                {
                    FileInfo file = new FileInfo(kv.Value);
                    files.Add(file);
                    long file_offset = header_size + info_chunk_size + data_chunk_size;
                    string str_hash = file.Name.Split('.')[0].Split('_')[1];
                    long int_hash = Convert.ToInt64(str_hash, 16);
                    int file_length = Convert.ToInt32(file.Length);
                    Log.Info($"虚拟文件{kv.Value}：头信息-推算偏移：{file_offset}，哈希：{int_hash}:{str_hash}，大小：{file_length}");

                    bw.Write(file_offset);
                    bw.Write(int_hash);
                    bw.Write(file_length);
                    data_chunk_size += file_length;



                }

                foreach (var i in files)
                {

                    byte[] bytes = File.ReadAllBytes(i.FullName);
                    Log.Info($"写入数据：{bytes.Length}，来自文件:{i.FullName}");
                    bw.Write(bytes);



                }


            }


        }



        public static byte[] BLZ4(string path)
        {

            using (MemoryStream ms = new MemoryStream())
            {

                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    byte[] file_data = File.ReadAllBytes(path);

                    bw.Write(Define.BLZ4_MAGIC);
                    bw.Flush();
                    Log.Info($"写入魔术码{Define.BLZ4_MAGIC}");

                    bw.Write(Convert.ToInt32(file_data.Length));
                    Log.Info($"写入文件大小{file_data.Length}");
                    bw.Write(0L);
                    bw.Flush();
                    Log.Info($"写入0L");
                    Log.Info($"当前指针位置：{bw.BaseStream.Position}");
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file_data);
                    Log.Info($"写入MD5:{CryptUtils.GetMD5HashFromBytes(file_data)}");
                    bw.Write(retVal);
                    bw.Flush();
                    Log.Info($"当前指针位置：{bw.BaseStream.Position}");
                    var split_data = BLZ4Utils.SplitBytes(file_data, 32768);
                    Log.Info($"即将写入压缩数据：总块数：{split_data.Count}");


                    byte[] compress;
                    //BLZ4Utils.CompressData(split_data[0], out compress);

                    //byte[] check_compress;
                    //BLZ4Utils.DecompressData(compress, out check_compress);

                    //Log.Info($"验证：{check_compress.Length}");

                    //Log.Info($"当前指针位置：{bw.BaseStream.Position}，写入大小。");
                    //bw.Write(Convert.ToUInt16(compress.Length));
                    //bw.Flush();
                    //Log.Info($"当前指针位置：{bw.BaseStream.Position}");
                    //bw.Write(compress);
                    //bw.Flush();
                    //Log.Info($"顺序写入区块，大小：{compress.Length}");
                    //Log.Info($"当前指针位置：{bw.BaseStream.Position}");


                    //for (int i = 1; i < split_data.Count; i++)
                    //{
                    //    BLZ4Utils.CompressData(split_data[i], out compress);
                    //    Log.Info($"当前指针位置：{bw.BaseStream.Position}，写入大小。");
                    //    bw.Write(Convert.ToUInt16(compress.Length));
                    //    bw.Flush();
                    //    Log.Info($"当前指针位置：{bw.BaseStream.Position}");
                    //    bw.Write(compress);
                    //    bw.Flush();
                    //    Log.Info($"顺序写入区块，大小：{compress.Length}");
                    //    Log.Info($"当前指针位置：{bw.BaseStream.Position}");
                    //}



                    if (split_data.Count > 1)
                    {
                        BLZ4Utils.CompressData(split_data[split_data.Count - 1], out compress);

                        bw.Write(Convert.ToUInt16(compress.Length));
                        bw.Write(compress);
                        Log.Info($"写入最后一块到头部：大小：{split_data[split_data.Count - 1].Length}");

                        for (int i = 0; i < split_data.Count - 1; i++)
                        {

                            BLZ4Utils.CompressData(split_data[i], out compress);

                            bw.Write(Convert.ToUInt16(compress.Length));
                            bw.Write(compress);
                            Log.Info($"顺序写入区块，大小：{split_data[i].Length}");

                        }
                    }
                    else
                    {
                        for (int i = 0; i < split_data.Count; i++)
                        {


                            BLZ4Utils.CompressData(split_data[i], out compress);

                            bw.Write(Convert.ToUInt16(compress.Length));
                            bw.Write(compress);
                            Log.Info($"顺序写入区块，大小：{split_data[i].Length}");

                        }
                    }





                    long limit = bw.BaseStream.Position;
                    ms.Seek(0, SeekOrigin.Begin);

                    byte[] result = new byte[limit];
                    ms.Read(result, 0, result.Length);
                    Log.Info($"返回数据:{result.Length}，压缩比率：{(double)result.Length / (double)file_data.Length}");
                    return result;


                }
            }

        }


    }
}
