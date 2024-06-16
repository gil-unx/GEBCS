using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using GECV;
using GECV_Extend;
using static GECV.Log;
using static GECV.Utils;
using static GECV.CryptUtils;
using static GECV.BLZ4Utils;


namespace GERDP_RE
{
    internal class Rtbl
    {

        FileInfo data_file;
        bool IsPS4;

        public string folder_name;

        public Rtbl(FileInfo data_file,bool IsPS4)
        {
            this.data_file = data_file;
            this.IsPS4 = IsPS4;
        }

        public void Decode()
        {
            BinaryReader br = GetBinaryReader(GetShareFileStream(data_file));


            string res_relative_path = Program.GetRelativePath(Program.TargetDirectiory.FullName, this.data_file.FullName);

            //Program.DGML.AddNode(new DGMLWriter.Node(res_relative_path, res_relative_path));



            br.BaseStream.Seek(0, SeekOrigin.Begin);
            long header_p = br.BaseStream.Position;
            long set_count = br.BaseStream.Length / 800L;
            string set_name = data_file.Name + "_rtbl";
            

            for (int i = 0; i < set_count; i++)
            {
                try { 

                if ((br.BaseStream.Position+32>br.BaseStream.Length)||(br.BaseStream.Position+81>br.BaseStream.Length))
                {
                    Warning($"[合法错误][{res_relative_path}][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.{br.BaseStream.Position}已经要超过了文件的上限{br.BaseStream.Length}。如果是PSV版本，差距在32，如果是PS4版本，差距在81。");
                    return;
                }

                DataLocation location = new DataLocation(IsPS4 ? br.ReadInt64() : br.ReadInt32(), data_file, IsPS4);

                Info($"[信息][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.的十六进制地址是{location.hex_origin_fix}（内存状况：{location.hex_origin}），实际偏移是:{location.real_offset.ToString("X")}，这个对象应该来自{location.GetDataLocationName()}。");

                if (location.real_offset == 0L)
                {
                    Warning($"[合法错误][{res_relative_path}][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.{location.hex_origin}是0，跳过这个对象！");
                    continue;
                }

                if (location.status == DataLocationStatus.UNK)
                {

                    Error($"[意外错误][{res_relative_path}][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.{location.hex_origin}是一个意外的地址！放弃处理这个对象。");
                    continue;

                }

                if (location.status == DataLocationStatus.NoSet_3)
                {
                    Warning($"[警告][{res_relative_path}][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.{location.hex_origin}是包外对象，不处理。");
                    continue;
                }



                long file_size = IsPS4 ? br.ReadInt64() : br.ReadInt32();
                long file_name_offset = IsPS4 ? br.ReadInt64() : br.ReadInt32();
                long file_name_count = IsPS4 ? br.ReadInt64() : br.ReadInt32();
                long unk_1 = br.ReadInt32();
                long unk_2 = br.ReadInt32();
                long unk_3 = br.ReadInt32();

                
                long unk_4 = IsPS4 ? br.ReadInt32() : 0;
                long unk_5 = IsPS4 ? br.ReadInt32() : 0;

                long file_real_size = IsPS4 ? br.ReadInt64() : br.ReadInt32();
                header_p = br.BaseStream.Position;
                Info($"[信息][{set_name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.下一个文件{i + 1 + 1}的地址应该在{header_p.ToString("X")}");
                Info($@"[信息][{set_name}][{br.BaseStream.Position.ToString("X")}]读到的头信息如下：
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


                if (location.status == DataLocationStatus.Package_4 && !location.res_file.Exists)
                {
                    Warning($"[警告][{set_name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.也许来自package.rdp但是文件不存在。");
                    continue;
                }
                else

                if (location.status == DataLocationStatus.Data_5 && !location.res_file.Exists)
                {
                    Warning($"[警告][{set_name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.也许来自data.rdp但是文件不存在。");
                    continue;
                }
                else

                if (location.status == DataLocationStatus.Patch_6 && !location.res_file.Exists)
                {
                    Warning($"[警告][{set_name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.也许来自patch.rdp但是文件不存在。");
                    continue;
                }

                if (file_name_count >= 6)
                {
                    Warning($"[警告][{set_name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.的数量也许不正确，读到的数量：{file_name_count}，跳过。");
                    continue;
                }

                    if (file_name_offset < 0 || file_name_offset > br.BaseStream.Length)
                    {
                        Error($"[错误][{set_name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.的文件名偏移{file_name_offset}不正确，小于了文件长度或者超过了文件长度{br.BaseStream.Length}。");
                        continue;
                    }

                    br.BaseStream.Seek(file_name_offset, SeekOrigin.Begin);

                FileInformationData fid = GetOriginData(br, file_name_count);
                Info($"[信息][{set_name}][{br.BaseStream.Position.ToString("X")}]读取到的内容：\n{GECV.Utils.GetListStringPrintData(fid.ORIGIN_DATA)}");
                br.BaseStream.Seek(header_p, SeekOrigin.Begin);


                    

                Task.Run(() =>
                {


                    if (!String.IsNullOrEmpty(fid.function_4) && fid.function_4.Equals("EXCLUDE_RDP"))
                    {
                        Warning($"[警告][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.的信息4是EXCLUDE_RDP，这意味着这是一个不在解包范围内的数据，它的路径是：{fid.function_args_5}。");
                        return;
                    }
                    var file_folder = folder_name + "\\";
                    var file_name = "";

                    var regex = "([A-Za-z0-9_]+/)*([A-Za-z0-9_\\.])+";


                    if (!String.IsNullOrEmpty(fid.function_4) && Utils.IsVaildRegex(fid.function_4, regex))
                    {
                        file_name = fid.function_4.Replace('/', '\\');
                        file_name = file_folder + file_name;




                        var check_file_name = "";

                        if (!String.IsNullOrEmpty(fid.name_1))
                        {
                            check_file_name += fid.name_1;
                        }

                        if (!String.IsNullOrEmpty(fid.ext_2))
                        {
                            check_file_name += '.' + fid.ext_2;
                        }

                        if (!Path.GetFileName(file_name).Equals(check_file_name))
                        {
                            Warning($"[警告][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.{fid.function_4}是一个符合正则表达式{regex}的内容，最后输出的地址是:{file_name}，然而这却不满足文件名校验模块：{check_file_name}。");
                            file_name = "";
                        }
                        else
                        {
                            Info($"[提示信息][{set_name}][{br.BaseStream.Position.ToString("X")}].{fid.function_4}是一个符合正则表达式{regex}的内容，最后输出的地址是:{file_name}，通过了文件名校验模块：{check_file_name}。");
                        }

                    }


                    if (String.IsNullOrEmpty(file_name))
                    {
                        if (!String.IsNullOrEmpty(fid.name_1))
                        {
                            file_name += fid.name_1;
                        }
                        else
                        {
                            file_name += $"GERDP_{res_relative_path}_{set_name}_{i}.gerdp";
                        }

                        if (!String.IsNullOrEmpty(fid.ext_2))
                        {
                            file_name += '.' + fid.ext_2;
                        }

                        file_name = file_folder + file_name;
                    }

                    regex = @"^([a-zA-Z]:\\)([-\u4e00-\u9fa5\w\s.()~!@#$%^&()\[\]{}+=]+\\?)*$";

                    if (!Utils.IsVaildRegex(file_name, regex))
                    {
                        throw new FormatException($"{file_name}不满足{regex}!如果你发现这个问题，请报告给开发者！");
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(file_name));
                    }



                    if (File.Exists(file_name))
                    {
                        Warning($"[小型提示][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.是已经存在的文件:{file_name}，没有必要再次覆盖这个文件。");
                        return;
                    }

                    BinaryReader remote_br = GetBinaryReader(GetShareFileStream(location.res_file));

                    var calc_real_offset = location.CalcRealOffset();
                    if (calc_real_offset < 0 || calc_real_offset > remote_br.BaseStream.Length)
                    {
                        Error($"[错误][{set_name}][{res_relative_path}][{remote_br.BaseStream.Position.ToString("X")}]{i}/{set_count}.有一个正确的文件信息，但有一个错误的指针：{calc_real_offset.ToString("X")}，它的虚拟指针是{location.hex_origin}，它的错误是因为小于或者大于文件流{remote_br.BaseStream.Position.ToString("X")}。");
                        return;
                    }


                    remote_br.BaseStream.Seek(calc_real_offset, SeekOrigin.Begin);

                    Info($"[信息][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.远端读取器的地址是{remote_br.BaseStream.Position.ToString("X")}，原始地址是{location.real_offset.ToString("X")}，这个文件在:{location.GetDataLocationName()}。");

                    List<byte> fun_data = new List<byte>();
                    long fun_i = 0;
                    try
                    {

                        for (; fun_i < file_size; fun_i++)
                        {

                            fun_data.Add(remote_br.ReadByte());

                        }
                    }
                    catch (Exception e)
                    {
                        Error($"[错误][{set_name}][{res_relative_path}][{remote_br.BaseStream.Position.ToString("X")}]{i}/{set_count}.指针位置{remote_br.BaseStream.Position.ToString("X")}，应该读取的长度:{file_size.ToString("X")}，实际读取的长度:{fun_i.ToString("X")}，原始数据长度:{remote_br.BaseStream.Length.ToString("X")}，上游指针位置:{br.BaseStream.Position.ToString("X")}，错误信息：{e.Message}");
                        File.WriteAllBytes(Program.TargetDirectiory + "\\error." + Path.GetFileName(file_name), fun_data.ToArray());
                        return;
                    }


                    //if (!String.IsNullOrEmpty(folder_name))








                    byte[] read_data = fun_data.ToArray();

                    var blz4_md5 = CryptUtils.GetMD5HashFromBytes(read_data);



                    byte[] save_data;




                    if (BLZ4FileDecompresser.IsBLZ4(read_data))
                    {
                        BLZ4FileDecompresser blz4 = new BLZ4FileDecompresser(read_data);
                        Info($"[信息][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.是BLZ4文件，解压缩！");
                        save_data = blz4.GetByteResult();
                        blz4.Dispose();
                    }
                    else
                    {
                        Info($"[信息][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.不是BLZ4文件。");
                        save_data = read_data;
                    }

                    fun_data = null; //销毁了


                    //if (ByteHashSet.Contains(save_data))
                    //{
                    //    continue;
                    //}
                    //else
                    //{
                    //    ByteHashSet.Add(save_data);
                    //}


                    //Program.DGML.AddNode(new DGMLWriter.Node(res_relative_path + '\\' + file_name, res_relative_path + '\\' + file_name));
                    //Program.DGML.AddLink(new DGMLWriter.Link(res_relative_path, res_relative_path + '\\' + file_name, set_name, res_relative_path + '.' + set_name));



                    if (IsRES(save_data))
                    {
                        var md5 = CryptUtils.GetMD5HashFromBytes(save_data); //作废
                        if (ResFileManager.IsUnique(file_name, md5))
                        {
                            ResFileManager.Add(file_name, md5);

                            var res_dir = Path.GetDirectoryName(file_name);
                            var res_name = Path.GetFileNameWithoutExtension(file_name);

                            res_name += '_' + md5;

                            var real_res_path = res_dir + "\\" + res_name + ".res";

                            File.WriteAllBytes(real_res_path, save_data);
                            Debug($"[测试][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.是一个独特的res文件，已存储{real_res_path}，文件大小:{save_data.LongLength}，MD5:{md5}");

                            if (fid.ORIGIN_DATA.Count > 2)
                            {

                                File.WriteAllLines(real_res_path + ".gerdp.ini", fid.ORIGIN_DATA);


                            }
                            Task.Run(() => { NextRes(fid.name_1, new FileInfo(real_res_path), IsPS4,folder_name); });

                            return;
                        }
                        else
                        {
                            ResFileManager.Add(file_name, md5);
                            Debug($"[测试][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.是完全重复的res文件{file_name}，MD5:{md5}，不输出，如果看到了这个，说明做这个游戏的人是（小猫骂人），但这个问题被解包的作者兜底了。");
                            return;
                        }

                    }
                    else
                    {

                        lock (ExperimentalDecoder.FileWriterLocker)
                        {

                            var file_dir = Path.GetDirectoryName(file_name);
                            var file_temp_name = Path.GetFileNameWithoutExtension(file_name);
                            var file_temp_ext = Path.GetExtension(file_temp_name);

                            file_name = $"{file_dir}\\{file_temp_name}_{blz4_md5}{file_temp_ext}";

                            if (File.Exists(file_name))
                            {
                                Warning($"[小型提示][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.因为旧的文件是存在的所以没有存储{file_name}，新文件大小:{save_data.LongLength}，如果你看到了这个，说明多线程有一些问题，但这个问题被兜底了。");
                            }
                            else
                            {

                                File.WriteAllBytes(file_name, save_data);
                                Info($"[信息][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.已存储{file_name}，文件大小:{save_data.LongLength}");



                            }


                            if (fid.ORIGIN_DATA.Count > 2)
                            {

                                File.WriteAllLines(file_name + ".gerdp.ini", fid.ORIGIN_DATA);


                            }

                            lock (ExperimentalDecoder.BLZ4MD5Map)
                            {
                                if (!ExperimentalDecoder.BLZ4MD5Map.ContainsKey(blz4_md5))
                                {
                                    Info($"[信息][{set_name}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.添加这个文件到BLZ4的解压记录中。");
                                    ExperimentalDecoder.BLZ4MD5Map.Add(blz4_md5, file_name);
                                }

                            }

                        }

                    }




                    //if (ResFileNameMap[file_name].Equals(md5) && IsRES(save_data))
                    //{
                    //    ResFileNameMap.Add(file_name,md5);
                    //    NextRes(fid.name_1, new FileInfo(file_name), IsPS4);
                    //}







                });
                }catch (Exception e)
                {

                    Error($"[错误][{set_name}][{res_relative_path}][{br.BaseStream.Position.ToString("X")}]{i}/{set_count}.这个RTBL没有办法正确读取：{e.Message}，请注意，这是一个实验级的办法，有错误是正常的。");

                }

            }




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

                long address = IsPS4 ? br.ReadInt64() : br.ReadInt32();

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

        private void NextRes(string title, FileInfo file, bool isPS4,string folder_name)
        {

            Res res = new Res(title, file, isPS4, folder_name);

            //res.SetDecoderSaveFolder(this.folder_name);



            res.DecodeAll();

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
