using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
namespace Translator
{
    public class NetworkData
    {

        private double[][] matrix;
        int position = 0;
        //добавить описание узлов
        public double[][] Matrix
        {
            get => matrix;
        }

        public string[] NodeDesc { get; private set; }

        //получение следующего узла (вывод по порядку)
        public string GetNextNodeDesc
        {
            get
            {
                if (position > NodeDesc.Length)
                {
                    position = 0;
                    return NodeDesc[0];
                }
                else return NodeDesc[position++];
            }
        }
        
        public bool IsDataReaded(string path)
        {
            try
            {
                int index = 1;
                string[] data = File.ReadAllLines(path);
                //считывание описания узлов
                NodeDesc = new string[int.Parse(data[0])];
                for (int i = 1; i < NodeDesc.Length + 1; i++)
                {
                    NodeDesc[i - 1] = data[i];
                    index++;
                }

                matrix = new double[int.Parse(data[index])][];
                //считывание матрицы
                index++;
                for (int i = 0; i < matrix.Length; i++)
                {
                    matrix[i] = data[index].Split(new char[] { ' ' },
                                    StringSplitOptions.RemoveEmptyEntries)
                                    .Select(Convert.ToDouble)
                                    .ToArray();
                    index++;
                }
                if (matrix.Length != matrix[0].Length)
                {
                    return false;
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
