using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GECV
{
    public static class Define
    {

        /*
         * QPCK
         * INT 四字节魔术码 = 0x37402858
         * INT 文件总数，文件索引数量=文件总数数量=文件数据数量
         * {文件索引} [八字节偏移，八字节大小，四字节哈希]
         * {文件数据} 
         */
        public static readonly int QPCK_MAGIC = 0x37402858;
        public static readonly int PRES_MAGIC = 0x73657250;
        public static readonly int BLZ4_MAGIC = 0x347a6c62;


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



    }

}
