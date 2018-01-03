using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    class GPSSNode
    {
        List<GPSSNode> children = new List<GPSSNode>();
        List<double> transfers = new List<double>();//вероятности перехода         
        bool IsVisited = false;
        

        public GPSSNode Parent
        {
            get;
            private set;
        }

        public List<GPSSNode> Children
        {
            get => children;
        }

        public List<double> Transfers
        {
            get => transfers;
        }

        public string NodeCode { get; set; } = "";

        public int Name
        {
            get;
            private set;
        }

        public static int Last
        {
            get;
            private set;
        }

        public GPSSNode(GPSSNode Parent, int Name)
        {
            this.Parent = Parent;
            this.Name = Name;
            NodeCode = $"\nlabel_{Name} block_{Name}\n";
        }

        public GPSSNode()
        {

        }

        public static GPSSNode BuildTree(Matrix m)
        {
            var matrix = m.GetMatrix;
            var node = new GPSSNode(null, 0);
            node.IsVisited = true;
            Last = matrix.Length-1;

            var visitedNodes = new List<int>();

            for (int i = 0; i < matrix.Length; i++)
            {
                if (!AreAllChildrenVisited(node))
                {
                    for (int k = 0; k < node.children.Count; k++)
                    {
                        if (!node.children[k].IsVisited)
                        {
                            node.children[k].IsVisited = true;
                            node = node.children[k];
                            break;
                        }
                        
                    }
                }

                for (int j = 0; j < matrix[i].Length; j++)
                {
                    if (visitedNodes.Exists(x => x == node.Name))
                    {
                        break;
                    }
                    if (matrix[i][j] != 0 &&  matrix[i][j]<1)
                    {                        
                        node.children.Add(new GPSSNode(node, j));
                        if (matrix[i].Count(x => x !=0)>2)
                        {
                            node.transfers.Add(matrix[i][j] / (1 - matrix[i].First(x => x != 0)));
                        }
                        else
                        {
                            node.transfers.Add(matrix[i][j]);
                        }
                    }

                    if(matrix[i][j] == 1)
                    {
                        node.children.Add(new GPSSNode(node, j));                        
                        node.transfers.Add(1);
                        break;
                    }                     
                }
                
                if (node.children.Count == 0)
                {
                    node.IsVisited = true;
                    while (AreAllChildrenVisited(node))
                    {
                        node.IsVisited = true;
                        visitedNodes.Add(node.Name);

                        node = node.Parent;
                        i--;
                        
                        if (node.Parent == null)
                        {
                            return node;
                        }                        
                    }                    
                }
            }
            return node;
        }

        private static bool AreAllChildrenVisited(GPSSNode node)
        {
            if(node.children.Count == 0)
            {
                return true;
            }            

            for (int i = 0; i < node.children.Count; i++)
            {
                if(!node.children[i].IsVisited)
                {
                    return false;
                }
            }
            return true;
        }    
    }
}
