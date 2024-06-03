﻿using MrG.Daemon.Control.Communication;
using MrG.Daemon.Control.Data;
using MrG.Daemon.Control.Enums;
using MrG.Daemon.Control.Events;
using MrG.Daemon.Control.Managers;
using System.ServiceProcess;
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
using System.Windows.Forms;

namespace MrG.Daemon.Manage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        MainWindowVM ViewModel;
        DaemonServerManager DaemonServerManager;
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = (MainWindowVM)this.DataContext;
            Startup();
             _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("AppIcon.ico"), // Path to your .ico file
                Visible = true,
                Text = "Your App Name"
            };

            // Create context menu
            var contextMenu = new ContextMenuStrip();
            var showMenu = contextMenu.Items.Add("Show");
            var hideMenu = contextMenu.Items.Add("Hides");
            var exitMenu = contextMenu.Items.Add("Exit");
            showMenu.Click += ShowMenu_Click;
            hideMenu.Click += HideMenu_Click;
            exitMenu.Click += ExitMenu_Click;
            

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += ShowApp;
        }

        private void HideMenu_Click(object? sender, EventArgs e)
        {
            this.Hide();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _notifyIcon.Visible = false; // Ensure tray icon is hidden when app is closed
            base.OnClosed(e);
        }

        private void ExitMenu_Click(object? sender, EventArgs e)
        {
            _notifyIcon.Visible = false; 
            System.Windows.Application.Current.Shutdown();
        }

        private void ShowMenu_Click(object? sender, EventArgs e)
        {
            this.Show();
        }

        private void ShowApp()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void ShowApp(object? sender, EventArgs e)
        {
            
        }

        public void Startup()
        {
            // read config from file
            var file = "config.json";
            if (System.IO.File.Exists(file))
            {
                var config = System.IO.File.ReadAllText(file);
                ViewModel.RemoteServerConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<RemoteServerConfig>(config);
            }
            else
            {
                ViewModel.RemoteServerConfig = new RemoteServerConfig();


                System.IO.File.WriteAllText(file, Newtonsoft.Json.JsonConvert.SerializeObject(ViewModel.RemoteServerConfig));
            }


            DaemonServiceManager.ServiceStatusChanged += LocalServerHelper_ServiceStatusChanged;
            DaemonServerManager = new DaemonServerManager(ViewModel.RemoteServerConfig);

            DaemonServerManager.DiskInfoEvent += DaemonServerManager_DiskInfoEvent;
            DaemonServerManager.ConfigEvent += DaemonServerManager_ConfigEvent;
            DaemonServerManager.FlagsEvent += DaemonServerManager_FlagsEvent;
            DaemonServerManager.StatusesEvent += DaemonServerManager_StatusesEvent;
            DaemonServerManager.SubApplicationsEvent += DaemonServerManager_SubApplicationEvent;
            DaemonServerManager.ConsoleEvent += DaemonServerManager_ConsoleEvent;
            DaemonServerManager.LogEvent += DaemonServerManager_LogEvent;

            DaemonServerManager.WebServerEvent += DaemonServerManager_WebServerEvent;
            DaemonServerManager.Start();
        }

        private void DaemonServerManager_LogEvent(object? sender, LogEvent? e)
        {
            if (e != null && e.AppId != null && e.Message != null)
            {
                if (e.AppId == "daemon")
                {
                    this.ViewModel.LogBuffer.Append(e.Message);
                }
                else
                {
                    var existing = ViewModel.SubApplications.Where(a => a.Id == e.AppId).FirstOrDefault();
                    if (existing != null)
                    {
                        existing.LogBuffer.Append(e.Message);
                    }
                    else
                    {
                        DaemonServerManager.SendMessage(new BaseRequest()
                        {
                            Request = RequestTypeEnum.AppList
                        });
                    }
                }
                
            }
        }

        private void DaemonServerManager_ConsoleEvent(object? sender, LogEvent? e)
        {
            if (e != null && e.AppId!=null && e.Message!=null)
            {
                var existing = ViewModel.SubApplications.Where(a => a.Id == e.AppId).FirstOrDefault();
                if (existing != null)
                {
                    existing.ConsoleBuffer.Append(e.Message);
                }
                else
                {
                    DaemonServerManager.SendMessage(new BaseRequest()
                    {
                        Request = RequestTypeEnum.AppList
                    });
                
                }
            }
        }

        private void SubApplicationControl_EventSubApplication(object sender, BaseRequest e)
        {
            DaemonServerManager.SendMessage(e);
        }

        private void DaemonServerManager_DiskInfoEvent(object? sender, List<DiskInfo>? e)
        {
            if (e != null)
            {
                foreach(var disk in e)
                {
                    var existing = ViewModel.DiskInfo.Where(a => a.Path == disk.Path).FirstOrDefault();
                    if (existing != null)
                    {
                        if(existing.Type == DiskEventEnum.Removed)
                        {
                            ViewModel.DiskInfo.Remove(existing);
                        }
                        else
                        {
                            existing = existing.Update(disk);
                        }
                    }
                    else
                    {
                        ViewModel.DiskInfo.Add(disk);
                    }
                    
                }
            }
        }

        private void DaemonServerManager_SubApplicationEvent(object? sender, List<Daemon.Control.Data.SubApplication>? subApplications)
        {
            if (subApplications == null) return;
            foreach(var subApplication in subApplications)
            {
                if (subApplication != null)
                {
                    var existing = ViewModel.SubApplications.Where(a => a.Id == subApplication.Id).FirstOrDefault();
                    if (existing != null)
                    {
                        existing = existing.Update(subApplication);
                    }
                    else
                    {
                        ViewModel.SubApplications.Add(subApplication);
                    }
                }
            }
            
        }

        

        private void DaemonServerManager_ConfigEvent(object? sender, ConfigData? e)
        {
        }

        private void DaemonServerManager_StatusesEvent(object? sender, List<SubAppStatus>? e)
        {
            foreach(var status in e)
            {
                var existing = ViewModel.SubApplications.Where(a => a.Id == status.Id).FirstOrDefault();
                if(existing != null)
                {
                    existing.Status = status.Status;
                    existing.Running = status.Running;
                }
                else
                {
                    DaemonServerManager.SendMessage(new BaseRequest()
                    {
                        Request = RequestTypeEnum.AppList
                    });
                }
            }
        }

        private void DaemonServerManager_FlagsEvent(object? sender,ApplicationFlagsData? e)
        {
        }

        private void DaemonServerManager_WebServerEvent(object? sender, WebServerEvent e)
        {
            if (ViewModel.WebConnected == false && e.Connected)
            {
                DaemonServerManager.SendMessage(new BaseRequest()
                {
                    Request = RequestTypeEnum.Status
                });
            }
            if(ViewModel.WebConnected != e.Connected)
            {
                ViewModel.WebConnected = e.Connected;
                
            }
            if (!ViewModel.WebConnected)
            {
                ViewModel.SubApplications.Clear();
            }
           
        }

       

        private void LocalServerHelper_ServiceStatusChanged(object? sender, ServiceControllerStatus? e)
        {
            ViewModel.IsLocalDaemon = e.HasValue;
            if (e.HasValue)
            {
                ViewModel.NotificationDaemonServiceStatus = e.Value;
                
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            var details = "The daemon will continue to run in the background";
            if(ViewModel.NotificationDaemonServiceStatus != ServiceControllerStatus.StartPending && ViewModel.NotificationDaemonServiceStatus != ServiceControllerStatus.Running)
            {
                details = "The daemon is currently not running";
            }
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to close the app?", details, System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
                

        }

       
        private void MenuInstallMrGAI_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuInstallComfyAI_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuInstallAutomatic1111_Click(object sender, RoutedEventArgs e)
        {

        }

      

        private void StartDaemon_Click(object sender, RoutedEventArgs e)
        {
            if(ViewModel.IsLocalDaemon)
            {
                DaemonServiceManager.StartService();
            }
          

        }

        private void StopDaemon_Click(object sender, RoutedEventArgs e)
        {
            if(ViewModel.IsLocalDaemon)
            {
                DaemonServiceManager.StopService();
            }
            else
            {
                DaemonServerManager.SendMessage(new BaseRequest()
                {
                    Request = RequestTypeEnum.StopService
                });
            }

        }

        private void RestartDaemon_Click(object sender, RoutedEventArgs e)
        {
            if(ViewModel.IsLocalDaemon)
            {
                DaemonServiceManager.RestartService();
            }
            else
            {
                DaemonServerManager.SendMessage(new BaseRequest()
                {
                    Request = RequestTypeEnum.RestartService
                });
            }

        }

        private void MenuConfigure_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}