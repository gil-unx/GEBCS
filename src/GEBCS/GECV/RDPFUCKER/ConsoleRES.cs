using RDPFUCKER;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static GECV.Log;
using static RDPFUCKER.Programs_Extend;

namespace GECV_Extend
{
    internal class ConsoleRES
    {


        public struct ResDataSet
        {
            public long address;
            public long count;



        }

        public struct FileInformationData
        {
            public string name_1;
            public string ext_2;
            public string unk_3;
            public string function_4;
            public string function_args_5;
            public long ORIGIN_COUNT; //原始数据的数量，下面是原始数据。
            public List<string> ORIGIN_DATA;
        }


        public enum DataLocation
        {
            NoUsed_3,Package_4,Data_5,Patch_6, Current_C,UNK
        }


        ResDataSet set_1_unk, set_2_res, set_3_prx, set_4_asset, set_5_unk, set_6_conf, set_7_tbl, set_8_text;
        byte[] input_data;

        bool isPS4;
        string title;

        public ConsoleRES(byte[] input_data,bool isPS4,string title)
        {
            this.isPS4 = isPS4;
            this.title = title;
            if (this.isPS4)
            {
                DecodePS4(input_data);
                
            }
            else
            {
                DecodePSV(input_data);
            }
        }



         void DecodePS4(byte[] input_data)
        {
            BinaryReader br = GetBinaryReader(input_data);
            br.BaseStream.Seek(0x30, SeekOrigin.Begin);

            set_1_unk =  BuildResDataSet(br.ReadInt64(),br.ReadInt64());
            set_2_res = BuildResDataSet(br.ReadInt64(), br.ReadInt64());
            set_3_prx = BuildResDataSet(br.ReadInt64(), br.ReadInt64());
            set_4_asset = BuildResDataSet(br.ReadInt64(), br.ReadInt64());
            set_5_unk = BuildResDataSet(br.ReadInt64(), br.ReadInt64());
            set_6_conf = BuildResDataSet(br.ReadInt64(), br.ReadInt64());
            set_7_tbl = BuildResDataSet(br.ReadInt64(), br.ReadInt64());
            set_8_text = BuildResDataSet(br.ReadInt64(), br.ReadInt64());
            this.input_data = input_data;

        }

        public void FullDecode()
        {
            Info($"开始解码……");
            Info($"数据区内容：\nset_1_unk:{set_1_unk.address}|{set_1_unk.count}\nset_2_res:{set_2_res.address}|{set_2_res.count}\nset_3_prx:{set_3_prx.address}|{set_3_prx.count}\nset_4_asset:{set_4_asset.address}|{set_4_asset.count}\nset_5_unk:{set_5_unk.address}|{set_5_unk.count}\nset_6_conf:{set_6_conf.address}|{set_6_conf.count}\nset_7_tbl:{set_7_tbl.address}|{set_7_tbl.count}\nset_8_text:{set_8_text.address}|{set_8_text.count}");
            Parallel.Invoke(() => { Decode(title+"_set_1_unk", set_1_unk, 0); }, () => { Decode(title + "_set_2_res", set_2_res, 0); }, () => { Decode(title + "_set_3_prx", set_3_prx, 0); }, () => { Decode(title+"_set_4_asset", set_4_asset, 0); }, () => { Decode(title+"_set_5_unk", set_5_unk, 0); }, () => { Decode(title+"_set_6_conf", set_6_conf, 0); }, () => { Decode(title + "_set_7_tbl", set_7_tbl, 0); }, () => { Decode(title + "_set_8_text", set_8_text, 0); });
        }

        

        void Decode(string tag, ResDataSet set,long level)
        {

            if(set.address == 0 || set.count == 0)
            {
                Info($"[{tag}][{level}]这一部分是空的，不处理。");
                return;
            }

            BinaryReader br = GetBinaryReader(this.input_data);

            br.BaseStream.Seek(set.address, SeekOrigin.Begin);
            Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]当前数据位置:{br.BaseStream.Position.ToString("X")}");


