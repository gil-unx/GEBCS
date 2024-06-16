using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GECV.Utils;
using static GECV.Log;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.IO;
using System.Security.Cryptography;
using GECV;

namespace GERDP_RE
{
    internal class Res
    {


        public FileInfo res_file;
        public string title;
        bool isPS4;
        
        

        public List<ResDataSet> DSList;

        private Res() { }

        public Res(string title, FileInfo res_file,bool isPS4, string folder)
        {
            this.title = title;
            this.res_file = res_file;
            this.isPS4 = isPS4;
            Init();
            SetDecoderSaveFolder(folder);
           
        }


        private void RegisterRes() //保险！
        {
            byte[] data = File.ReadAllBytes(res_file.FullName);

            var md5 = CryptUtils.GetMD5HashFromBytes(data);


            ResFileManager.Add(res_file.FullName, md5);

        }
        

        private void SetDecoderSaveFolder(string folder)
        {
            foreach (var i in DSList)
            {
                i.decoder.folder_name = folder + "\\" + this.title;
            }

            if(!Directory.Exists(folder + "\\" + this.title))
            {
                Directory.CreateDirectory(folder + "\\" + this.title);
            }

        }

        private void Init()
        {
            BinaryReader br = GetBinaryReader(GetShareFileStream(this.res_file));

            if (this.isPS4)
            {
                br.BaseStream.Seek(0x30,SeekOrigin.Begin);
            }
            else
            {
                br.BaseStream.Seek(0x20, SeekOrigin.Begin);
            }

            var DS1 = ResDataSet.BuildResDataSet(br,"set_1_res",isPS4);
            var DS2 = ResDataSet.BuildResDataSet(br, "set_2_prx", isPS4);
            var DS3 = ResDataSet.BuildResDataSet(br, "set_3_asset", isPS4);
            var DS4 = ResDataSet.BuildResDataSet(br, "set_4_unk", isPS4);
            var DS5 = ResDataSet.BuildResDataSet(br, "set_5_conf", isPS4);
            var DS6 = ResDataSet.BuildResDataSet(br, "set_6_tbl", isPS4);
            var DS7 = ResDataSet.BuildResDataSet(br, "set_7_text", isPS4);
            var DS8 = ResDataSet.BuildResDataSet(br, "set_8_restbl", isPS4);



            DSList = new List<ResDataSet>();
            DSList.Add(DS1);
            DSList.Add(DS2);
            DSList.Add(DS3);
            DSList.Add(DS4);
            DSList.Add(DS5);
            DSList.Add(DS6);
            DSList.Add(DS7);
            DSList.Add(DS8);

            DS6.decoder.size_mul = 4;


            Parallel.ForEach<ResDataSet>(DSList,i => {
                Info($"注册{title}:{i.name}，[{i.reader_position.ToString("X")}]地址：{i.address.ToString("X")}，数量：{i.count}");
            });




        }

        public void DecodeAll()
        {


            //foreach(var i in DSList)
            //{
            //    i.decoder.Decode(i, this.res_data);
            //}

            byte[] data = File.ReadAllBytes(res_file.FullName);

            var md5 = CryptUtils.GetMD5HashFromBytes(data);

            if (ResFileManager.IsUnique(res_file.FullName, md5))
            {
                ResFileManager.Add(res_file.FullName,md5);
                var run_Result =  Parallel.ForEach<ResDataSet>(DSList, i =>
                {
                    i.decoder.Decode(i, this.res_file);
                });

                while (!run_Result.IsCompleted) ;
                return;


            }
            else
            {
                return;
            }

            



        }




    }
}

