using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
namespace GPSSLib
{
    public class NetworkData
    {       
        //добавить описание узлов
        public List<NetworkThread> Threads{ get; private set; }
        
        public int ThreadCount { get; private set; }

        public int NodeCount { get; private set; }
        
        //переделеать для нескольких потоков
        public bool IsDataReaded(string path)
        {
            try
            {
                int index = 2;

                string[] data = File.ReadAllLines(path);

                if (!data[0].StartsWith("NODE_COUNT"))
                    return false;
                //считывание количества узлов
                else NodeCount = int.Parse(data[0].Split(' ')[1]);

                //считывание количества потоков
                if (!data[1].StartsWith("THREAD_NUM"))
                    return false;
                else ThreadCount = int.Parse(data[1].Split(' ')[1]);

                var nodeDesc = new string[NodeCount];
                var matrix = new double[NodeCount][];
                Threads = new List<NetworkThread>(ThreadCount);
                
                for (int i = 0; i < ThreadCount; i++)
                {
                    nodeDesc = new string[NodeCount];
                    matrix = new double[NodeCount][];
                    //считывание описания узлов
                    for (int j = 0; j < NodeCount; j++)
                    {
                        nodeDesc[j] = data[index];
                        index++;
                    }
                    //считывание матрицы
                    for (int j = 0; j < NodeCount; j++)
                    {
                        matrix[j] = data[index].Split(' ')
                            .Select(Convert.ToDouble).ToArray();
                        index++;
                    }
                    bool flag = matrix.Last().Last() == 1;
                    Threads.Add(new NetworkThread(matrix, nodeDesc, i + 1, flag));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Исходные данные неверны");
                return false;
            }
            return true;
        }       
    }
}
