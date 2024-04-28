using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project_OOP_Arduino__Sensor_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort _serialPort;
        private int maximaleAfstand;

        public MainWindow()
        {
            InitializeComponent();

            cbxPortName.Items.Add("None");
            foreach (string s in SerialPort.GetPortNames())
                cbxPortName.Items.Add(s);

            _serialPort = new SerialPort();
            _serialPort.BaudRate = 9600;

            _serialPort.DataReceived += _serialPort_DataReceived;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                maximaleAfstand = Convert.ToInt32(txtbxMaxAfstand.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wrong input data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbxPortName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                if (cbxPortName.SelectedItem.ToString() != "None")
                {
                    _serialPort.PortName = cbxPortName.SelectedItem.ToString();
                    _serialPort.Open();
                }
            }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadLine();
            data = data.TrimEnd('\r', '\n');

            float distance = float.Parse(data) / 10;

            if (distance > maximaleAfstand)
            {
                UpdateLabels(distance, Brushes.Red);
            }
            else
            {
                UpdateLabels(distance, Brushes.White);
            }
        }

        private void UpdateLabels(float afstand, Brush kleur)
        {
            Dispatcher.Invoke(() =>
            {
                lbl1.Content = $"{afstand} Cm";
                lbl1.Background = kleur;
                UpdateCircle();
            });
        }

        private async void UpdateCircle()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                cirkelDataOntvangen.Fill = new SolidColorBrush(Colors.Blue);
            });

            await Task.Delay(100);

            Application.Current.Dispatcher.Invoke(() =>
            {
                cirkelDataOntvangen.Fill = new SolidColorBrush(Colors.Gray);
            });
        }

        private void btnUpdateMax_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                maximaleAfstand = Convert.ToInt32(txtbxMaxAfstand.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wrong input data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}