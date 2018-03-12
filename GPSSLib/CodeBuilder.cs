using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace GPSSLib
{
    public class CodeBuilder
    {
        private List<GPSSNode> visited = new List<GPSSNode>();

        NetworkData networkData;

        public CodeBuilder(NetworkData data)
        {
            networkData = data;
        }

        public string Code { get; private set; } = "";
                
        //public static string AddTerminateteBlock(double option, int name)
        //{
        //    return string.Format(string.Format("TERMINATE {0}\n", option));
        //}

        private static string AddTransferCode(GPSSNode node, double option, GPSSNode childNode, int threadNum)
        {
            return string.Format("TRANSFER {0:N2},, label_{1}_{2}", option, childNode.Name, threadNum+1)
                .Replace(" 0,", " 0.");
        }
        
        private static void AddNodeCode(string nodeType, GPSSNode node, int threadNum = 0)
        {
            string[] param = nodeType.Split(' ');
            string res = "";
            switch (param[0])
            {
                case "GENERATE":
                    res = $"{param[0]} {param[1]}({string.Join(",", param.Skip(2))})\n" +
                        $"queue net\nqueue net_{threadNum+1}\n";
                    break;
                case "TERMINATE":
                    res = $"depart net\ndepart net_{threadNum+1}\n"+
                        $"{nodeType}\n\n";
                    break;
                case "FACILITY_ONECHANNEL":
                    res = AddQueue(node.Name, threadNum)+
                        $"SEIZE b_{node.Name}\n" +
                        AddDepart1(node.Name, threadNum)+
                        $"ADVANCE ({param[1]}({string.Join(",", param.Skip(2))}))\n" +
                        $"RELEASE b_{node.Name}\n";
                        AddDepart2(node.Name, threadNum);
                    break;
                case "FACILITY_MULTICHANNEL":
                    res =  AddQueue(node.Name, threadNum)+
                        $"b_{node.Name} STORAGE {param[1]} \n" +
                        $"ENTER b_{node.Name}\n" +
                        AddDepart1(node.Name, threadNum) +
                        $"ADVANCE ({param[2]}({string.Join(",", param.Skip(3))}))\n" +
                        $"LEAVE b_{node.Name}\n"+
                        AddDepart2(node.Name, threadNum);
                    break;
            }
            //ИЗМЕНИТЬ            
            node.NodeCode = node.NodeCode.Substring(0, node.NodeCode.IndexOf(' ') + 1)
                + res + node.NodeCode.Substring(node.NodeCode.IndexOf(' ') + 1);
        }

        //public static string AddDistribution(string dist, params double[] values)
        //{
        //    return string.Format("{0}({1}}", dist, string.Join(",", values));
        //}

        //переписать для добавления всех переменных
        private static string AddQueue(int name, int threadNum)
        {
            return $"queue b{name}_queue\n"+
                   $"queue b{name}_{threadNum+1}_queue\n"+
                   $"queue b{name}\n" +
                   $"queue b{name}_{threadNum+1}\n";
        }

        //переименовать методы
        private static string AddDepart1(int name, int threadNum)
        {
            return $"depart b{name}_queue\n"+
                   $"depart b{name}_{threadNum+1}_queue\n";
        }

        //переименовать методы
        private static string AddDepart2(int name, int threadNum)
        {
            return $"depart b{name}\n" +
                   $"depart b{name}_{threadNum+1}\n";
        }

        //метод для построения кода 
        public string MakeCode()
        {
            Code = "";
            for (int i = 0; i < networkData.ThreadCount; i++)
            {
                visited = new List<GPSSNode>();
                visited.Clear();
                //добавление кода в узлы
                BuildCode(networkData.Threads[i].Tree, i);
                //обход дерева для записи в строку
                ShowCode(networkData.Threads[i].Tree);
                //очистка кода от transfer
                ClearCode();
                //упорядочивание узлов в массиве по имени узла
                visited.OrderBy(x => x.Name);
                networkData.Threads[i].Nodes = visited;
            }
            return Code;
        }

        //обход дерева для записи в строку и запись дерева в массив
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
                visited.Add(tree);
                for (int i = 0; i < tree.Children.Count; i++)
                { 
                    if (!visited.Exists(x => x.Name == tree.Children[i].Name))
                    {
                        Code += tree.NodeCode;
                        ShowCode(tree.Children[i]);
                    }
                }
            }
        }

        //добавление кода в узлы
        private void BuildCode(GPSSNode tree, int num = 0)
        {
            if (tree.Children.Count == 0 && GPSSNode.Last == tree.Name)
            {
                AddNodeCode(networkData.Threads[num].GetNextNodeDesc, tree, num);
            }
            else
            {
                for (int index = 0; index < tree.Children.Count; ++index)
                {
                    if ((tree.Name + 1) == tree.Children[index].Name)
                    {
                        AddNodeCode(networkData.Threads[num].GetNextNodeDesc, tree, num);
                        BuildCode(tree.Children[index],num);
                    }
                    else
                    {
                        if (tree.Children.Count > 2)
                        {
                            tree.Transfers[index] = tree.Transfers[index] / (1 - tree.Transfers[0]);
                        }
                        //сократить
                        tree.NodeCode += CodeBuilder.AddTransferCode(tree, tree.Transfers[index], tree.Children[index], num);
                    }
                }
            }
        }
        
        private void ClearCode()
        {
            for (int i = 0; i < visited.Count; i++)
            {
                if (!Regex.IsMatch(Code, $@"TRANSFER 0.\d*,, label_{i}_\d\n"))
                {
                    Code = Regex.Replace(Code, $@"\nlabel_{i}_\d ", "");
                }
            }           
        }        
    }
}
