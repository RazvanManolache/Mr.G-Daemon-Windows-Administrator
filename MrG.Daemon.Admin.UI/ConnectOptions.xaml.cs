using MrG.Daemon.Control.Data;
using MrG.Daemon.Control.Events;
using MrG.Daemon.Control.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MrG.Daemon.Admin.UI
{
    /// <summary>
    /// Interaction logic for ConnectOptions.xaml
    /// </summary>
    public partial class ConnectOptions : Window
    {
        private readonly object lockObject = new object();

        public ConnectOptions()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void StartScanning(object sender, RoutedEventArgs e)
        {
            ScanButton.IsEnabled = false;
            OKButton.IsEnabled = false;
            TestButton.IsEnabled = false;
            IpTextBox.Text = "";

            await Task.Run(() => ScanNetwork());
        }

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            ScanButton.IsEnabled = false;
            OKButton.IsEnabled = false;
            TestButton.IsEnabled = false;

            await Task.Run(() => ScanNetwork());
        }

        private async Task<bool> CheckIP(string ip)
        {
            WebSocketClient client = new WebSocketClient($"http://{ip}:8180/ws");
            try
            {
                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                return false;
            }
            return true;
        }

        public async void ScanNetwork()
        {
            var ips = GetLocalIPv4();
            var found = false;
            ips = ips.Where(ip => ip.StartsWith("192.168")).ToList();
            var cnt = 0;
            ScanProgressBar.Dispatcher.Invoke(() => { 
                ScanProgressBar.Visibility = Visibility.Visible;
                ScanProgressBar.Value = 0;
                ScanProgressBar.Maximum = ips.Count * 255; 
            });
           
            await Task.Run(() =>
            {
                Parallel.ForEach(ips, ip =>
                {
                    var parts = ip.Split('.');
                    var subnet = $"{parts[0]}.{parts[1]}.{parts[2]}";

                    Parallel.For(1, 256, new ParallelOptions() { MaxDegreeOfParallelism=4}, i =>
                    {
                        if (!found)
                        {
                            var ipToScan = $"{subnet}.{i}";
                            var res = CheckIP(ipToScan).GetAwaiter().GetResult();

                            if (res)
                            {
                                lock (lockObject)
                                {
                                    if (!found)
                                    {
                                        IpTextBox.Dispatcher.Invoke(() => IpTextBox.Text = ipToScan);
                                        found = true;
                                    }
                                }
                            }
                        }
                        
                        cnt++;
                        ScanProgressBar.Dispatcher.Invoke(() => {
                            ScanProgressBar.Value = cnt;
                        });
                    });
                });
            });

            if (!found)
            {
                IpTextBox.Dispatcher.Invoke(() =>
                {
                    this.serverInvalid.Visibility = Visibility.Collapsed;
                    this.noServerFound.Visibility = Visibility.Visible;
                });
            }

            ScanButton.Dispatcher.Invoke(() => ScanButton.IsEnabled = true);
            
            TestButton.Dispatcher.Invoke(() => TestButton.IsEnabled = true);
            ScanProgressBar.Dispatcher.Invoke(() => {
                ScanProgressBar.Visibility = Visibility.Collapsed;
            });
        }

        public List<string> GetLocalIPv4()
        {
            List<string> ips = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ips.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ips;
        }

        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            TestButton.IsEnabled = false;
            var text = IpTextBox.Text;
            if (await CheckIP(text))
            {
                OKButton.IsEnabled = true;
                this.serverInvalid.Visibility = Visibility.Collapsed;
            }
            else
            {
                OKButton.IsEnabled = false;
                this.serverInvalid.Visibility = Visibility.Visible;
            }
            TestButton.IsEnabled = true;
        }
        public event EventHandler<RemoteServerConfig>? ConfigChanged;
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            
            RemoteServerConfig config = null;
            var localserver = LocalServer.IsChecked;
            if (localserver == true)
            {
                config = new RemoteServerConfig();
                config.ServerType = Control.Enums.ServerType.Local;
                
            }
            
            var remoteServer = RemoteServer.IsChecked;
            if (remoteServer == true)
            {
                config = new RemoteServerConfig();
                config.ServerType = Control.Enums.ServerType.Remote;
                config.IP = IpTextBox.Text;
            }

            if(config != null)
            {
                ConfigChanged?.Invoke(this, config);

            }
            this.Close();



        }

        private void IpTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.serverInvalid.Visibility = Visibility.Collapsed;
            //check if the ip is valid
            if (IPAddress.TryParse(IpTextBox.Text, out IPAddress address))
            {
                OKButton.IsEnabled = false;
                TestButton.IsEnabled = true;
                this.serverInvalid.Visibility = Visibility.Collapsed;
            }
            else
            {
                OKButton.IsEnabled = false;
                TestButton.IsEnabled = false;
                this.serverInvalid.Visibility = Visibility.Visible;
            }
        }

        private void LocalChecked(object sender, RoutedEventArgs e)
        {
            if (OKButton != null)
                OKButton.IsEnabled = true;
        }

        private void RemoteChecked(object sender, RoutedEventArgs e)
        {
            if (OKButton != null)
                OKButton.IsEnabled = false;
        }
    }
}
