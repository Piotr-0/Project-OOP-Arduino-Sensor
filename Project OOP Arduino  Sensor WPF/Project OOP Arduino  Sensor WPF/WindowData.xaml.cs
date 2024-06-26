﻿using System.IO.Ports;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using static System.IO.Path;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.ObjectModel;


namespace Project_OOP_Arduino__Sensor_WPF
{
    /// <summary>
    /// Interaction logic for WindowData.xaml
    /// </summary>
    public partial class WindowData : Window
    {

        private MainWindow _mainWindow;

        public WindowData(MainWindow mainWindow)
        {
            InitializeComponent();

            _mainWindow = mainWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayData();
        }

        private void btnClearData_Click(object sender, RoutedEventArgs e)
        {

            string filePath = Combine(Environment.CurrentDirectory, "Data_Overschrijdingen.json");
            if (File.Exists(filePath))
            {
                try
                {
                    File.WriteAllText(filePath, string.Empty);
                    _mainWindow.DisplayLastSavedData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Niet mogelijk om data te verwijderen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Bestand niet gevonden", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            DisplayData();
        }

        public void DisplayData()
        {
            string[] jsonLines = File.ReadAllLines("Data_Overschrijdingen.json");
            List<OpslaanData> dataList = new List<OpslaanData>();

            foreach (string line in jsonLines)
            {
                OpslaanData data = JsonConvert.DeserializeObject<OpslaanData>(line);
                dataList.Add(data);
            }

            Dispatcher.Invoke(() =>
            {
                listbxData.ItemsSource = dataList;
            });
        }
    }
}
