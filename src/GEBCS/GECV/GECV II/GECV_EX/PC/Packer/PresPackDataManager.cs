using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.PC.Packer
{




    internal class PresPackDataManager
    {


        
        public List<PresPackCountryNode> nodes = new List<PresPackCountryNode>();


        public List<string> registerInformation = new List<string>();

        



        public PresPackDataManager(string data_dir_path)
        {

            DirectoryInfo data_dir = new DirectoryInfo(data_dir_path);


            DirectoryInfo[] country_dirs = data_dir.GetDirectories("????????_*",SearchOption.TopDirectoryOnly);

            if(country_dirs.Length >6 || country_dirs.Length <=0)
            {
                throw new FileNotFoundException($"{data_dir.FullName} Just Get Error Number ({country_dirs.Length}) Dirs.");
            }


            if(country_dirs.Length == 1)
            {
                Console.WriteLine($"{data_dir.FullName} Is Single Country Pres.");
            }
            else
            {
                Console.WriteLine($"{data_dir.FullName} Is Countries Pres.");
            }

            for(int i = 0; i < country_dirs.Length; i++)
            {

                var country = country_dirs[i];
                Console.WriteLine($"Id:{i}:{country.Name}:Building.");

                PresPackCountryNode ppcn = new PresPackCountryNode(i);

                DirectoryInfo[] set_dirs = country.GetDirectories("????????_*", SearchOption.TopDirectoryOnly);

                for(int si =0; si < set_dirs.Length; si++)
                {
                    var dataset = set_dirs[si];

                    Console.WriteLine($"Id:{i}-{si}:{country.Name}\\{dataset.Name}:Building.");

                    //PresPackDataSetNode ppdsn = new PresPackDataSetNode(si,ppcn);
                    PresPackDataSetNode ppdsn = ppcn.AddNode(si);

                    FileInfo[] files = dataset.GetFiles("????????_*.bin",SearchOption.TopDirectoryOnly);

                    for(int ssi=0; ssi < files.Length; ssi++)
                    {

                        var bin_file = files[ssi];
                        Console.WriteLine($"Id:{i}-{si}-{ssi}:{country.Name}\\{dataset.Name}\\{bin_file.Name}:Building.");

                        //PresPackDataNode ppdn = new PresPackDataNode(bin_file.FullName, ssi, ppdsn);
                        PresPackDataNode ppdn = ppdsn.AddNode(ssi,bin_file.FullName);

                        
                        registerInformation.Add(ppdn.GetId());

                    }


                    
                }

                this.nodes.Add(ppcn);

            }

            


        }


        public bool IsSingleCountryPres()
        {
            return nodes.Count == 1;
        }


        public static byte[] GetCacheByMD5(string md5)
        {

            if (PresPackDataNode.FileCacheMap.ContainsKey(md5))
            {
                return PresPackDataNode.FileCacheMap[md5];
            }
            else
            {
                throw new InvalidDataException($"{md5} Not In FileCache.");
            }

        }


        


    }
}
