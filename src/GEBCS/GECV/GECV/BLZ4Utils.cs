using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using zlib;

namespace GECV
{
    public static class BLZ4Utils
    {






        // partially from http://stackoverflow.com/a/6627194/5343630
        public static void CompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_BEST_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
                Log.Info($"压缩数据：原始：{inData.Length}压缩：{outData.Length}");
            }
        }

        public static void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }


        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[1000000];
            int len;
            while ((len = input.Read(buffer, 0, 1000000)) > 0)
            {
                Log.Info($"复制流：目前读取长度{len}");
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }


        public static List<byte[]> SplitBytes(byte[] input,int size)
        {

            Log.Info($"切割数组，大小为，{input.Length},切割大小为:{size}");
            List<byte[]> list = new List<byte[]>();

            List<byte> slist = new List<byte>();

            int first_size = input.Length % size; //学别人学的，这一步没啥意义，末尾余数组在前后都一样。

            Log.Info($"第一组长度应该是:{first_size}");


            for(int i = 0; i < first_size; i++)
            {
                slist.Add(input[i] );

            }

            list.Add(slist.ToArray());
            Log.Info($"切割第{list.Count}个数组，大小为：{slist.Count}");
            slist.Clear();


            for (int i = first_size; i < input.Length; i++)
            {

                slist.Add(input[i]);

                if(slist.Count == size)
                {
                    list.Add(slist.ToArray());
                    Log.Info($"切割第{list.Count}个数组，大小为：{slist.Count}");
                    slist.Clear();
                }

            }





            return list;
        }






    }
}
