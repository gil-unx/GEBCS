using ComponentAce.Compression.Libs.zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace GECV_EX.Utils
{
    public static class BLZ4Utils //Ionic.Zlib;?
    {
        public static readonly int BLZ4_HEADER = 0x347a6c62;


        public static bool IsBLZ4(byte[] data)
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

                    if (magic != BLZ4_HEADER)
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


        //// partially from http://stackoverflow.com/a/6627194/5343630
        //public static void CompressData(byte[] inData, out byte[] outData)
        //{
        //    using (MemoryStream outMemoryStream = new MemoryStream())
        //    using (ZLibStream outZStream = new ZLibStream(outMemoryStream,CompressionLevel.SmallestSize))
        //    using (Stream inMemoryStream = new MemoryStream(inData))
        //    {
        //        CopyStream(inMemoryStream, outZStream);
        //        outZStream.Dispose();
        //        outData = outMemoryStream.ToArray();

        //    }
        //}

        //public static void DecompressData(byte[] inData, out byte[] outData)
        //{
        //    using (MemoryStream outMemoryStream = new MemoryStream())
        //    using (ZLibStream outZStream = new ZLibStream(outMemoryStream,CompressionLevel.SmallestSize))
        //    using (Stream inMemoryStream = new MemoryStream(inData))
        //    {
        //        CopyStream(inMemoryStream, outZStream);
        //        outZStream.Dispose();
        //        outData = outMemoryStream.ToArray();
        //    }
        //}

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




        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[1000000];
            int len;
            while ((len = input.Read(buffer, 0, 1000000)) > 0)
            {
                
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }


        public static List<byte[]> SplitBytes(byte[] input,int size)
        {

            
            List<byte[]> list = new List<byte[]>();

            List<byte> slist = new List<byte>();

            int first_size = input.Length % size; 

            


            for(int i = 0; i < first_size; i++)
            {
                slist.Add(input[i] );

            }

            list.Add(slist.ToArray());
            
            slist.Clear();


            for (int i = first_size; i < input.Length; i++)
            {

                slist.Add(input[i]);

                if(slist.Count == size)
                {
                    list.Add(slist.ToArray());
                    
                    slist.Clear();
                }

            }





            return list;
        }

        public static byte[] UnpackBLZ4Data(byte[] blz4_file)
        {
            BLZ4FileDecompresser bfd = new BLZ4FileDecompresser(blz4_file);

            byte[] result = bfd.GetByteResult();
            bfd.Dispose();
            return result;


        }





        public static byte[] PackBLZ4Data(byte[] origin_file)
        {

            using (MemoryStream ms = new MemoryStream())
            {

                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    byte[] file_data = origin_file;

                    bw.Write(BLZ4_HEADER);
                    bw.Flush();
                    

                    bw.Write(Convert.ToInt32(file_data.Length));
                    
                    bw.Write(0L);
                    bw.Flush();
                    
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file_data);
                    
                    bw.Write(retVal);
                    bw.Flush();
                    
                    var split_data = BLZ4Utils.SplitBytes(file_data, 32768);
                    


                    byte[] compress;

                    if (split_data.Count > 1)
                    {
                        BLZ4Utils.CompressData(split_data[split_data.Count - 1], out compress);

                        bw.Write(Convert.ToUInt16(compress.Length));
                        bw.Write(compress);


                        for (int i = 0; i < split_data.Count - 1; i++)
                        {

                            BLZ4Utils.CompressData(split_data[i], out compress);

                            bw.Write(Convert.ToUInt16(compress.Length));
                            bw.Write(compress);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < split_data.Count; i++)
                        {


                            BLZ4Utils.CompressData(split_data[i], out compress);

                            Console.WriteLine($"Compress Data;{split_data[i].Length} To {compress.Length}. ({i+1}/{split_data.Count})");

                            bw.Write(Convert.ToUInt16(compress.Length));
                            bw.Write(compress);


                        }
                    }





                    //long limit = bw.BaseStream.Position;
                    //ms.Seek(0, SeekOrigin.Begin);

                    //byte[] result = new byte[limit];
                    //ms.Read(result, 0, result.Length);

                    Console.WriteLine($"BLZ4:Header:{BLZ4_HEADER},Original Length:{file_data.Length},MD5:{FileUtils.GetByteArrayString(retVal)},Block Count:{split_data.Count}.");

                    byte[] result = ms.ToArray();


                    if (!IsBLZ4(result))
                    {
                        throw new InvalidDataException($"BLZ4 Build Error!");  
                    }

                    //File.WriteAllBytes("E:\\PRES\\TEST\\"+FileUtils.GetByteArrayString(retVal),result);

                    return result;
                }
            }

        }


        protected internal class BLZ4FileDecompresser : IDisposable
        {



            int magic = BLZ4Utils.BLZ4_HEADER; //4
            int unpacked_size; //8
            long zerozero; //16
            byte[] md5; //32
            bool is_uncompressd = false;

            List<byte[]> block_list = new List<byte[]>();
            private bool disposedValue;

            public BLZ4FileDecompresser(byte[] input)
            {


                if (input.Length <= 32 + 2)
                {
                    throw new FileLoadException($"DecompressBLZ4:input.length:{input.Length}<=32+2");
                }

                using (MemoryStream input_ms = new MemoryStream(input))
                {

                    using (BinaryReader input_br = new BinaryReader(input_ms))
                    {

                        int magic = input_br.ReadInt32();

                        if (magic != this.magic)
                        {
                            throw new FileLoadException($"DecompressBLZ4:magic:{magic.ToString("X8")}!={BLZ4Utils.BLZ4_HEADER.ToString("X8")}");
                        }

                        unpacked_size = input_br.ReadInt32();
                        zerozero = input_br.ReadInt64();
                        md5 = input_br.ReadBytes(16);


                        while (input_br.BaseStream.Position < input.Length)
                        {
                            int chunk_size = input_br.ReadUInt16();
                            if (chunk_size == 0)
                            {

                                block_list.Add(input_br.ReadBytes(Convert.ToInt32(input.LongLength - input_br.BaseStream.Position)));
                                is_uncompressd = true;

                            }
                            else
                            {



                                block_list.Add(input_br.ReadBytes(chunk_size));



                            }

                        }


                        if (block_list.Count <= 0)
                        {
                            throw new FileLoadException($"DecompressBLZ4:data_block.count:{block_list.Count}<=0");
                        }


                    }


                }


            }



            public byte[] GetByteResult()
            {

                if (is_uncompressd)
                {
                    return block_list[0];
                }
                else
                {
                    LinkedList<byte[]> real_list = new LinkedList<byte[]>();


                    for (int i = 1; i < block_list.Count; i++)
                    {
                        real_list.AddLast(block_list[i]);
                    }
                    real_list.AddLast(block_list[0]);

                    LinkedList<byte[]> real_block_list = new LinkedList<byte[]>();

                    foreach (var i in real_list)
                    {

                        byte[] out_file;
                        BLZ4Utils.DecompressData(i, out out_file);
                        real_block_list.AddLast(out_file);

                    }

                    LinkedList<byte> result_list = new LinkedList<byte>();

                    foreach (var i in real_block_list)
                    {
                        foreach (var si in i)
                        {
                            result_list.AddLast(si);
                        }
                    }

                    return result_list.ToArray();
                }

            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {

                    }


                    disposedValue = true;
                }
            }

            public void Dispose()
            {

                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

    }




}
