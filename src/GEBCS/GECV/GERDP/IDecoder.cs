using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERDP
{
    internal interface IDecoder
    {


        abstract void Decode(ResDataSet set, byte[] res_data);




    }
}
