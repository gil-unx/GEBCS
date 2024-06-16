using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static GECV.Log;

namespace GERDP_RE
{
    internal class NullDecoder : IDecoder
    {
        void IDecoder.Decode(ResDataSet set, FileInfo res_file)
        {
            Info($"没有实现这个解码器！{set.name}不能被处理！");
            return;
        }
    }
}
