using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERDP_RE
{
    internal class ResFileManager
    {

        private static HashSet<ResFileManager> manager = new HashSet<ResFileManager>();


        public string file_name;

        public string md5;



        private ResFileManager(string file_name,string md5)
        {
            this.file_name = file_name;
            this.md5 = md5;

            lock (manager) { 

            manager.Add(this);
            }
        }



        public static void Add(string file_name,string md5)
        {
            new ResFileManager(file_name, md5);
        }

        public static bool IsUnique(string file_name,string md5)
        {
            lock(manager) { 

                

            foreach(ResFileManager rfm in manager)
            {

                if(rfm.file_name.Equals(file_name) && rfm.md5.Equals(md5))
                {
                    return false;
                }


            }

            }

            return true;
        }




    }
}
