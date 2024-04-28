using System.IO.Ports;
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

        public WindowData()
        {
            InitializeComponent();
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
                    MessageBox.Show("Data verwijderd");
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.DisplayLastSavedData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to clear data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Data file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void DisplayData()
        {

        }
    }
}
