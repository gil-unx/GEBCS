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

namespace GERDP
{
    internal class Res
    {


        byte[] res_data;
        public string title;
        bool isPS4;
        long level;
        ResDataSet DS1, DS2, DS3, DS4, DS5, DS6, DS7, DS8;

        public string extend_title;

        public List<ResDataSet> DSList;

        private Res() { }

        public Res(string title, byte[] data,bool isPS4)
        {
            this.title = title;
            this.res_data = data;
            this.isPS4 = isPS4;
            Init();

        }
        public Res(string title, byte[] data, bool isPS4,long level)
        {
            this.title = title;
            this.res_data = data;
            this.isPS4 = isPS4;
            this.level = level;
            Init();

        }

        public void SetDecoderSaveFolder(string folder)
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
            BinaryReader br = GetBinaryReader(this.res_data);

            if (this.isPS4)
            {
                br.BaseStream.Seek(0x30,SeekOrigin.Begin);
            }
            else
            {
                br.BaseStream.Seek(0x20, SeekOrigin.Begin);
            }

            DS1 = ResDataSet.BuildResDataSet(br,"set_1_res",isPS4);
            DS2 = ResDataSet.BuildResDataSet(br, "set_2_prx", isPS4);
            DS3 = ResDataSet.BuildResDataSet(br, "set_3_asset", isPS4);
            DS4 = ResDataSet.BuildResDataSet(br, "set_4_unk", isPS4);
            DS5 = ResDataSet.BuildResDataSet(br, "set_5_conf", isPS4);
            DS6 = ResDataSet.BuildResDataSet(br, "set_6_tbl", isPS4);
            DS7 = ResDataSet.BuildResDataSet(br, "set_7_text", isPS4);
            DS8 = ResDataSet.BuildResDataSet(br, "set_8_restbl", isPS4);



            DSList = new List<ResDataSet>();
            DSList.Add(DS1);
            DSList.Add(DS2);
            DSList.Add(DS3);
            DSList.Add(DS4);
            DSList.Add(DS5);
            DSList.Add(DS6);
            DSList.Add(DS7);
            DSList.Add(DS8);

            DS7.decoder.size_mul = 16;


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

            Parallel.ForEach<ResDataSet>(DSList, i =>
            {
                i.decoder.Decode(i, this.res_data);
            });


        }




    }
}

