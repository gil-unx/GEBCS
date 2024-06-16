using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GECV.Log;

namespace GERDP
{
    internal class NullDecoder : IDecoder
    {
        void IDecoder.Decode(ResDataSet set, byte[] res_data)
        {
            Info($"没有实现这个解码器！{set.name}不能被处理！");
            return;
        }
    }
}
