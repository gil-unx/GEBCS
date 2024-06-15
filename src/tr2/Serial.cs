using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEBCS.tr2
{
    
    public class INT8
    {
        public string EncodingType { get; set; }
        public List<List<sbyte>> Data { get; set; }
    }
    public class INT16
    {
        public string EncodingType { get; set; }
        public List<List<short>> Data { get; set; }
    }
    public class INT32
    {
        public string EncodingType { get; set; }
        public List<List<int>> Data { get; set; }
    }
    public class UINT8
    {
        public string EncodingType { get; set; }
        public List<List<byte>> Data { get; set; }
    }
    public class UINT16
    {
        public string EncodingType { get; set; }
        public List<List<ushort>> Data { get; set; }
    }
    public class UINT32
    {
        public string EncodingType { get; set; }
        public List<List<uint>> Data { get; set; }
    }
    public class FLOAT32
    {
        public string EncodingType { get; set; }
        public List<List<Single>> Data { get; set; }
    }
}
