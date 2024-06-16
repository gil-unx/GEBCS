using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GECV_EX.Shared
{

    public class GnfFile
    {

        public static readonly int DDS_MAGIC = 0x20534444;

        public List<byte[]> dds_data;


        public string filename;




        public GnfFile(byte[] data) 
        {


            
            this.filename = CryptUtils.GetMD5HashFromBytes(data);

            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryReader br = new BinaryReader(ms))
                {

                    int count = br.ReadInt32();

                    int[] file_size_group = new int[count];

                    for(int i = 0; i < count; i++)
                    {

                        file_size_group[i] = br.ReadInt32();


                    }

                    dds_data = new List<byte[]>();

                    for(int i = 0;i < file_size_group.Length; i++)
                    {

                        var data_file = br.ReadBytes(file_size_group[i]);


                        using (MemoryStream file_ms = new MemoryStream(data_file))
                        {
                            using (BinaryReader reader = new BinaryReader(file_ms))
                            {
                                int dds_magic = reader.ReadInt32();

                                if (dds_magic != DDS_MAGIC)
                                {
                                    throw new FileLoadException($"Gnf Error:{i} is not dds file.");
                                }
                                else
                                {
                                    dds_data[i] = data_file;
                                }

                            }
                        }

                    }



                }
            }



        }


        public void WriteDDS(string path)
        {

            for(int i=0;i<this.dds_data.Count;i++)
            {
                File.WriteAllBytes(path+"\\"+this.filename+"_"+i+".dds", dds_data[i]);
            }


        }

        public void WriteGNF(string filename)
        {

            using(FileStream fs = new FileStream(filename,FileMode.Create,FileAccess.Write,FileShare.None))
            {
                using(BinaryWriter bw = new BinaryWriter(fs))
                {

                    bw.Write(this.dds_data.Count);
                    
                    foreach(byte[] b in this.dds_data)
                    {
                        bw.Write(b.Length);
                    }

                    foreach (byte[] b in this.dds_data)
                    {
                        bw.Write(b);
                    }


                }
            }



        }



        

    }
}
