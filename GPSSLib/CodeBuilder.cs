using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Translator
{
    public class CodeBuilder
    {
        private  List<GPSSNode> visited = new List<GPSSNode>();

        GPSSNode tree;
        NetworkData networkData;

        public CodeBuilder(GPSSNode node, NetworkData data)
        {
            tree = node;
            networkData = data;
        }


        //TODO: реализовать коллекцию
        public List<GPSSNode> Vertex
        {
            get => visited;
        }

        public string Code { get; private set; } = "";
                
        public static string AddTerminateteBlock(double option, int name)
        {
            return string.Format(string.Format("TERMINATE {0}\n", option));
        }

        public static string AddTransferCode(GPSSNode node, double option, GPSSNode childNode)
        {
            return string.Format("TRANSFER {0:N2},, label_{1}\n", option, childNode.Name)
                .Replace(" 0,", " 0.");
        }
        
        /// <summary>
        /// Метод для добавления кода в узел
        /// </summary>
        /// <param name="nodeType">Параметры узла</param>
        /// <param name="nodeName">Имя узла</param>
        /// <returns></returns>
        public static string AddNodeCode(string nodeType, string nodeCode, int nodeName)
        {
            string[] param = nodeType.Split(' ');
            string res = "";
            switch (param[0])
            {
                case "GENERATE":
                    res += string.Format("{0} {1}({2})\n", 
                        param[0], param[1], string.Join(",", param.Skip(2)));
                    break;
                case "TERMINATE":
                    res += string.Join(" ", nodeType);
                    break;
                case "FACILITY_ONECHANNEL":
                    res += string.Format(string.Format("SEIZE b_{0}\n", nodeName) +
                        string.Format("ADVANCE ({0}({1}))\n", param[1], string.Join(",", param.Skip(2))) + 
                        string.Format("RELEASE b_{0}\n", nodeName));
                    break;
                    //вынести в отдельные методы
                case "FACILITY_MULTICHANNEL":
                    res += string.Format(string.Format("b_{0} STORAGE {1} \n", nodeName, param[1]) +
                        string.Format("ENTER b_{0}\n", nodeName) +
                        string.Format("ADVANCE ({0}({1}))\n", param[2], string.Join(",", param.Skip(3))) +
                        string.Format("LEAVE b_{0}\n", nodeName));
                    break;
            }
            return nodeCode.Substring(0, nodeCode.IndexOf(' ')+1)
                + res + nodeCode.Substring(nodeCode.IndexOf(' ')+1);
        }

        public static string AddDistribution(string dist, params double[] values)
        {
            return string.Format("{0}({1}}", dist, string.Join(",", values));
        }

        //переписать для добавления всех переменных
        public static string AddQueueCode(string block, params double[] options)
        {
            return string.Format(string.Format("QUEUE {0}\n", block) + 
                   string.Format("DEPART {0}\n", block));
        }

        //метод для построения кода 
        public string MakeCode(GPSSNode tree)
        {
            Code = "";
            visited.Clear();
            //добавление кода в узлы
            BuildCode(tree);
            //обход дерева для записи в строку
            ShowCode(tree);
            //очистка кода от transfe
            ClearCode();
            return Code;
        }

        //обход дерева для записи в строку
        private void ShowCode(GPSSNode tree)
        {
            if (visited.Exists((x => x.Name == tree.Name)))
            {
                return;
            }
            if (tree.Children.Count == 0 && GPSSNode.Last == tree.Name)
            {
                visited.Add(tree);
               Code += tree.NodeCode;
            }
            else
            {
                for (int i = 0; i < tree.Children.Count; i++)
                {
                    visited.Add(tree);
                    if (!visited.Exists(x => x.Name == tree.Children[i].Name))
                    {
                        Code += tree.NodeCode;
                        ShowCode(tree.Children[i]);
                    }
                }
            }
        }

        private void BuildCode(GPSSNode tree)
        {
            if (tree.Children.Count == 0 && GPSSNode.Last == tree.Name)
            {
                tree.NodeCode += CodeBuilder.AddTerminateteBlock(1.0, 1);
            }
            else
            {
                for (int index = 0; index < tree.Children.Count; ++index)
                {
                    if ((tree.Name + 1) == tree.Children[index].Name)
                    {
                        tree.NodeCode = AddNodeCode(networkData.GetNextNodeDesc, tree.NodeCode, tree.Name);
                        BuildCode(tree.Children[index]);
                    }
                    else
                    {
                        if (tree.Children.Count > 2)
                        {
                            tree.Transfers[index] = tree.Transfers[index] / (1 - tree.Transfers[0]);
                        }
                        tree.NodeCode += CodeBuilder.AddTransferCode(tree, tree.Transfers[index], tree.Children[index]);
                    }
                }
            }
        }


        private void ClearCode()
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
