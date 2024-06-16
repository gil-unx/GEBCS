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
    public static class BLZ2Utils
    {
        public static readonly int BLZ2_HEADER = 0x327a6c62;


        public static bool IsBLZ2(byte[] data)
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

                    if (magic != BLZ2_HEADER)
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


        public static byte[] UnpackBLZ2Data(byte[] blz2_file)
        {
            BLZ2FileDecompresser bfd = new BLZ2FileDecompresser(blz2_file);

            byte[] result = bfd.GetByteResult();
            bfd.Dispose();
            return result;


        }

        public static byte[] DoMicrosoftDeflateData(byte[] inData, CompressionMode mode)
        {

            if(CompressionMode.Compress == mode)
            {
                throw new NotSupportedException("Microsoft Deflate Zlib Cannot Create A Readable File For Game.");
                using (MemoryStream outMemoryStream = new MemoryStream(inData))
                {
                    using (MemoryStream resultStream = new MemoryStream())
                    {

                        using (DeflateStream deflateStream = new DeflateStream(resultStream, mode))
                        {



                            outMemoryStream.CopyTo(deflateStream);
                            
                        }

                        
                        return resultStream.ToArray();
                    }
                }
            }
            else
            {

            using (MemoryStream outMemoryStream = new MemoryStream(inData))
            {
                using (MemoryStream resultStream = new MemoryStream())
                {

                    using (DeflateStream deflateStream = new DeflateStream(outMemoryStream, mode))
                    {

                            
                            

                        deflateStream.CopyTo(resultStream);

                    }

                    return resultStream.ToArray();

                }
            }


            }
        }



        //public static byte[] DecompressData(byte[] data)
        //{
            
        //    using(MemoryStream data_ms = new MemoryStream(data))
        //    {
        //        using(MemoryStream result_ms = new MemoryStream())
        //        {

        //            using(InflaterInputStream inputStream = new InflaterInputStream(data_ms))
        //            {
        //                inputStream.CopyTo(result_ms);
        //            }


        //            return result_ms.ToArray();
        //        }
        //    }




        //}

        //public static byte[] CompressData(byte[] data)
        //{

        //    using (MemoryStream data_ms = new MemoryStream(data))
        //    {
        //        using (MemoryStream result_ms = new MemoryStream())
        //        {
        //            Deflater deflater = new Deflater(Deflater.BEST_COMPRESSION);
        //            using (DeflaterOutputStream outputStream = new DeflaterOutputStream(data_ms, deflater))
        //            {
        //                outputStream.CopyTo(result_ms);
        //            }


        //            return result_ms.ToArray();
        //        }
        //    }




        //}






        public static byte[] PackBLZ2Data(byte[] origin_file)
        {

            using (MemoryStream ms = new MemoryStream())
            {

                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    byte[] file_data = origin_file;

                    bw.Write(BLZ2_HEADER);
                    bw.Flush();
                    

                    //bw.Write(Convert.ToInt32(file_data.Length));
                    
                    //bw.Write(0L);
                    //bw.Flush();
                    
                    //System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    //byte[] retVal = md5.ComputeHash(file_data);
                    
                    //bw.Write(retVal);
                    //bw.Flush();

                    if(file_data.Length<= 0xff)
                    {

                        bw.Write(Convert.ToUInt16(file_data.Length));
                        bw.Write(file_data);

                        Console.WriteLine($"BLZ2:Header:{BLZ2_HEADER},Length:{file_data.Length},No Need Compress.");
                    }
                    else
                    {

                    
                    
                    var split_data = BLZ4Utils.SplitBytes(file_data, 0xff);
                    


                    byte[] compress;

                    if (split_data.Count > 1)
                    {
                        compress = DoMicrosoftDeflateData(split_data[split_data.Count - 1],CompressionMode.Compress);
                        

                        bw.Write(Convert.ToUInt16(compress.Length));
                        bw.Write(compress);


                        for (int i = 0; i < split_data.Count - 1; i++)
                        {

                            compress = DoMicrosoftDeflateData(split_data[i],CompressionMode.Compress);
                            //compress = BLZ2Utils.CompressData(split_data[i]);

                            bw.Write(Convert.ToUInt16(compress.Length));
                            bw.Write(compress);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < split_data.Count; i++)
                        {


                                compress = DoMicrosoftDeflateData(split_data[i], CompressionMode.Compress);


                                Console.WriteLine($"Compress Data;{split_data[i].Length} To {compress.Length}. ({i+1}/{split_data.Count})");

                            bw.Write(Convert.ToUInt16(compress.Length));
                            bw.Write(compress);


                        }
                    }
                        Console.WriteLine($"BLZ2:Header:{BLZ2_HEADER},Block Count:{split_data.Count}.");

                    }


                    //long limit = bw.BaseStream.Position;
                    //ms.Seek(0, SeekOrigin.Begin);

                    //byte[] result = new byte[limit];
                    //ms.Read(result, 0, result.Length);

                    

                    byte[] result = ms.ToArray();


                    if (!IsBLZ2(result))
                    {
                        throw new InvalidDataException($"BLZ2 Build Error!");  
                    }

                    //File.WriteAllBytes("E:\\PRES\\TEST\\"+FileUtils.GetByteArrayString(retVal),result);

                    return result;
                }
            }


        }


        protected internal class BLZ2FileDecompresser : IDisposable
        {



            int magic = BLZ2_HEADER; //4
            bool is_single = false;

            List<byte[]> block_list = new List<byte[]>();
            private bool disposedValue;

            public BLZ2FileDecompresser(byte[] input)
            {


                if (input.Length <= 4 + 2)
                {
                    throw new FileLoadException($"DecompressBLZ2:input.length:{input.Length}<=32+2");
                }

                using (MemoryStream input_ms = new MemoryStream(input))
                {

                    using (BinaryReader input_br = new BinaryReader(input_ms))
                    {

                        int magic = input_br.ReadInt32();

                        if (magic != this.magic)
                        {
                            throw new FileLoadException($"DecompressBLZ2:magic:{magic.ToString("X8")}!={BLZ2_HEADER.ToString("X8")}");
                        }



                        while (input_br.BaseStream.Position < input.Length)
                        {
                            int chunk_size = input_br.ReadUInt16();
                            block_list.Add(input_br.ReadBytes(chunk_size));

                        }

                        if(block_list.Count == 1)
                        {
                            this.is_single = true;
                        }else if (block_list.Count <= 0)
                        {
                            throw new FileLoadException($"DecompressBLZ2:data_block.count:{block_list.Count}<=0");
                        }


                    }


                }


            }




            public byte[] GetByteResult()
            {

                if (is_single)
                {
                    byte[] out_single;
                    out_single = DoMicrosoftDeflateData(block_list[0],CompressionMode.Decompress);
                    //out_single = BLZ2Utils.DecompressData(block_list[0]);
                    return out_single;
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
                        out_file = DoMicrosoftDeflateData(i,CompressionMode.Decompress);
                        //out_file = BLZ2Utils.DecompressData(block_list[0]);
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
