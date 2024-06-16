using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.PC.Packer
{
    internal class PresPackDataSetNode
    {

        public int id;

        public List<PresPackDataNode> nodes = new List<PresPackDataNode>();

        public PresPackCountryNode parent;



        public PresPackDataSetNode(int id,PresPackCountryNode parent)
        {
            this.id = id;
            this.parent = parent;
            

        }

        public PresPackDataNode AddNode(int id,string bin_file)
        {
            var result = new PresPackDataNode(bin_file, id, this);

            this.nodes.Add(result);
            Console.WriteLine($"Registered File Node:{result.GetId()}.");
            return result;
        }

        public PresPackDataNode GetNode(int id)
        {
            return this.nodes[id];
        }

        public PresPackCountryNode GetParent()
        {
            return parent;
        }

        public int GetNodeCount()
        {
            return nodes.Count;
        }

        public bool IsBlankSet()
        {
            return nodes.Count == 0;
        }

    }
}
