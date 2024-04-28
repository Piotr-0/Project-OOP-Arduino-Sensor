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

            DisplayLastSavedData();
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

                    txtbxMaxAfstand.IsEnabled = true;
                    btnUpdateMax.IsEnabled = true;
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
                SaveData(distance);
                DisplayLastSavedData();
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
                lblAfstand.Content = $"{afstand} Cm";
                lblAfstand.Background = kleur;
                UpdateCircle();
            });
        }

        private async void SaveData(float afstand)
        {
            DateTime currentTime = DateTime.Now;
            int day = currentTime.Day;
            string time = currentTime.ToLongTimeString();
            string dayString = currentTime.ToShortDateString();


            OpslaanData dataToSave = new OpslaanData()
            {
                Afstand = afstand,
                Tijd = time,
                Datum = dayString
            };

            string json = JsonSerializer.Serialize(dataToSave);
            if (!string.IsNullOrEmpty(json))
            {
                string filePath = Combine(Environment.CurrentDirectory, "Data_Overschrijdingen.json");
                File.AppendAllText(filePath, json + Environment.NewLine);
            }
        }

        public async void DisplayLastSavedData()
        {
            try
            {
                string filePath = Combine(Environment.CurrentDirectory, "Data_Overschrijdingen.json");
                string[] lines = File.ReadAllLines(filePath);
                List<OpslaanData> dataList = new List<OpslaanData>();

                foreach (string line in lines)
                {
                    OpslaanData data = JsonSerializer.Deserialize<OpslaanData>(line);
                    dataList.Add(data);
                }

                OpslaanData lastData = dataList.OrderByDescending(d => d.Datum).ThenByDescending(d => d.Tijd).FirstOrDefault();

                if (lastData != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblLaatsteOvers.Content = lastData.Tijd;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblLaatsteOvers.Content = "Geen data beschikbaar";
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    lblLaatsteOvers.Content = "Geen data beschikbaar";
                });
            }
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

        private void btnShowMore_Click(object sender, RoutedEventArgs e)
        {
            WindowData windowData = new WindowData();
            windowData.Show();
        }
    }
}