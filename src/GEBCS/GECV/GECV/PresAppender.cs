using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace GECV
{
    public class PresAppender
    {


        struct NewFile
        {
            //public int pres_address; //pres文件偏移的头地址
            //public int new_file_offset; 
            //public int new_file_pres_path; // pres里存的地址
            //public int new_file_csize;
            //public int new_file_csize_16; //实际要写进去的文件大小
            //public int new_file_usize;


            public int new_file_offset;
            public byte[] new_file_bytes;

        }


        private byte[] pres;

        private int NewFileOffsetCursor = 0; //每加进来一个文件就把这个大小算进去

        private Dictionary<string, NewFile> NewFileMap = new Dictionary<string, NewFile>();
        private List<string> NewFileList = new List<string>();
        
        

        private int GetLastPresOffset()
        {
            return pres.Length;
        }

        private int CalcOffset(int origin)
        {
            int result = 0;

            string str = origin.ToString("X8");

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('F');
            stringBuilder.Append(str.Substring(1));

            Log.Info($"转码{result},（{stringBuilder.ToString()}）,来自:{origin}（{str}）");
            result = Convert.ToInt32(stringBuilder.ToString(), 16); 
            return result;
        }


        public PresAppender(byte[] pres_file)
        {
            this.pres = pres_file;
        }

        public void RemoveLastFileAndAppendNewFile(DataRow dr)
        {

            Log.Info($"YS启动！删除末尾文件，拼合新文件。");

            string set_data_3_file_offset_real = dr["set_data_3_file_offset_real"].ToString();
            Log.Info($"原始文件在{set_data_3_file_offset_real}，真实偏移应该是：{set_data_3_file_offset_real.Substring(1)}。");

            int header_length = Convert.ToInt32(set_data_3_file_offset_real.Substring(1), 16);

            Log.Info($"头长度：{header_length}");

            byte[] new_pres_header = new byte[header_length];

            for(int i = 0; i < new_pres_header.Length; i++)
            {
                new_pres_header[i] = this.pres[i];
            }

            this.pres = new_pres_header;

            Log.Info($"文件去尾完成！目前头长度：{new_pres_header.Length},/16:{new_pres_header.Length/16},%16:{new_pres_header.Length%16}");

            AppendNewFile(dr);
        }

        public void AppendPres7Data(DataRow dr)
        {
            NewFile new_file;


            string data_new = dr["set_data_7_data_new"].ToString();

            byte[] data_new_bytes = Encoding.UTF8.GetBytes(data_new);

            Log.Info($"读取数据{dr["set_data_7_data_new"].ToString()},大小:{data_new_bytes.Length}");

            int origin_size = data_new_bytes.Length;
            int array_size = data_new_bytes.Length;

            if (array_size % 16 != 0)
            {
                array_size = array_size / 16;
                array_size += 1;
                array_size *= 16;
            }
            else
            {
                array_size = array_size / 16;
                array_size *= 16; //我这写的什么垃圾代码，我看了我都想吐，真累啊，有没有大佬给弟弟做做汉化啊，我不想过年的时候都在做这个东西啊！ 不想动脑子了就这样吧毁灭吧！！！
            }



            Array.Resize(ref data_new_bytes, array_size);

            Log.Info($"取16倍整数，新数组大小为：{data_new_bytes.Length}");

            new_file = new NewFile();
            new_file.new_file_bytes = data_new_bytes;
            new_file.new_file_offset = GetLastPresOffset() + NewFileOffsetCursor;
            NewFileOffsetCursor += new_file.new_file_bytes.Length;
            Log.Info($"这个文件应该在{new_file.new_file_offset},指针推进：{NewFileOffsetCursor}");


            int write_offset = CalcOffset(new_file.new_file_offset);

            WritePres7Cursor(Convert.ToInt32(dr["set_data_7_offset"]),write_offset,origin_size);


        }

        public void AppendNewFile(DataRow dr)
        {
            NewFile new_file;




            string map_path = dr["set_data_3_complete"].ToString();

            if (NewFileMap.ContainsKey(map_path))
            {
                new_file = NewFileMap[map_path];
            }
            else
            {

                byte[] new_file_bytes = File.ReadAllBytes(dr["n_file_path"].ToString());

                Log.Info($"读取文件{dr["n_file_path"].ToString()},大小:{new_file_bytes.Length}");

                Array.Resize(ref new_file_bytes, Convert.ToInt32(dr["n_file_csize_16"]));

                Log.Info($"取16倍整数，新数组大小为：{new_file_bytes.Length}");

                new_file = new NewFile();
                new_file.new_file_bytes = new_file_bytes;
                new_file.new_file_offset = GetLastPresOffset() + NewFileOffsetCursor;
                NewFileOffsetCursor += new_file.new_file_bytes.Length;
                Log.Info($"这个文件应该在{new_file.new_file_offset},指针推进：{NewFileOffsetCursor}");


                
                NewFileMap.Add(map_path, new_file);
                NewFileList.Add(map_path);
                Log.Info($"登记文件：{dr["set_data_3_complete"].ToString()}，大小为：{new_file_bytes.Length}，在位置:{NewFileList.Count}");

            }

            int write_offset = CalcOffset(new_file.new_file_offset);

            WritePresCursor(Convert.ToInt32(dr["set_data_3_file_address"]),write_offset, Convert.ToInt32(dr["n_file_csize"]), Convert.ToInt32(dr["n_file_usize"]));
            

        }

        public void SaveFile(string path)
        {

            FileInfo file = new FileInfo(path); 

            if(file.Exists)
            {
                file.Delete();
            }

            using(BinaryWriter bw = new BinaryWriter(file.OpenWrite()))
            {

                bw.Write(pres);
                Log.Info($"写入{pres.Length}大小作为原始Pres，");


                foreach(var i in NewFileList)
                {


                    NewFile new_file = NewFileMap[i];

                    Log.Info($"理论{i}的位置在{new_file.new_file_offset}，写入大小为:{new_file.new_file_bytes.Length}，实际上在:{bw.BaseStream.Position}");

                    bw.Write(new_file.new_file_bytes);

                }




                bw.Flush();
            }

            

            Log.Info($"完成，文件存放于：{file.FullName}");

        }



        private void WritePresCursor(int address,int offset,int csize,int usize)
        {

            //int offset_file = br.ReadInt32();
            //int csize_file = br.ReadInt32();
            //int name_off_file = br.ReadInt32();
            //int name_elements_file = br.ReadInt32();
            //int file_unk1 = br.ReadInt32();
            //int file_unk2 = br.ReadInt32();
            //int file_unk3 = br.ReadInt32();
            //int usize_file = br.ReadInt32();

            using (MemoryStream ms = new MemoryStream(pres))
            {
                using(BinaryWriter bw = new BinaryWriter(ms))
                {

                    bw.Seek(address, SeekOrigin.Begin);
                    Log.Info($"定位地址在:{bw.BaseStream.Position}，写入offset:{offset}");
                    bw.Write(offset);
                    Log.Info($"定位地址在:{bw.BaseStream.Position}，写入csize:{csize}");
                    bw.Write(csize);
                    bw.Seek(4, SeekOrigin.Current); //name
                    bw.Seek(4, SeekOrigin.Current); //name_e
                    bw.Seek(4, SeekOrigin.Current); //unk1
                    bw.Seek(4, SeekOrigin.Current); //unk2
                    bw.Seek(4, SeekOrigin.Current); //unk3
                    Log.Info($"跳过20，定位地址在:{bw.BaseStream.Position}，写入usize:{usize}");
                    bw.Write(usize);

                }
            }

            
        }

        private void WritePres7Cursor(int address,int offset,int length)
        {
            using (MemoryStream ms = new MemoryStream(pres))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    bw.Seek(address, SeekOrigin.Begin);
                    Log.Info($"定位地址在:{bw.BaseStream.Position}，写入offset:{offset}");
                    bw.Write(offset);
                    Log.Info($"定位地址在:{bw.BaseStream.Position}，length:{length}");
                    bw.Write(length);

                }
            }
        }

    }



}
