using GECV_EX.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GECV_EX.Shared.QpckFile;

namespace GECV_EX.Shared
{
    public class QpckFile : IEnumerable<QpckData>
    {
        public static readonly int QPCK_MAGIC = 0x37402858;


        private int magic;
        private List<QpckData> data;




        public IEnumerator<QpckData> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }





        public QpckFile(FileInfo file) {


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

                        QpckData qdata = new QpckData(i,offset,hash,size,file);

                        this.data.Add(qdata);


                    }
                    
                }
            }


        }

        public QpckFile(DirectoryInfo dir)
        {

            var files = dir.GetFiles("*_*.*",SearchOption.TopDirectoryOnly);

            this.data = new List<QpckData>();
            this.magic = QPCK_MAGIC;
            


            for(int i=0; i < files.Length; i++)
            {
                this.data.Add(new QpckData(files[i]));
            }




        }

        public void SaveAsQpck(string file)
        {

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
                        bw.Write(i.GetData().Key);
                    }



                }
            
            
            }



        }

        public void ExportQpck(string dir)
        {

            Parallel.ForEach<QpckData>(this.data, item => {
                var file_data = item.GetData();

                File.WriteAllBytes(dir+"\\"+file_data.Key, file_data.Value);
            
            });


        }


    }


    public class QpckData
    {
        public int id;
        public long offset;
        public long hash;
        public int size;


        FileInfo file;

        private QpckData()
        {

        }

        public QpckData(int id,long offset,long hash,int size,FileInfo qpck_file) {

            this.id = id;
            this.file = file;
            

        }

        public QpckData(FileInfo qpck_data_file)
        {

            string name = Path.GetFileNameWithoutExtension(qpck_data_file.FullName);

            var str = name.Split('_');

            this.id = Convert.ToInt32(str[0]);

            this.hash = Convert.ToInt64(str[1],16);

            this.size = (int)file.Length;

            this.offset = 0;
            

            this.file = qpck_data_file;

        }


        public KeyValuePair<string, byte[]> GetData()
        {

            byte[] value;
            uint read_magic;
            using(FileStream fs =  new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using(BinaryReader br = new BinaryReader(fs))
                {
                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                    read_magic = br.ReadUInt32();
                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                    value = br.ReadBytes(size);
                }
            }
            


            string key = (id).ToString("D8") + "_" + hash.ToString("X16") + FileUtils.GetExtension(read_magic);




            var kv = new KeyValuePair<string, byte[]>(key,value);

            return kv;


        }


        




    }

}
