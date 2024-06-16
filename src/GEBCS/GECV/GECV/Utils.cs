using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;


namespace GECV
{
    public static class Utils
    {

        public static bool IsVaildRegex(string str,string regex)
        {

            try { 
            
            Regex reg = new Regex(regex);
            Match result = reg.Match(str);
                return result.Success;
            }
            catch(Exception e) {

                Log.Error($"{str}无法被{regex}处理！");

            }
            return false;

        }
        public static FileStream GetShareFileStream(String path)
        {
            
            return new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
        }

        public static FileStream GetShareFileStream(FileInfo file)
        {
            return GetShareFileStream(file.FullName);
        }

        public static BinaryReader GetBinaryReader(FileStream fs)
        {
            return new BinaryReader(fs);
        }

        public static BinaryReader GetBinaryReader(byte[] input_data)
        {
            MemoryStream stream = new MemoryStream(input_data, false);
            return new BinaryReader(stream);
        }

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

        public static string PrintByteArray(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
                sb2.Append(bytes[i].ToString("X2"));
                if (i < bytes.Length - 1)
                {
                    sb.Append(" ");
                }
            }
            Log.Info(sb.ToString());
            return sb2.ToString();

        }

        public static string GetListStringPrintData<T>(List<T> list)
        {

            StringBuilder sb = new StringBuilder();
            int number = 0;
            foreach(var i in list)
            {
                sb.Append(number++);
                sb.Append(":");
                sb.Append(i.ToString());
                sb.Append('\n');
            }

            return sb.ToString();

        }

        public static byte[] GetStringBytes(string str)
        {

            byte[] bytes = new byte[str.Length];


            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }
            PrintByteArray(bytes);
            return bytes;

        }

        public static void WriteListToFile(List<string> list, string path)
        {

            if (File.Exists(path))
            {
                File.Delete(path);
            }


            File.WriteAllLines(path, list.ToArray());


        }

        public static void ReadMagicAndSaveByType(byte[] file,string file_name, string dir)
        {

            using (MemoryStream memoryStream = new MemoryStream(file))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {

                    var byte_magic = reader.ReadBytes(4);
                    Array.Reverse(byte_magic);
                    var file_magic = BitConverter.ToUInt32(byte_magic,0);
                    Log.Info($"{file_name}这个文件的魔法码是：{file_magic.ToString("X8")}");
                    var path = Path.GetDirectoryName(dir  + ".bin\\");


                    if (Define.extension_ext.ContainsKey(file_magic))
                    {
                        Log.Info($"找到了：{dir +  Define.extension_ext[file_magic]}");
                        path = Path.GetDirectoryName(dir + Define.extension_ext[file_magic]+"\\");


                    }
                    else
                    {
                        Log.Info($"没找到。");
                    }
                    Log.Info($"最后的分类：{path}");

                    Directory.CreateDirectory(path);

                    

                    File.WriteAllBytes(path + "\\" + file_name, file);

                }
            }


        }


    }
}
