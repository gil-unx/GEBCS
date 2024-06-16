using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.Utils
{
    public static class FileUtils
    {


        


        public static Dictionary<uint, string> extension_ext = new Dictionary<uint, string>
            {
                { 0x46534e42, ".bnsf" },
                { 0x6c566d47, ".gmvl" },
                { 0x3272742e, ".tr2" },
                { 0x6B737431, ".kst" },
                { 0xffd8ffe1, ".jpg" },
                { 0x52494646, ".riff" },
                { 0x89504E47, ".png" },
                { 0x646F7466, ".dotf" },
                { 0x73657250, ".pres" },
                { 0x347a6c62, ".blz4" },
                { 0x69780300, ".ixo" },
                { 0x2E6C6F62, ".lob" },
            };



        public static string GetExtension(uint magic)
        {

            string extension_str;
            if (!extension_ext.TryGetValue(magic, out extension_str)) { extension_str = ".bin"; }

            return extension_str;
        }

        public static string GetOrderName(int order,string name)
        {

            return order.ToString().PadLeft(8, '0')+'_'+name;

        }

        public static string GetByteArrayString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
                
                if (i < bytes.Length - 1)
                {
                    sb.Append(" ");
                }
            }
            return sb.ToString();

        }

        public static byte[] GetBytesByHexString(string hex_str)
        {
            if (string.IsNullOrEmpty(hex_str))
            {
                return new byte[0];
            }

            var hex_str_arr = hex_str.Split(' ');


            byte[] result = new byte[hex_str_arr.Length];


            for(int i = 0; i < result.Length; i++)
            {


                result[i] = Convert.ToByte(hex_str_arr[i],16);


            }

            return result;

        }


        public static string[] GetHexEditorStyleString(byte[] bytes) { 
        
            List<string> list = new List<string>();


            int offset;


            list.Add($"OFFSET  || 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F || 0123456789ABCDEF");

            int count = 0;

            int byte_count = 0;

            StringBuilder sb = new StringBuilder();


            sb.Append(count.ToString("X8"));
            sb.Append("|| ");

            StringBuilder sb_str = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {

                sb.Append(bytes[i].ToString("X2"));
                byte_count++;
                sb.Append(' ');

                //sb_str.Append((char)bytes[i]);
                //sb_str.Append(GetHexEditoryStyleChar(bytes[i]));

                if(byte_count == 16)
                {

                    count += 0xf + 0x1;

                    sb.Append("|| ");
                    sb.Append(sb_str.ToString());

                    sb_str = new StringBuilder();

                    list.Add(sb.ToString());

                    sb = new StringBuilder(count);

                    sb.Append(count.ToString("X8"));
                    sb.Append("|| ");
                    byte_count = 0;
                }

            }




            return list.ToArray();
        
        
        }





        public static char GetHexEditoryStyleChar(byte b)
        {

            switch ((char)b)
            {
                case '0':
                    return '.';
                case '\r':
                    return '.';
                case '\n':
                    return '.';


                default:
                    return (char)b;
            }


        }


        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }


        public static void CreateSymbolLinkDosCommandFile(Dictionary<string,string> map,string root_dir)
        {

            HashSet<string> list = new HashSet<string>();


            list.Add("@echo off");
            list.Add("cd %~dp0");

            list.Add("rd .\\Symbol-Link /s /q");

            list.Add("mkdir .\\Symbol-Link");

            //List<string> dir_list = new List<string>();

            foreach(var kv in map)
            {
                string origin = ".\\"+Path.GetRelativePath(root_dir,kv.Key);

                if (String.IsNullOrEmpty(kv.Value))
                {
                    continue;
                }

                string target = ".\\Symbol-Link\\" + kv.Value;


                string target_dir = Path.GetDirectoryName(target);
                list.Add($"mkdir {target_dir}");
                //if (!dir_list.Contains(target_dir))
                //{
                //    list.Add($"mkdir {target_dir}");
                //    dir_list.Add(target_dir);
                //}



                list.Add($"echo {origin}={target}");

                list.Add($"mklink \"{target}\" \"{origin}\"");

            }
            list.Add("pause");

            File.WriteAllLines(root_dir+"\\mklink.bat",list);

            
        }

        


    }
}
