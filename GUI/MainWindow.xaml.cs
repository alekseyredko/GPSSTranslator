﻿using System;
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
namespace Translator
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
        private void Visualize(Graph graph, Grid grid, LayerDirection layerDirection)
        {
            //TreeGridView
            System.Windows.Forms.Integration.WindowsFormsHost host =
                new System.Windows.Forms.Integration.WindowsFormsHost();
            //create a viewer object 
            GViewer viewer = new GViewer();
            
            //create the graph content 
            graph.Attr.LayerDirection = layerDirection;
            //bind the graph to the viewer 
            viewer.Graph = graph;
            //associate the viewer with the form            
            viewer.Dock = DockStyle.Fill;
           
            host.Child = viewer;
            grid.Children.Add(host);
        }

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
            var graph = BuildTree();
            Visualize(graph, TreeGridView , LayerDirection.TB);
        }

        private void SchemBuild_Click(object sender, RoutedEventArgs e)
        {
            var graph = BuildScheme();
            Visualize(graph, SchemGridView, LayerDirection.LR);
        }

        private Graph BuildTree()
        {
            Graph graph = new Graph("graph");
            var vertex = GPSSCode.Vertex;
            
            vertex = vertex.Distinct().ToList();
            //прямой обход
            for (int i = 0; i < vertex.Count; i++)
            {
                if (vertex[i].Children.Count != 0)
                {
                    for (int j = 0; j < vertex[i].Children.Count; j++)
                    {
                        if (vertex[i].Children[j].Name == i + 1)
                        {
                            graph.AddEdge(vertex[i].Name.ToString(),
                            vertex[i].Children[j].Name.ToString());
                        }
                    }
                }
            }
            string space = " ";
            //добавление деток
            for (int i = 0; i < vertex.Count; i++)
            {

                for (int j = 0; j < vertex[i].Children.Count; j++)
                {
                    if (graph.Edges.Where(x => x.Source == vertex[i].Name.ToString() &&
                         x.Target == vertex[i].Children[j].Name.ToString()).Count() == 0)
                    {
                        graph.AddEdge(vertex[i].Name.ToString(),
                           vertex[i].Children[j].Name.ToString() + space);
                    }
                    space += " ";
                }
            }

            return graph;
        }

        private Graph BuildScheme()
        {
            Graph graph = new Graph("graph");
            var vertex = GPSSCode.Vertex;

            
            for (int i = 0; i < vertex.Count; i++)
            {
                for (int j = 0; j < vertex[i].Children.Count; j++)
                {
                    if (graph.Edges.Where(x => x.Source == vertex[i].Name.ToString() &&
                         x.Target == vertex[i].Children[j].Name.ToString()).Count() == 0)
                    {

                        if (vertex[i].Transfers.Count >= 2)
                        {
                            if (vertex[i].Transfers.Count == 2)
                            {
                                graph.AddEdge(vertex[i].Name.ToString(),
                                    vertex[i].Transfers[j].ToString(),
                                     vertex[i].Children[j].Name.ToString());
                            }
                            if (vertex[i].Transfers.Count > 2)
                            {
                                graph.AddEdge(vertex[i].Name.ToString(),
                                    (vertex[i].Transfers[j] * (1 - vertex[i].Transfers[0])).ToString(),
                                     vertex[i].Children[j].Name.ToString());
                            }
                        }
                        else
                        {
                            graph.AddEdge(vertex[i].Name.ToString(),
                            vertex[i].Children[j].Name.ToString());
                        }
                    }
                }
            }

            return graph;
        }
    }
}
