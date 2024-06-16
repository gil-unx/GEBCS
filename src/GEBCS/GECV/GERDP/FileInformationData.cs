using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERDP
{
    internal struct FileInformationData
    {

        public string name_1;
        public string ext_2;
        public string unk_3;
        public string function_4;
        public string function_args_5;
        public long ORIGIN_COUNT; //原始数据的数量，下面是原始数据。
        public List<string> ORIGIN_DATA;

    }
}
