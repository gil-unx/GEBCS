using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.PC
{
    internal class PresSet
    {

        public int offset;

        public PresFileData[] pres_file_set;

        public int mul = 1;


        public bool IsNoUsed = false;


        public PresSet(int offset, int count, int mul)
        {
            this.pres_file_set = new PresFileData[count];
            this.offset = offset;
            this.mul = mul;
        }


        public void GetFileDataFromPres(byte[] pres_data)
        {

            using (MemoryStream ms = new MemoryStream(pres_data))
            {

                using (BinaryReader br = new BinaryReader(ms))
                {


                    br.BaseStream.Seek(this.offset, SeekOrigin.Begin);
                    Console.WriteLine($"Count Pres File Data:{this.pres_file_set.Length},Start Cursor:{br.BaseStream.Position.ToString("X8")}.");


                    for (int i = 0; i < pres_file_set.Length; i++)
                    {

                        PresFileData pres_file;
                        
                        using (MemoryStream file_ms = new MemoryStream(pres_data))
                        {

                            using (BinaryReader file_br = new BinaryReader(file_ms))
                            {
                                
                                file_br.BaseStream.Position = br.BaseStream.Position;

                                Console.WriteLine($"Send New File Data Reader ({file_br.BaseStream.Position.ToString("X8")}.(Count:{i+1}/{this.pres_file_set.Length})");

                                pres_file = new PresFileData(file_br, mul);


                            }


                        }

                        pres_file_set[i] = pres_file;
                        var next = br.BaseStream.Seek(32,SeekOrigin.Current);

                    }






                }

            }




        }



    }
}
