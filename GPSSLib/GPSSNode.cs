using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
namespace GPSSLib
{
    //TODO: представить узлы в виде коллекции
    public class GPSSNode
    {
        List<GPSSNode> children = new List<GPSSNode>();
        List<double> transfers = new List<double>();//вероятности перехода         
        bool IsVisited = false;

        public GPSSNode Parent
        { get; private set;}

        public List<GPSSNode> Children
        {
            get { return children; }
        }

        public List<double> Transfers
        {
            get { return transfers; }
        }

        public string NodeCode { get; set; } = "";

        public int Name
        { get; private set; }

        public static int Last
        { get; private set; }
        public GPSSNode(GPSSNode Parent, int Name, int threadNum = 0)
        {
            this.Parent = Parent;
            this.Name = Name;
            NodeCode = $"label_{Name}_{threadNum} ";
        }
       
        public static GPSSNode BuildTree(double[][] matrix, int threadNum)
        {
            var node = new GPSSNode(null, 0, threadNum);
            node.IsVisited = true;
            Last = matrix.Length-1;
            var visitedNodes = new List<GPSSNode>();

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
                for (int j = 0; j < matrix.Length; j++)
                {
                    if (visitedNodes.Exists(x => x.Name == node.Name))
                    {
                        break;
                    }
                    if (matrix[i][j] != 0 &&  matrix[i][j] < 1)
                    {                        
                        node.children.Add(new GPSSNode(node, j, threadNum));
                        node.transfers.Add(matrix[i][j]);
                    }
                    if(matrix[i][j] == 1)
                    {
                        node.children.Add(new GPSSNode(node, j, threadNum));                        
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
                        visitedNodes.Add(node);
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
