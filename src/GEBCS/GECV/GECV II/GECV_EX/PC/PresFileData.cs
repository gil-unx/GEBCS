using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GECV_EX.PC
{
    [Serializable]
    [XmlRoot]
    public class PresFileData
    {

        [XmlIgnore]
        public int original_offset_file;
        [XmlIgnore]
        public int real_offset_file;
        [XmlIgnore]
        public int csize_file;
        [XmlIgnore]
        public int name_off_file;
        [XmlIgnore]
        public int name_elements_file;
        [XmlAttribute]
        public int file_unk1;
        [XmlAttribute]
        public int file_unk2;
        [XmlAttribute]
        public int file_unk3;
        [XmlIgnore]
        public int usize_file; //uncompressd

        [XmlArray]
        public string[] name_list;

        [XmlIgnore]
        public byte[] file_data;

        [XmlAttribute]
        public string VirtualFileName;

        [XmlAttribute]
        public bool IsVirtualFile;
        [XmlAttribute]
        public bool IsCompressed;

        [XmlAttribute]
        public bool IsBlankFile;

        [XmlAttribute]
        public int file_size_mul = 1;

        [XmlAttribute]
        public string OriginalOffsetHex;
        [XmlAttribute]
        public string OriginalCSizeHex;

        

        private PresFileData()
        {

        }


        public PresFileData(BinaryReader br,int mul)
        {
            long debug_cursor = br.BaseStream.Position;

            this.file_size_mul = mul;


            original_offset_file = br.ReadInt32();
            csize_file = br.ReadInt32();
            name_off_file = br.ReadInt32();
            name_elements_file = br.ReadInt32();
            file_unk1 = br.ReadInt32();
            file_unk2 = br.ReadInt32();
            file_unk3 = br.ReadInt32();
            usize_file = br.ReadInt32();

            csize_file *= file_size_mul;

            Console.WriteLine($@"File Data ({debug_cursor.ToString("X8")}):
            File Offset:{original_offset_file.ToString("X8")}
            File Output Size:{csize_file}
            File Config Offset:{name_off_file.ToString("X8")}
            File Config Count:{name_elements_file}
            File Data A:{file_unk1.ToString("X8")}
            File Data B:{file_unk2.ToString("X8")}
            File Data C:{file_unk3.ToString("X8")}
            File Uncompress(BLZ4) Size:{usize_file}
            ");

            //if(original_offset_file == 0)
            //{
            //    Console.WriteLine("This Is Blank File,Pass.");

            //    this.file_data = new byte[0];

            //    return;
            //}

            //if (csize_file == usize_file)
            //{
            //    this.IsCompressed = false;
            //}
            //else
            //{
            //    this.IsCompressed = true;
            //}

            br.BaseStream.Seek(name_off_file, SeekOrigin.Begin);

            int[] name_element_address = new int[name_elements_file];
            name_list = new string[name_elements_file];

            if (name_elements_file > 7 || name_elements_file < 0)
            {
                throw new DataException($"Name Element Over Limit! {name_elements_file},At:{name_off_file}.(Debug:Start Position:{debug_cursor.ToString("X8")})");
            }



            for (int i = 0; i < name_element_address.Length; i++)
            {

                name_element_address[i] = br.ReadInt32();
                Console.WriteLine($"Get Config Elements Offset:{name_element_address[i].ToString("X8")}.(Index:{i})");
            }


            for (int i = 0; i < name_element_address.Length; i++)
            {
                br.BaseStream.Seek(name_element_address[i], SeekOrigin.Begin);

                name_list[i] = StreamUtils.readNullterminated(br);
                Console.WriteLine($"Get Config Data:{name_list[i]}");
            }

            int temp = original_offset_file;
            string hex_temp = temp.ToString("X8");
            real_offset_file = Convert.ToInt32(hex_temp.Substring(1), 16);
            OriginalOffsetHex = hex_temp;
            OriginalCSizeHex = csize_file.ToString("X");

            IsVirtualFile = hex_temp[0].Equals('B') ? true : false;

            br.BaseStream.Seek(real_offset_file, SeekOrigin.Begin);


            if(IsVirtualFile )
            {
                VirtualFileName = StreamUtils.readNullterminated(br);


            }


            if (!IsVirtualFile)
            {
                file_data = br.ReadBytes(csize_file);
                IsCompressed = BLZ4Utils.IsBLZ4(file_data);

                if (file_data.Length == 0)
                {
                    IsBlankFile = true;
                }

            }
            else
            {
                file_data = new byte[0];
            }





        }



        public string GetWindowsFileName()
        {

            if (name_list.Length >= 4)
            {
                return name_list[3].Replace('/', '\\');
            }
            else
            {

                return null;

            }



        }

        public string GetFileMD5Name()
        {

            return CryptUtils.GetMD5HashFromBytes(file_data);

        }




    }
}
