using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.PC.Packer
{
    internal class PresPackDataNode
    {


        public int id;

        public PresPackDataSetNode parent;


        public static Dictionary<string, byte[]> FileCacheMap = new Dictionary<string, byte[]>();

        public string md5;

        public PresFileData presDataInf;

        public PresPackDataSetNode GetParent()
        {
            return parent;
        }

        public PresPackDataNode(string bin_file,int id,PresPackDataSetNode parent)
        {

            this.id = id;
            this.parent = parent;

            if (!Path.Exists(bin_file))
            {
                throw new FileNotFoundException($"{bin_file} Not Exists!");
            }
            

            var xml = Path.GetDirectoryName(bin_file) +'\\' + Path.GetFileNameWithoutExtension(bin_file) + ".xml";

            if(Path.Exists(xml))
            {

                using(StreamReader sr = new StreamReader(File.OpenRead(xml)))
                {
                    presDataInf =  XmlUtils.Load<PresFileData>(sr);
                }


            }
            else
            {
                throw new FileNotFoundException($"{xml} Not Exists!");
            }

            byte[] cache_data;

            byte[] read_data = File.ReadAllBytes(bin_file);

            presDataInf.usize_file = read_data.Length / presDataInf.file_size_mul;

            Console.WriteLine($"BLZ4:{presDataInf.IsCompressed}");
            if (presDataInf.IsCompressed)
            {

                //if(parent.id != 2)
                //{
                //    throw new ArgumentException($"{bin_file} Is Not 3 Set Files.Why Is Compressed?");
                //}
                
                cache_data = BLZ4Utils.PackBLZ4Data(read_data);

                if (!BLZ4Utils.IsBLZ4(cache_data))
                {
                    throw new InvalidDataException($"BLZ4 Build Error!");
                }

            }
            else
            {
                cache_data = read_data;
            }

            presDataInf.csize_file = cache_data.Length / presDataInf.file_size_mul;

            md5 = CryptUtils.GetMD5HashFromBytes(cache_data);

            presDataInf.file_data = cache_data;
            

            if(!FileCacheMap.ContainsKey(md5))
            {
                FileCacheMap.Add(md5, cache_data);
                Console.WriteLine($"Cache {GetId()}.");
            }
            else
            {
                Console.WriteLine($"{GetId()} Was In CacheMap.");
            }


        }


        public string GetId()
        {
            return $"{GetParent().GetParent().id.ToString().PadLeft(8,'0')}-{GetParent().id.ToString().PadLeft(8, '0')}-{id.ToString().PadLeft(8, '0')}-{md5}-{presDataInf.name_list.Length}";
        }
        

    }
}
