using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
namespace Translator
{
    static class GPSSCode
    {
        private static List<GPSSNode> visited = new List<GPSSNode>();

        public static List<GPSSNode> Vertex
        {
            get => visited;
        }
        public static string Code { get; set; } = "";

        public static string AddGenerateBlock(params double[] options)
        {
            return string.Format(string.Format("GENERATE {0}\n", string.Join(",", options)));
        }

        public static string AddTerminateteBlock(double option, int name)
        {
            return string.Format(string.Format("TERMINATE {0}\n", (object)option));
        }

        public static string AddTransferCode(GPSSNode node, double option, GPSSNode childNode)
        {
            return string.Format(string.Format("TRANSFER {0},, label_{1}\n", option, childNode.Name));
        }

        public static string AddNodeCode(string block, params double[] options)
        {
            return string.Format(string.Format("SEIZE {0}\n", block) + 
                   string.Format("ADVANCE {0}\n", string.Join(",", options)) + 
                   string.Format("RELEASE {0}\n", block));
        }

        public static string AddQueueCode(string block, params double[] options)
        {
            return string.Format(string.Format("QUEUE {0}\n", block) + 
                   string.Format("DEPART {0}\n", block));
        }

        public static void MakeCode(GPSSNode tree)
        {
            GPSSCode.Code = "";
            GPSSCode.visited.Clear();
            GPSSCode.BuildCode(tree);
            GPSSCode.ShowCode(tree);
            GPSSCode.ClearCode();
        }

        private static void ShowCode(GPSSNode tree)
        {
            if (GPSSCode.visited.Exists((x => x.Name == tree.Name)))
            {
                return;
            }
            if (tree.Children.Count == 0 && GPSSNode.Last == tree.Name)
            {
                GPSSCode.visited.Add(tree);
                GPSSCode.Code += tree.NodeCode;
            }
            else
            {
                for (int i = 0; i < tree.Children.Count; i++)
                {
                    GPSSCode.visited.Add(tree);
                    if (!GPSSCode.visited.Exists(x => x.Name == tree.Children[i].Name))
                    {
                        GPSSCode.Code += tree.NodeCode;
                        GPSSCode.ShowCode(tree.Children[i]);
                    }
                }
            }
        }

        private static void BuildCode(GPSSNode tree)
        {
            if (tree.Children.Count == 0 && GPSSNode.Last == tree.Name)
            {
                tree.NodeCode += GPSSCode.AddTerminateteBlock(1.0, 1);
            }
            else
            {                
                for (int index = 0; index < tree.Children.Count; ++index)
                {
                    if((tree.Name + 1) == tree.Children[index].Name)
                    {
                        GPSSCode.BuildCode(tree.Children[index]);
                    }
                    else
                    {
                        if(tree.Children.Count > 2)
                        {
                           tree.Transfers[index] = tree.Transfers[index] / (1 - tree.Transfers[0]);                           
                        }
                        tree.NodeCode += GPSSCode.AddTransferCode(tree, tree.Transfers[index], tree.Children[index]);
                    }           
                }                
            }
        }
        
        private static void ClearCode()
        {
            for (int i = 0; i < visited.Count; i++)
            {
                if(!Regex.IsMatch(Code, $@"TRANSFER 0.\d*,, label_{i}\n"))
                {
                    Code = Regex.Replace(Code, $"label_{i} ", "");
                }
            }
        }        
    }
}
