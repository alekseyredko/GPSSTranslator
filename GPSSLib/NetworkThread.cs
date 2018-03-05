using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSSLib
{
    public class NetworkThread
    {
        int position = 0;
        public GPSSLib.GPSSNode Tree { get; private set; }

        public double[][] Matrix { get; private set; }

        public List<GPSSNode> Nodes { get; set; }

        public string[] NodeDesc { get; private set; }

        //получение следующего узла (вывод по порядку)
        public string GetNextNodeDesc
        {
            get
            {
                if (position == NodeDesc.Length)
                {
                    position = 0;
                    return NodeDesc[0];
                }
                else return NodeDesc[position++];
            }
        }

        public NetworkThread(double[][]Matrix, string[] desc)
        {
            this.Matrix = Matrix;
            this.NodeDesc = desc;
            Tree = GPSSNode.BuildTree(this.Matrix);
        }

        public NetworkThread() { }
    }
}
