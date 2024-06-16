using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RETAEDOG_GUI
{
    public class QpckFile
    {
        public static readonly int QPCK_MAGIC = 0x37402858;


        private int magic;
        private List<QpckData> data;

        private FileInfo origin;


        public QpckFile(FileInfo file) {

            this.origin = file;

            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                    int read_magic = br.ReadInt32();


                    if(QPCK_MAGIC!= read_magic)
                    {
                        throw new FileLoadException($"{file.FullName} is not qpck file!");
                    }


                    int count = br.ReadInt32();

                    data = new List<QpckData>();

                    for(int i = 0; i < count; i++)
                    {

                        long offset = br.ReadInt64();
                        long hash = br.ReadInt64();
                        int size = br.ReadInt32();

                        QpckData qdata = new QpckData(i,offset,hash,size);

                        this.data.Add(qdata);


                    }
                    
                }
            }


        }

        public void UpdateData(int id, byte[] data)
        {





        }


        public void SaveAsQpck(string file)
        {
            using(FileStream origin_fs = new FileStream(origin.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)) { 

            using(FileStream fs = new FileStream(file, FileMode.Create)) {
            
                using(BinaryWriter bw = new BinaryWriter(fs))
                {

                    bw.Write(this.magic);
                    bw.Write(this.data.Count);

                    long file_start = this.data.Count * 20;
                    file_start += 8;

                    foreach(var i in this.data)
                    {

                        bw.Write(file_start);
                        bw.Write(i.hash);
                        bw.Write(i.size);

                        file_start += i.size;

                    }

                    foreach(var i in this.data)
                    {

                            



                    }



                }
            
            
            }

            }


        }



    }


    public class QpckData
    {
        public int id;
        public long offset;
        public long hash;
        public int size;


        public byte[] data;

        private QpckData()
        {

        }

        public QpckData(int id,long offset,long hash,int size) {

            this.id = id;
            this.offset = offset;
            this.hash = hash;
            this.size = size;
           
        }



        




    }

}
