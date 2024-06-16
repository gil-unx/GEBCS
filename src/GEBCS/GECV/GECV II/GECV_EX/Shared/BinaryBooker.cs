using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.Shared
{
    internal class BinaryBooker
    {




        private MemoryStream datastream;
        private BinaryWriter datawriter;



        private Dictionary<string, int> bookmark = new Dictionary<string, int>();

        private Dictionary<string, byte[]> bookmark_debug = new Dictionary<string, byte[]>();


        public List<string> GetBookInformation()
        {

            List<string> book = new List<string>();

            foreach(var kv in bookmark)
            {

                if (!bookmark_debug.ContainsKey(kv.Key))
                {
                    book.Add(kv.Key + ",[GECV-NULL],00");
                }
                else {
                    book.Add(kv.Key + "," + kv.Value.ToString("X") + "," + FileUtils.GetByteArrayString(bookmark_debug[kv.Key]));
                }

                



            }

            return book;

        }


        public BinaryBooker()
        {




            datastream = new MemoryStream();
            datawriter = new BinaryWriter(datastream);

            Console.WriteLine($"DataStream;Write:{datastream.CanWrite},Read:{datastream.CanRead},Seek:{datastream.CanSeek}");




        }



        public void SetBookMark(string name, int address)
        {

            if (bookmark.ContainsKey(name))
            {
                throw new InvalidDataException($"BookMark {name} has been alive. Value:{bookmark["name"].ToString("X8")}");
            }

            bookmark[name] = address;

            RefreshStreamLimit(address);

        }

        public bool HasBookMark(string name)
        {
            if (bookmark.ContainsKey(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetBookMark(string name)
        {

            if (bookmark.ContainsKey(name))
            {
                return bookmark[name];
            }
            else
            {
                throw new KeyNotFoundException($"BookMark {name} Not Found!");
            }

        }

        public int GetBookMark(string name,int offset)
        {

            if (!bookmark.ContainsKey(name))
            {
                SetBookMark(name, offset);
            }

            return GetBookMark(name);

        }


        private void WriteBytes(string name, byte[] b)
        {

            //if(b.Length % 16 != 0 )
            //{
            //    throw new ArgumentException($"Bytes Length Error :{b.Length} mod 16 != 0.");
            //}


            if (!datastream.CanWrite)
            {
                throw new IOException($"DataStream Cannot Write Data!");
            }




            datawriter.BaseStream.Seek(GetBookMark(name), SeekOrigin.Begin);
            RefreshStreamLimit(datawriter.BaseStream.Position + b.Length);
            datawriter.Write(b);
            datawriter.Flush();
            Console.WriteLine($"Write {name} At {bookmark[name].ToString("X")}.");

            if (!bookmark_debug.ContainsKey(name))
            {
                bookmark_debug.Add(name,b);
            }
            else
            {
                bookmark_debug[name] = b;
            }


        }


        private void RefreshStreamLimit(long data)
        {
            if (datastream.Length < data)
            {
                Console.WriteLine($"Refresh Data Stream Limit {datastream.Length.ToString("X")} To {data.ToString("X")}");
                datastream.SetLength(data);


            }
        }

        public void WriteData(string name, byte[] data)
        {
            this.WriteBytes(name, data);
        }

        public int WriteData(string name, int data)
        {

            byte[] b = BitConverter.GetBytes(data);

            //Array.Reverse(b);

            this.WriteData(name, b);

            return b.Length;
        }

        public int WriteData(string name,byte data)
        {
            byte[] b = new byte[1];

                b[0] = data;

            //Array.Reverse(b);

            this.WriteData(name, b);

            return b.Length;
        }

        public int WriteData(string name,long data)
        {

            byte[] b = BitConverter.GetBytes(data);

            //Array.Reverse(b);

            this.WriteData(name, b);

            return b.Length;

        }








        public byte[] GetAllData()
        {

            byte[] result;
            datastream.Flush();
            result = datastream.ToArray();

            return result;

        }






    }
}
