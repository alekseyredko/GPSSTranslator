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
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
namespace GPSSLib
{
    public partial class MainWindow : Window
    {
        private NetworkData NetData;        
        private CodeBuilder builder;
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
            //считывание данных сети
            NetData = new NetworkData();
            if (!NetData.IsDataReaded(openFileDialog.FileName))
            {
                System.Windows.MessageBox.Show("Ошибка. Неверная матрица", "Ошибка");
                BuildTreeButton.IsEnabled = false;                
            }
            else
            {
                MatrixTextBox.Text = "";
                //вывод данных сети //заменить на все параметры
                foreach (var item in NetData.Threads)
                {
                    this.MatrixTextBox.Text += string.Join("\n", item.NodeDesc);
                    this.MatrixTextBox.Text += '\n';
                }
                this.BuildTreeButton.IsEnabled = true;
            }
        }

        //построение кода 
        private void BuildTreeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NetData == null)
            {
                System.Windows.MessageBox.Show("Матрица не выбрана");
            }
            else
            {   
                //добавление кода в узлы
                builder = new CodeBuilder(NetData);
                CodeTextBox.Text = builder.MakeCode();
                ResultTextBox.Text += "Код построен\n";

                TreeVars.Items.Clear();
                SchemeVars.Items.Clear();

                for (int i = 0; i < NetData.Threads.Count; i++)
                {
                    TreeVars.Items.Add($"Поток {i + 1}");
                    SchemeVars.Items.Add($"Поток {i + 1}");
                }
                TreeVars.SelectedIndex = 0;
                SchemeVars.SelectedIndex = 0;
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

        //сохранение кода в txt файлик
        private void CodeSaveItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*|GPS Model (*.gps)|*.gps";
        
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            else
            {
                File.WriteAllText(saveFileDialog.FileName, builder.Code);
            }
        }

        private void TreeButton_Click(object sender, RoutedEventArgs e)
        {
            //передалать для переключения между потоками
            
            var graph = BuildTree(NetData.Threads[TreeVars.SelectedIndex].Nodes);
            Visualize(graph, TreeGridView , LayerDirection.TB);
            ResultTextBox.Text += "Дерево показано\n";
        }

        private void SchemBuild_Click(object sender, RoutedEventArgs e)
        {
            //передалать для переключения между потоками
            var graph = BuildScheme(NetData.Threads[SchemeVars.SelectedIndex].Nodes);
            Visualize(graph, SchemGridView, LayerDirection.LR);
            ResultTextBox.Text += "Схема построена\n";
        }

        //построение дерева
        //TODO: сделать рекурсивным
        private Graph BuildTree(List<GPSSNode> vertex)
        {
            Graph graph = new Graph("graph");
            
            
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

        //TODO: переделать схему построения, вывод вероятностей с 3
        //TODO: сделать рекурсивным
        //построение схемы сети
        private Graph BuildScheme(List<GPSSNode> vertex)
        {
            Graph graph = new Graph("graph");
            
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