            Parallel.For(0, set.count,i => {
                long virtual_address =  this.isPS4?br.ReadInt64():br.ReadInt32();
                Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.虚地址分析:{(this.isPS4?virtual_address.ToString("X16"): virtual_address.ToString("X8"))}");

                if(virtual_address == 0)
                {
                    Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.【意外错误】虚地址:{(this.isPS4 ? virtual_address.ToString("X16") : virtual_address.ToString("X8"))}是非法地址，不再处理这个项目。");
                    return;
                }

                DataLocation location = GetLocation(virtual_address);


                if(location == DataLocation.UNK)
                {
                    Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.【警告】虚地址:{(this.isPS4 ? virtual_address.ToString("X16") : virtual_address.ToString("X8"))}的头是无法处理的内容:{GetLocationInformation(location)}，尝试获取原始数据。");
                    return;
                }

                long real_address = GetRealAddress(virtual_address);

                Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.数据地址：{real_address.ToString("X")}|{GetLocationInformation(location)}");

                byte[] data_library;
                switch (location)
                {
                    case DataLocation.NoUsed_3:
                        data_library = null;
                        break;
                    case DataLocation.Package_4:
                        data_library = Programs_Extend.Package.LongLength <= 0 ? null : Programs_Extend.Package;
                        break;
                    case DataLocation.Data_5:
                        data_library = Programs_Extend.Data.LongLength <= 0 ? null : Programs_Extend.Data;
                        break;
                    case DataLocation.Patch_6:
                        data_library = Programs_Extend.Patch.LongLength <=0 ? null : Programs_Extend.Patch;
                        break;
                    case DataLocation.Current_C:
                        data_library = this.input_data.LongLength <= 0 ? null : this.input_data;
                        break;
                    default:
                        throw new InvalidDataException($"[意外错误][{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.{virtual_address.ToString("X16")}的头不在范围内（3-C）。");

                }

                if (data_library == null)
                {
                    Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.【意外错误】索引的文件不存在！{(this.isPS4? virtual_address.ToString("X16"): virtual_address.ToString("X8"))}，但这是一个合法的头:{GetLocationInformation(location)}这是一个意外的内容？");
                    return;
                }

                BinaryReader data_br = GetBinaryReader(data_library);

                long file_size = this.isPS4?br.ReadInt64() : br.ReadInt32();

                long file_info_address = this.isPS4 ? br.ReadInt64() : br.ReadInt32();

                long file_info_count = this.isPS4? br.ReadInt64() : br.ReadInt32();

                long file_size_uncompress = this.isPS4?br.ReadInt64() : br.ReadInt32();


                Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.文件大小：{file_size}\n文件信息地址：{file_info_address.ToString("X16")};文件信息数量：{file_info_count}：文件解压大小：{file_size_uncompress}");

                

                Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.数据地址：{real_address}|{GetLocationInformation(location)},准备读取文件信息数：{file_info_count}。");

                br.BaseStream.Position = file_info_address;

                

                //FileInformationData fid = GetOriginData(data_br,file_info_count);
                
                //if (fid.ORIGIN_COUNT <3 || fid.ORIGIN_COUNT >5)
                //{

                //    Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.【警告】这个文件的数据好像不对？读取数量：{fid.ORIGIN_COUNT}。");


                //}
                //Info($"[{tag}][{br.BaseStream.Position.ToString("X")}][{level}]{i}/{set.count}.读取到的内容：\n{GECV.Utils.GetListStringPrintData(fid.ORIGIN_DATA)}");



            });





        }

        

        public FileInformationData GetOriginData(BinaryReader br,long count)
        {
            FileInformationData fid = new FileInformationData();

            fid.ORIGIN_COUNT = count;
            fid.ORIGIN_DATA = new List<string>();


            for (int i = 0; i < fid.ORIGIN_COUNT; i++)
            {
                
                long address = this.isPS4?br.ReadInt64():br.ReadInt32();

                br.BaseStream.Position = address;

                string data = GECV.Utils.readNullterminated(br);

                fid.ORIGIN_DATA.Add(data);

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


        public DataLocation GetLocation(long address)
        {
            string virtual_address = this.isPS4? address.ToString("X16"):Convert.ToInt32(address).ToString("X8");

            if (virtual_address.Length > 8 && !this.isPS4)
            {
                Info($"处理微软的问题之前:{virtual_address}");
                virtual_address = virtual_address.Substring(8);
                Info($"处理微软的问题之后:{virtual_address}");
            }

            switch (virtual_address[0])
            {
                case '3':
                    return DataLocation.NoUsed_3;
                case '4':
                    return DataLocation.Package_4;
                case '5':
                    return DataLocation.Data_5;
                case '6':
                    return DataLocation.Patch_6;
                case 'C':
                    return DataLocation.Current_C;
                default:
                    return DataLocation.UNK;
                    //throw new InvalidDataException($"[意外错误]{virtual_address}的头不在范围内（3-C）。");
                    
            }

            
            

        }

        public static string GetLocationInformation(DataLocation dl)
        {
            string inf;
            switch (dl)
            {
                case DataLocation.NoUsed_3:
                    inf = "没有用（3）";
                    break;
                case DataLocation.Package_4:
                    inf = "Package（4）";
                    break;
                case DataLocation.Data_5:
                    inf = "Data（5）";
                    break;
                case DataLocation.Current_C:
                    inf = "当前res（C）";
                    break;
                default:
                    inf = "错误选项（？）";
                    break;

            }

            return inf;

        }

        public long GetRealAddress(long address)//十六进制没想明白怎么<<，还是土方法做吧。
        {
            string virtual_address = this.isPS4?address.ToString("X16"):Convert.ToInt32(address).ToString("X8");

            if (virtual_address.Length > 8 && !this.isPS4)
            {
                Info($"处理微软的问题之前:{virtual_address}");
                virtual_address = virtual_address.Substring(8);
                Info($"处理微软的问题之后:{virtual_address}");
            }

            return Convert.ToInt64(virtual_address.Substring(1),16);
        }


        public ResDataSet BuildResDataSet(long address,long count)
        {
            ResDataSet res = new ResDataSet();
            res.address = address;
            res.count = count;
            return res;
        }


        void DecodePSV(byte[] input_data)
        {
            BinaryReader br = GetBinaryReader(input_data);
            br.BaseStream.Seek(0x20, SeekOrigin.Begin);

            set_1_unk = BuildResDataSet(br.ReadInt32(), br.ReadInt32());
            set_2_res = BuildResDataSet(br.ReadInt32(), br.ReadInt32());
            set_3_prx = BuildResDataSet(br.ReadInt32(), br.ReadInt32());
            set_4_asset = BuildResDataSet(br.ReadInt32(), br.ReadInt32());
            set_5_unk = BuildResDataSet(br.ReadInt32(), br.ReadInt32());
            set_6_conf = BuildResDataSet(br.ReadInt32(), br.ReadInt32());
            set_7_tbl = BuildResDataSet(br.ReadInt32(), br.ReadInt32());
            set_8_text = BuildResDataSet(br.ReadInt32(), br.ReadInt32());
            this.input_data = input_data;
        }


        static BinaryReader GetBinaryReader(byte[] input_data)
        {
            MemoryStream stream = new MemoryStream(input_data,false);
            return new BinaryReader(stream);
        }


    }
}

