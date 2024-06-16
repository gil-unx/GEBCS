using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GERDP
{
    internal class ResDataSet
    {

        public string name { get; private set; }
        public long address { get; private set; }
        public long count { get; private set; }

        public long reader_position { get; private set; }

        public bool isPS4 { get; private set; }

        public ExperimentalDecoder decoder { get; set; }

        private ResDataSet() { }


        private ResDataSet(string name,long address,long count,long reader_postision,bool isPS4)
        {
            this.name = name;
            this.address = address;
            this.count = count;
            this.reader_position = reader_postision;
            this.isPS4 = isPS4;
            this.decoder = new ExperimentalDecoder();
        }

        public static ResDataSet BuildResDataSet(BinaryReader br,string name,bool isPS4)
        {
            long address;
            long count;

            long p = br.BaseStream.Position;

            if (isPS4)
            {
                address = br.ReadInt64();
                count = br.ReadInt64();
            }
            else
            {
                address = br.ReadInt32();
                count = br.ReadInt32();
            }

            return new ResDataSet(name,address,count,p,isPS4);


        }


    }
}
