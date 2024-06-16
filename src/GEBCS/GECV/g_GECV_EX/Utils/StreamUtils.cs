using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.Utils
{
    public static class StreamUtils
    {


        public static string readNullterminated(BinaryReader reader)
        {
            var char_array = new List<byte>();
            string str = "";
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                str = Encoding.UTF8.GetString(char_bytes2);
                return str;
            }
            byte b = reader.ReadByte();
            while ((b != 0x00) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                char_array.Add(b);
                b = reader.ReadByte();
            }
            byte[] char_bytes = char_array.ToArray();
            str = Encoding.UTF8.GetString(char_bytes);
            return str;
        }

        public static byte[] readZeroterminated(BinaryReader reader)
        {
            var char_array = new List<byte>();
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                return char_bytes2;
            }
            byte b = reader.ReadByte();
            while ((b != 0x00) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                char_array.Add(b);
                b = reader.ReadByte();
            }
            byte[] char_bytes = char_array.ToArray();
            
            return char_bytes;
        }

        public static byte[] readWideDataterminated(BinaryReader reader)
        {
            var char_array = new List<byte>();
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                return char_bytes2;
            }
            byte b = reader.ReadByte();
            byte b2 = reader.ReadByte();
            while ((b != 0x00 || b2!= 0x00) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                char_array.Add(b);
                char_array.Add(b2);
                b = reader.ReadByte();
                b2 = reader.ReadByte();
            }
            byte[] char_bytes = char_array.ToArray();

            return char_bytes;



        }



    }
}
