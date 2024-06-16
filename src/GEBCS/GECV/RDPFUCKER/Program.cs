using GECV;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;


namespace RDPFUCKER
{




    internal class Program
    {

        enum ReaderStatus
        {

            BLANK, PRES, BLZ4

        }




        static byte[] rdp_file;

        static byte[] pres_header = { 0x50, 0x72, 0x65, 0x73 };
        static byte[] blz4_header = { 0x62, 0x6c, 0x7a, 0x34 };

        static int pres_header_count = 0;
        static int blz4_header_count = 0;

        static ReaderStatus global_status = ReaderStatus.BLANK;
        static long global_count = 0;

        static DirectoryInfo target_dir;

        static void Main(string[] args)
        {
            Log.Info("CODE EATER 噬神者 RDP 解包器 BY 兰德里奥（HaoJun0823）");
            Log.Info("https://blog.haojun0823.xyz/");
            Log.Info("https://github.com/HaoJun0823/GECV");
            Programs_Extend.Virtual_MainAsync(args);

            Log.Info("程序已经结束，按任意键继续。");
            Console.ReadKey();

            return;

            if (args.Length != 2)
            {


                throw new ArgumentException($"需要两个参数：第一个，rdp文件，第二个，解压的文件夹。");
            }


            FileInfo file = new FileInfo(args[0]);
            target_dir = new DirectoryInfo(args[1]);

            if (!target_dir.Exists)
            {
                target_dir.Create();
            }



            if (file.Length % 16 != 0)
            {
                Log.Info($"RDP长度可能不对！：{file.Length}无法被16整除！要不你自己补几个0？");
            }

            rdp_file = File.ReadAllBytes(args[0]);

            Log.Info($"已经读取：{rdp_file.Length}，开始解包。");
            Work();

            Console.ReadKey();
        }


        static void WriteFile(List<byte> data, string path)
        {

            if (File.Exists(path))
            {
                File.Delete(path);
            }


            File.WriteAllBytes(path, data.ToArray());

            Log.Info($"已经写入{data.Count}大小的文件于:{path}");
            global_count++;
        }

        static void Work()
        {

            Log.Info("主线程启动！");
            // 0x50,0x72,0x65,0x73 0x20 0x00 0x00 0x00 Pres
            //status_map.Add(new byte[] {0x50,0x72,0x65,0x73 },0);

            //BLZ4
            //status_map.Add(new byte[] { 0x62,0x6c,0x7a,0x34 }, 0);



            using (MemoryStream ms = new MemoryStream(rdp_file))
            {


                using (BinaryReader br = new BinaryReader(ms))
                {


                    do
                    {
                        byte b = br.ReadByte();
                        //Log.Info($"读取到了{b}。");
                        global_status = GetReaderStatus(b);


                        if (global_status == ReaderStatus.BLANK)
                        {

                        }


                        if (global_status == ReaderStatus.BLZ4)
                        {
                            //br.BaseStream.Seek(-4, SeekOrigin.Current);
                            GetTier0BLZ4(br);
                            //Console.ReadKey();





                        }

                        if(global_status == ReaderStatus.PRES)
                        {
                            //global_status = ReaderStatus.BLANK;
                            //GetPresFileCount(br);
                           global_status = ReaderStatus.BLANK;



                        }

                    } while (br.BaseStream.Position < rdp_file.LongLength);




                }






            }




        }


        //status是BLZ4，切回BLANK，直到下一个状态出现为止

        static int GetPresFileCount(BinaryReader br)
        {

            //记录基础地址

            br.BaseStream.Seek(-4, SeekOrigin.Current);

            long original_address = br.BaseStream.Position;
            int magic = br.ReadInt32();
            int magic_space = br.ReadInt32();
            int magic_1 = br.ReadInt32();
            int magic_2 = br.ReadInt32();
            int magic_3 = br.ReadInt32();
            long offset_data = br.ReadInt32();
            long zerozero = br.ReadInt64();
            int count_set = br.ReadInt32();

            long index_root = br.BaseStream.Position;
            long index_set = 0;
            long index_file = 0;

            Log.Info($"错误的魔法码1:{magic_1}");
            Log.Info($"错误的魔法码2:{magic_2}");
            Log.Info($"错误的魔法码3:{magic_3}");
            Log.Info($"错误的偏移数据:{offset_data}");
            Log.Info($"错误的zerozero:{zerozero}");

            Log.Info($"错误的集合数量:{count_set}");
            offset_data =  original_address + 0x20;
            count_set = 8;
            Log.Info($"修正偏移数据:{offset_data}，固定为{count_set}个集合");

            




            return 0;




        }

        static void GetTier0BLZ4(BinaryReader br)
        {

            global_status = ReaderStatus.BLANK;

            List<byte> list = new List<byte>();

            list.Add(blz4_header[0]);
            list.Add(blz4_header[1]);
            list.Add(blz4_header[2]);
            list.Add(blz4_header[3]);

            long base_address = br.BaseStream.Position;
            byte b;
            do
            {
                b = br.ReadByte();
                list.Add(b);


                if (list.Count % 1024 == 0)
                {
                    Log.Info($"【BLZ4】目前读到了：{list.Count / 1024}。");
                }
                global_status = GetReaderStatus(b);
            } while (global_status == ReaderStatus.BLANK);

            Log.Info($"【BLZ4】读到了其他文件，状态为：{global_status}，文件长度：{list.Count}。");
            list.RemoveAt(list.Count-1);
            list.RemoveAt(list.Count-1);
            list.RemoveAt(list.Count-1);
            list.RemoveAt(list.Count-1);

            Log.Info($"【BLZ4】去头，文件长度：{list.Count}。");
            //因为BLZ4是有块长度的所以末尾根本不需要考虑00的问题。

            if (list.Count % 16 != 0)
            {
                Log.Info($"警告，这个长度不是16的倍数，可能有问题？");
            }

            br.BaseStream.Seek(-4, SeekOrigin.Current);

            global_status = ReaderStatus.BLANK;
            WriteFile(list, $"{target_dir}{global_count.ToString("X8")}_{base_address.ToString("X8")}_{br.BaseStream.Position.ToString("X8")}_{list.Count.ToString("X8")}.blz4");


        }

        static ReaderStatus GetReaderStatus(byte b)
        {

            if (global_status != ReaderStatus.BLANK)
            {
                return global_status;
            }


            if (b == pres_header[pres_header_count])
            {
                pres_header_count++;
            }
            else
            {
                pres_header_count = 0;
            }

            if (b == blz4_header[blz4_header_count])
            {
                blz4_header_count++;
            }
            else
            {
                blz4_header_count = 0;
            }


            if (pres_header_count == pres_header.Length)
            {
                pres_header_count = 0;
                return ReaderStatus.PRES;
            }

            if (blz4_header_count == blz4_header.Length)
            {
                blz4_header_count = 0;
                return ReaderStatus.BLZ4;
            }






            return ReaderStatus.BLANK;


        }





    }
}
