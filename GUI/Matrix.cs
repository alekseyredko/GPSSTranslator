using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GUI
{
    class Matrix
    {
        private double[][] matrix;
        private string[] text;       

        public double[][] GetMatrix
        {
            get => matrix;
        }


        public Matrix(string[] text)
        {
            this.text = text;         
        }

        public bool IsValidMatrix()
        {
            try
            {    
                matrix = new double[text.Length][];
                for (int i = 0; i < text.Length; i++)
                {
                    matrix[i] = text[i].Split(new char[]{' '}, 
                        StringSplitOptions.RemoveEmptyEntries).Select(Convert.ToDouble).ToArray();
                }
                if (matrix.Length!= matrix[0].Length)
                {
                    return false;
                }

                //проверка суммы вероятностей в строке
                for (int i = 0; i < matrix.Length-1; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < matrix[i].Length; j++)
                    {
                        sum += matrix[i][j];
                    }
                    if (sum != 1.0)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                //Array.Clear(matrix,0,text.Length);
                System.Windows.MessageBox.Show(e.ToString());
                return false;
            }
        }
    }
}
