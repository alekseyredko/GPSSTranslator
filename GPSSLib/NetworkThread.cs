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
        private double[][] matrix;

        public GPSSLib.GPSSNode Tree { get; private set; }

        public double[][] Matrix
        {
            get
            {
                return matrix;
            }
            private set
            {
                matrix = value;
            }
        }

        public List<GPSSNode> Nodes { get; set; }

        public string[] NodeDesc { get; private set; }

        public int ThreadNum { get; private set; }

        public bool IsMatrixExpanded { get; private set; }

        public void ExpandMatrix()
        {
            int oldLen = matrix.Length;
            for (int i = 0; i < oldLen; i++)
            {
                if (matrix[i].Any(x => x < 1 && x != 0))
                {
                    double[] tempLine = new double[matrix.Length+1];
                    matrix[i].CopyTo(tempLine,0);

                    Array.Clear(matrix[i], 0, matrix[i].Length);
                    
                    for (int j = 0; j < matrix.Length; j++)
                    {
                        Array.Resize(ref matrix[j], matrix.Length + 1);
                    }

                    matrix[i][matrix.Length] = 1;

                    Array.Resize(ref matrix, matrix.Length + 1);
                    matrix[matrix.Length - 1] = tempLine;                    
                }               
            }
            IsMatrixExpanded = true;
        }

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

        public NetworkThread(double[][] Matrix, string[] desc, int threadNum, bool IsExpand = false)
        {
            this.ThreadNum = threadNum;
            this.Matrix = Matrix;
            this.NodeDesc = desc;
            
            if (IsExpand)
                ExpandMatrix();
            else Tree = GPSSNode.BuildTree(this.Matrix, ThreadNum);            
        }
    }
}
