using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GECV_EX.PC.Packer
{
    internal class PresPackCountryNode
    {

        public int id;

        private List<PresPackDataSetNode> nodes = new List<PresPackDataSetNode>();

        
        


        public PresPackCountryNode(int id)
        {
            
            this.id = id;
            
        }


        public PresPackDataSetNode AddNode(int id)
        {

            var result = new PresPackDataSetNode(id, this);
            this.nodes.Add(result);
            Console.WriteLine($"Registered Data Set Node:{id}.");
            return result;
        }

        public PresPackDataSetNode GetNode(int id)
        {
            return this.nodes[id];
        }

        public int GetNodeCount()
        {
            return nodes.Count;
        }




    }
}
