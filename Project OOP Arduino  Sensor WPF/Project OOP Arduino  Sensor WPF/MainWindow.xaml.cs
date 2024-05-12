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
using System.Windows.Threading;


namespace Project_OOP_Arduino__Sensor_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort _serialPort;
        DispatcherTimer _dispatcherTimer;
        private int maximaleAfstand;
        private int opslaanDelay;
        private DateTime lastSavedTime = DateTime.MinValue;


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
            string settingsFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Settings.json");
            string json = File.ReadAllText(settingsFilePath);

            OpslaanInstellingen settings = JsonSerializer.Deserialize<OpslaanInstellingen>(json);

            maximaleAfstand = settings.MaxAfstand;
            opslaanDelay = settings.Vertraging;

            txtbxMaxAfstand.Text = maximaleAfstand.ToString();
            txtbxOpslaanDelay.Text = opslaanDelay.ToString();

            progressAfstand.Maximum = maximaleAfstand;

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Start();

            DisplayLastSavedData();
        }

        private void _dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            lblTimer.Content = $"{DateTime.Now.ToLongTimeString()} \t\t {DateTime.Now.ToLongDateString()}";
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

            if (float.TryParse(data, out float distance))
            {
                if (distance > maximaleAfstand)
                {
                    UpdateLabels(distance, Brushes.Red);
                    if (CanSaveData())
                    {
                        SaveData(distance);
                        lastSavedTime = DateTime.Now;
                    }
                    DisplayLastSavedData();
                }
                else
                {
                    UpdateLabels(distance, Brushes.White);
                }
            }
        }

        private void UpdateLabels(float afstand, Brush kleur)
        {
            Dispatcher.Invoke(() =>
            {
                lblAfstand.Content = $"{afstand} Cm";
                lblAfstand.Background = kleur;
                progressAfstand.Value = afstand;
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
                Datum = dayString,
                Tijd = time,
                Afstand = afstand
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
                        lblLaatsteOvers.Content = lastData.Datum + " " + lastData.Tijd;
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
                progressAfstand.Maximum = maximaleAfstand;
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wrong input data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnShowMore_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow(1);
        }

        private bool CanSaveData()
        {
            TimeSpan elapsedTime = DateTime.Now - lastSavedTime;
            return elapsedTime.TotalSeconds >= opslaanDelay;
        }

        private void UpdateWindow(int getal)
        {
            WindowData _windowData = new WindowData();
            if (getal == 1)
            _windowData.Show();
        }

        private void btnUpdateDelay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                opslaanDelay = Convert.ToInt32(txtbxOpslaanDelay.Text);
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wrong input data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings()
        {
            OpslaanInstellingen settings = new OpslaanInstellingen()
            {
                MaxAfstand = maximaleAfstand,
                Vertraging = opslaanDelay
            };

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            string settingsFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Settings.json");

            File.WriteAllText(settingsFilePath, json);
        }
    }
}