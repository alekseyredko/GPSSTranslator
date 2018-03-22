using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
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
                
        private static void AddTransferCode(GPSSNode node, double option, int childNode, int threadNum)
        {
            node.NodeCode = string.Format("TRANSFER {0:N2},,label_{1}_{2}\n", option, childNode, threadNum+1)
                .Replace(" 0,", " 0.");
        }
        private static string AddTransferCode(GPSSNode node, double option, GPSSNode childNode, int threadNum)
        {
            return string.Format("TRANSFER {0:N2},,label_{1}_{2}\n", option, childNode.Name, threadNum + 1)
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
                        $"SEIZE b{node.Name}\n" +
                        AddDepart1(node.Name, threadNum)+
                        $"ADVANCE ({param[1]}({string.Join(",", param.Skip(2))}))\n" +
                        $"RELEASE b{node.Name}\n"+
                        AddDepart2(node.Name, threadNum);
                    break;
                case "FACILITY_MULTICHANNEL":
                    res =  AddQueue(node.Name, threadNum)+
                        $"b_{node.Name} STORAGE {param[1]} \n" +
                        $"ENTER b{node.Name}\n" +
                        AddDepart1(node.Name, threadNum) +
                        $"ADVANCE ({param[2]}({string.Join(",", param.Skip(3))}))\n" +
                        $"LEAVE b{node.Name}\n"+
                        AddDepart2(node.Name, threadNum);
                    break;
            }
            //ИЗМЕНИТЬ            
            node.NodeCode = node.NodeCode.Substring(0, node.NodeCode.IndexOf(' ') + 1)
                + res + node.NodeCode.Substring(node.NodeCode.IndexOf(' ') + 1);
        }
     
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
                if(!networkData.Threads[i].IsMatrixExpanded)
                {
                    BuildCode(networkData.Threads[i].Tree, i);
                    //обход дерева для записи в строку
                    ShowCode(networkData.Threads[i].Tree);                   
                    //упорядочивание узлов в массиве по имени узла
                    visited.OrderBy(x => x.Name);
                    networkData.Threads[i].Nodes = visited;                   
                }

                else
                {                   
                    RecursiveBuild(networkData.Threads[i], visited, i);
                    networkData.Threads[i].Nodes = visited;                    
                }
                Code = string.Join("\n", visited.Select(x => x.NodeCode));
                ClearCode(networkData.Threads[i].IsMatrixExpanded);
            }
            
            return Code;
        }


        private void RecursiveBuild(NetworkThread thread, List<GPSSNode> visited, int num = 0, int m = 0, int n = 0)
        {
            for (int i = m; i < thread.Matrix.Length; i++)
            {
                for (int j = n; j < thread.Matrix.Length; j++)
                {
                    if(thread.Matrix[i][j]==1)
                    {
                        if (visited.Any(x => x.Name == i))
                            return;

                        var node = new GPSSNode(null, i, num+1);
                        AddNodeCode(thread.GetNextNodeDesc, node, num);
                        visited.Add(node);
                        RecursiveBuild(thread, visited,num, j, 0);
                    }
                    else if(thread.Matrix[i].Any(x=>x != 0 && x < 1))
                    {
                        double firstTransfer = thread.Matrix[i].First(x => x != 0);
                        if (visited.Any(x => x.Name == i))
                            return;

                        for (int k = 0; k < thread.Matrix[i].Length; k++)
                        {                           
                            if (thread.Matrix[i][k] !=0)
                            {
                                //пересчитать вероятности, где больше двух переходов
                                if (thread.Matrix[i].Count(x => x != 0) > 2)
                                    thread.Matrix[i][k] /= (1 - firstTransfer);
                                
                                var node = new GPSSNode(null, i, num+1);
                                visited.Add(node);
                                AddTransferCode(node, thread.Matrix[i][k], k, num);                               
                            }
                        }
                        return;
                    }                    
                }
            }
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
                        //изменено добавление transfer
                        tree.NodeCode+=CodeBuilder.AddTransferCode(tree, tree.Transfers[index], tree.Children[index], num);
                    }
                }
            }
        }

        private void ClearCode(bool IsExp)
        {                       
            var lines = Code.Split('\n').ToList();

            if (IsExp)
            {
                for (int i = 0; i < lines.Count - 2; i++)
                {
                    if (lines[i].StartsWith("TRANSFER"))
                    {
                        var line = lines[i].Split(',').Last();
                        for (int j = 1; j < 10; j++)
                        {
                            if (lines[i + j].StartsWith(line))
                            {
                                visited.RemoveAll(x => x.NodeCode.StartsWith(lines[i]));
                                lines.RemoveAt(i);
                                i--;
                                break;
                            }
                        }
                    }
                }
            }

            Code = string.Join("\n", lines);

            for (int i = 0; i < visited.Count; i++)
            {                
                if (!Regex.IsMatch(Code, $@"TRANSFER 0.\d*,,label_{i}_\d\n"))
                {
                    Code = Regex.Replace(Code, $@"label_{i}_\d ", "");
                }
            }
            lines = Code.Split('\n').ToList();
            Code = "";
            for (int i = 0; i < lines.Count - 2; i++)
            {
                var line = lines[i].Split(' ');
                if (lines[i].Count(x => x == ' ') == 1)
                {
                    Code += string.Format("{0,-12}{1,-12}{2,-12}", " ",
                        lines[i].Split(' ')[0], lines[i].Split(' ')[1]) + '\n';
                }
                else if (lines[i]!="")
                {
                    Code += string.Format("{0,-12}{1,-12}{2}",
                        lines[i].Split(' ')[0], lines[i].Split(' ')[1], lines[i].Split(' ')[2]) + '\n';
                }
            }
        }   
    }
}
