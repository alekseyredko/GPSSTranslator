using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using Microsoft.Win32;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
namespace GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Matrix matrix;
        private GPSSNode tree;
        public MainWindow()
        {
            InitializeComponent();
        }

        //считывание файла по кнопке ОК
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            string[] text = File.ReadAllLines(openFileDialog.FileName);
            this.matrix = new Matrix(text);
            if (!this.matrix.IsValidMatrix())
            {
                System.Windows.MessageBox.Show("Ошибка. Неверная матрица", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Hand);
                this.BuildTreeButton.IsEnabled = false;
            }
            else
            {
                this.MatrixTextBox.Text = string.Join("\n", text);
                this.BuildTreeButton.IsEnabled = true;
            }
        }

        private void BuildTreeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.matrix == null)
            {
                System.Windows.MessageBox.Show("Матрица не выбрана");
            }
            else
            {
                tree = GPSSNode.BuildTree(matrix);
                //построение кода
                GPSSCode.MakeCode(tree);
                CodeTextBox.Text = GPSSCode.Code;
                //System.Windows.MessageBox.Show("ГОТОВО");
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(this.CodeTextBox.Text);
        }

        private void CopyMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(this.MatrixTextBox.Text);
        }

        private void PasteMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            this.MatrixTextBox.Text = System.Windows.Clipboard.GetText();
        }

        //визуализация дерева
        //TODO: переделать под WPF
        //сделать дерево, а не схему сети
        private void VisualizeTree(GPSSNode tree)
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            //create a viewer object 
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            //create a graph object 
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            //create the graph content 

            var vertex = GPSSCode.Vertex;

            for (int i = 0; i < vertex.Count; i++)
            {
                for (int j = 0; j < vertex[i].Children.Count; j++)
                {                   
                    if(graph.Edges.Where(x=>x.Source == vertex[i].Name.ToString() && 
                        x.Target == vertex[i].Children[j].Name.ToString()).Count() == 0)
                    {
                        if(vertex[i].Transfers.Count > 1)
                        {
                            graph.AddEdge(vertex[i].Name.ToString(), vertex[i].Transfers[j].ToString(),
                            vertex[i].Children[j].Name.ToString());
                        }
                        else
                        {
                            graph.AddEdge(vertex[i].Name.ToString(),
                            vertex[i].Children[j].Name.ToString());
                        }
                    }                   
                }
            }
            graph.Attr.LayerDirection = LayerDirection.LR;
            //bind the graph to the viewer 
            viewer.Graph = graph;
            //associate the viewer with the form 
            form.SuspendLayout();
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            form.Controls.Add(viewer);
            form.ResumeLayout();
            //show the form 
            form.Show();
        }

        //TODO: добавить сохранение кода в txt
        private void CodeSaveItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
        
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            else
            {
                File.WriteAllText(saveFileDialog.FileName, GPSSCode.Code);
            }
        }

        private void TreeButton_Click(object sender, RoutedEventArgs e)
        {
            VisualizeTree(tree);
        }
    }
}
