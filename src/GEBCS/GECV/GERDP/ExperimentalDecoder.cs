using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GECV.Utils;
using static GECV.Log;
using GECV_Extend;
using GECV;
using System.Net;

namespace GERDP
{



    internal class ExperimentalDecoder : IDecoder
    {

        //public static List<string> LogList = new List<string>();

        //public static List<string> warningLogList = new List<string>();

        public long size_mul = 1;

        public static HashSet<byte[]> ByteHashSet = new HashSet<byte[]>();

        public string folder_name;

        public ResDataSet set;

        private static object FileWriterLocker = new object();


        public void Decode(ResDataSet set, byte[] res_data) //这个level不想改了，就这么摆烂
        {
            this.set = set;
            BinaryReader br = GetBinaryReader(res_data);

            


            string res_relative_path = Path.GetRelativePath(Program.TargetDirectiory.FullName, this.folder_name);

            Program.DGML.AddNode(new DGMLWriter.Node(res_relative_path, res_relative_path));



            br.BaseStream.Seek(set.address, SeekOrigin.Begin);
            long header_p = br.BaseStream.Position;
            for (int i = 0; i < set.count; i++)
            {

                DataLocation location = new DataLocation(set.isPS4 ? br.ReadInt64() : br.ReadInt32(), res_data, set.isPS4);

                Info($"[信息][{set.name}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.的十六进制地址是{location.hex_origin_fix}（内存状况：{location.hex_origin}），实际偏移是:{location.real_offset.ToString("X")}，这个对象应该来自{location.GetDataLocationName()}。");

                if (location.status == DataLocationStatus.UNK)
                {

                    Error($"[意外错误][{res_relative_path}][{set.name}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.{location.hex_origin}是一个意外的地址！放弃处理这个对象。");
                    continue;

                }

                if (location.status == DataLocationStatus.NoSet_3)
                {
                    Warning($"[警告][{res_relative_path}][{set.name}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.{location.hex_origin}是包外对象，不处理。");
                    continue;
                }



                long file_size = set.isPS4 ? br.ReadInt64() : br.ReadInt32();
                long file_name_offset = set.isPS4 ? br.ReadInt64() : br.ReadInt32();
                long file_name_count = set.isPS4 ? br.ReadInt64() : br.ReadInt32();
                long unk_1 = br.ReadInt32();
                long unk_2 = br.ReadInt32();
                long unk_3 = br.ReadInt32();

                file_size *= size_mul;
                long unk_4 = set.isPS4 ? br.ReadInt32() : 0;
                long unk_5 = set.isPS4 ? br.ReadInt32() : 0;

                long file_real_size = set.isPS4 ? br.ReadInt64() : br.ReadInt32();
                header_p = br.BaseStream.Position;
                Info($"[信息][{set.name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.下一个文件{i + 1 + 1}的地址应该在{header_p.ToString("X")}");
                Info($@"[信息][{set.name}][{br.BaseStream.Position.ToString("X")}]读到的头信息如下：
                 文件信息头位置：{location.CalcRealOffset().ToString("X")}
                 文件包位置：{location.GetDataLocationName()}
                 文件大小：{file_size}
                 文件名字偏移：{file_name_offset.ToString("X")}
                 文件名字数量：{file_name_count}
                 未知1：{unk_1.ToString("X")}
                 未知2：{unk_2.ToString("X")}
                 未知3：{unk_3.ToString("X")}
                 未知4(PS4)：{unk_4.ToString("X")}
                 未知5(PS4)：{unk_5.ToString("X")}
                 最终文件大小：{file_real_size}");


                if (location.status == DataLocationStatus.Package_4 && location.data.Length <= 0)
                {
                    Warning($"[警告][{set.name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.也许来自package.rdp但是文件不存在。");
                    continue;
                }
                else

                if (location.status == DataLocationStatus.Data_5 && location.data.Length <= 0)
                {
                    Warning($"[警告][{set.name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.也许来自data.rdp但是文件不存在。");
                    continue;
                }
                else

                if (location.status == DataLocationStatus.Patch_6 && location.data.Length <= 0)
                {
                    Warning($"[警告][{set.name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.也许来自patch.rdp但是文件不存在。");
                    continue;
                }

                if (file_name_count >= 6)
                {
                    Warning($"[警告][{set.name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.的数量也许不正确，读到的数量：{file_name_count}，跳过。");
                    continue;
                }

                br.BaseStream.Seek(file_name_offset, SeekOrigin.Begin);

                FileInformationData fid = GetOriginData(br, file_name_count);
                Info($"[信息][{set.name}][{br.BaseStream.Position.ToString("X")}]读取到的内容：\n{GECV.Utils.GetListStringPrintData(fid.ORIGIN_DATA)}");
                br.BaseStream.Seek(header_p, SeekOrigin.Begin);

                Task.Run(() =>
                {

                    BinaryReader remote_br = GetBinaryReader(location.data);

                    remote_br.BaseStream.Seek(location.CalcRealOffset(), SeekOrigin.Begin);

                    Info($"[信息][{set.name}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.远端读取器的地址是{remote_br.BaseStream.Position.ToString("X")}，原始地址是{location.real_offset.ToString("X")}，这个文件在:{location.GetDataLocationName()}。");

                    List<byte> fun_data = new List<byte>();
                    int fun_i = 0;
                    try
                    {

                        for (; fun_i < file_size; fun_i++)
                        {

                            fun_data.Add(remote_br.ReadByte());

                        }
                    }
                    catch (Exception e)
                    {
                        Error($"[错误][{set.name}][{res_relative_path}][{remote_br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.指针位置{remote_br.BaseStream.Position.ToString("X")}，应该读取的长度:{file_size.ToString("X")}，实际读取的长度:{fun_i.ToString("X")}，原始数据长度:{remote_br.BaseStream.Length.ToString("X")}，上游指针位置:{br.BaseStream.Position.ToString("X")}，错误信息：{e.Message}");
                    }




                    byte[] read_data = fun_data.ToArray();

                    byte[] save_data;

                    if (BLZ4FileDecompresser.IsBLZ4(read_data))
                    {
                        BLZ4FileDecompresser blz4 = new BLZ4FileDecompresser(read_data);
                        Info($"[信息][{set.name}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.是BLZ4文件，解压缩！");
                        save_data = blz4.GetByteResult();
                    }
                    else
                    {
                        Info($"[信息][{set.name}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.不是BLZ4文件。");
                        save_data = read_data;
                    }

                    //if (ByteHashSet.Contains(save_data))
                    //{
                    //    continue;
                    //}
                    //else
                    //{
                    //    ByteHashSet.Add(save_data);
                    //}

                    if (!String.IsNullOrEmpty(folder_name))
                    {
                        var file_folder = folder_name + "\\";
                        var file_name = "";
                        if (!String.IsNullOrEmpty(fid.name_1))
                        {
                            file_name += fid.name_1;
                        }
                        else
                        {
                            file_name += $"GERDP_{res_relative_path}_{set.name}_{i}.gerdp";
                        }

                        if (!String.IsNullOrEmpty(fid.ext_2))
                        {
                            file_name += '.' + fid.ext_2;
                        }



                        Program.DGML.AddNode(new DGMLWriter.Node(res_relative_path + '\\' + file_name, res_relative_path + '\\' + file_name));
                        Program.DGML.AddLink(new DGMLWriter.Link(res_relative_path, res_relative_path + '\\' + file_name, set.name, res_relative_path + '.' + set.name));

                        file_name = file_folder + file_name;

                        lock (FileWriterLocker)
                        {

                            if (File.Exists(file_name))
                            {
                                Warning($"[小型提示][{set.name}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.因为旧的文件所以没有存储存储{file_name}，新文件大小:{save_data.LongLength}");
                            }
                            else
                            {

                                    File.WriteAllBytes(file_name, save_data);
                                    Info($"[信息][{set.name}][{br.BaseStream.Position.ToString("X")}]{i + 1}/{set.count}.已存储{file_name}，文件大小:{save_data.LongLength}");
                                


                            }


                                if (fid.ORIGIN_DATA.Count > 2)
                                {
                                    try
                                    {
                                        File.WriteAllLines(file_name + ".gerdp.ini", fid.ORIGIN_DATA);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }

                            

                        }
                    }



                    if (!ByteHashSet.Contains(save_data) && IsRES(save_data))
                    {
                        ByteHashSet.Add(save_data);
                        NextRes(fid.name_1, save_data, set.isPS4);
                    }

                });


            }




        }

        /*
     *          4字节 文件偏移 ？ 要命，终于知道了。
     *          4字节 文件大小
     *          4字节 文件名字地址
     *          4字节 文件名字数量
     *          4字节 未知1
     *          4字节 未知2
     *          4字节 未知3
     *          PS4:
     *          4字节 未知4 4字节 未知5
     *          4字节 实际文件大小
*/


        private void NextRes(string title, byte[] data, bool isPS4)
        {

            Res res = new Res(title, data, isPS4);

            res.SetDecoderSaveFolder(this.folder_name);



            res.DecodeAll();

        }

        public static bool IsRES(byte[] data)
        {
            if (data.Length < 4)
            {
                return false;
            }

            using (MemoryStream input_ms = new MemoryStream(data))
            {

                using (BinaryReader input_br = new BinaryReader(input_ms))
                {
                    int magic = input_br.ReadInt32();

                    if (magic != Define.PRES_MAGIC)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }





        public FileInformationData GetOriginData(BinaryReader br, long count)
        {
            FileInformationData fid = new FileInformationData();

            fid.ORIGIN_COUNT = count;
            fid.ORIGIN_DATA = new List<string>();

            long p = br.BaseStream.Position;
            //Info($"[测试]获取文件信息：流位置：{br.BaseStream.Position}，流长度:{br.BaseStream.Length}。");
            for (int i = 0; i < fid.ORIGIN_COUNT; i++)
            {

                long address = set.isPS4 ? br.ReadInt64() : br.ReadInt32();

                p = br.BaseStream.Position;

                //Info($"{i}/{fid.ORIGIN_COUNT}文件名称地址:{address.ToString("X")}");
                br.BaseStream.Seek(address, SeekOrigin.Begin);

                fid.ORIGIN_DATA.Add(ReadStringData(br));
                br.BaseStream.Seek(p, SeekOrigin.Begin);
            }

            if (fid.ORIGIN_DATA.Count >= 1)
            {
                fid.name_1 = fid.ORIGIN_DATA[0];
            }

            if (fid.ORIGIN_DATA.Count >= 2)
            {
                fid.ext_2 = fid.ORIGIN_DATA[1];
            }

            if (fid.ORIGIN_DATA.Count >= 3)
            {
                fid.unk_3 = fid.ORIGIN_DATA[2];
            }

            if (fid.ORIGIN_DATA.Count >= 4)
            {
                fid.function_4 = fid.ORIGIN_DATA[3];
            }

            if (fid.ORIGIN_DATA.Count >= 5)
            {
                fid.function_args_5 = fid.ORIGIN_DATA[4];
            }


            return fid;


        }


        public string ReadStringData(BinaryReader br)
        {


            List<byte> list = new List<byte>();
            byte b;
            while ((b = br.ReadByte()) != 0x00 && br.BaseStream.Position < br.BaseStream.Length)
            {
                list.Add(b);
            }


            return Encoding.UTF8.GetString(list.ToArray());

        }


    }
}
