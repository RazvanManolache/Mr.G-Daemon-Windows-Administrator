using GalaSoft.MvvmLight;
using MrG.Daemon.Control.Data;
using MrG.Daemon.Control.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MrG.Daemon.Manage
{
    public class MainWindowVM: ViewModelBase
    {
        private RemoteServerConfig? remoteServerConfig;
        public RemoteServerConfig? RemoteServerConfig
        {
            get { return remoteServerConfig; }
            set { 
                Set(ref remoteServerConfig, value); 
                RaisePropertyChanged(nameof(PossibleConfigLocalIssue));
                RaisePropertyChanged(nameof(PossibleConfigRemoteIssue));
                RaisePropertyChanged(nameof(PossibleConfigAccountIssue));
                RaisePropertyChanged(nameof(DaemonLocation));

            }
        }

        private bool hasUpdate;
        public bool HasUpdate
        {
            get { return hasUpdate; }
            set { Set(ref hasUpdate, value); }
        }

        public Visibility UpdateVisible
        {
            get
            {
                if (HasUpdate)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        public bool NotificationDaemonServiceVisible
        {
            get { return !string.IsNullOrWhiteSpace(NotificationDaemonServiceMessage); }
        }

        public Visibility PossibleConfigLocalIssue
        {
            get
            {
                
                if (RemoteServerConfig!=null && RemoteServerConfig.ServerType== Control.Enums.ServerType.Local && !IsLocalDaemon && !WebConnected)
                {
                    return Visibility.Visible;
                }
               
                return Visibility.Collapsed;
            }
        }

        public Visibility PossibleConfigRemoteIssue
        {
            get
            {
                if (RemoteServerConfig != null && RemoteServerConfig.ServerType == Control.Enums.ServerType.Remote&& !WebConnected)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public Visibility PossibleConfigAccountIssue
        {
            get
            {
                if (RemoteServerConfig != null && RemoteServerConfig.ServerType == Control.Enums.ServerType.Account && !WebConnected)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public bool DiskVisible { get; set; } = true;

        public bool NeedConfigure{ get; set; }

       

        public ObservableCollection<DiskInfo> DiskInfo { get; set; } = new ObservableCollection<DiskInfo>();

        public string NotificationDaemonServiceMessage
        {
            get
            {
                if (webConnected)
                {
                    if (IsLocalDaemon)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return $"Service is connected, but is not on this machine. It cannot be started from a stopped state";
                    }
                }
                else
                {
                    if (IsLocalDaemon)
                    {
                        switch (NotificationDaemonServiceStatus)
                        {
                            case ServiceControllerStatus.StartPending:
                                return "Service is starting";
                            case ServiceControllerStatus.StopPending:
                                return "Service is stopping";
                            case ServiceControllerStatus.Paused:
                                return "Service is paused";
                            case ServiceControllerStatus.PausePending:
                                return "Service is pausing";
                            case ServiceControllerStatus.Stopped:
                                return "Service is stopped";
                        }
                    }
                    else
                    {
                        NeedConfigure = true;
                        return string.Empty;
                    }
                    return string.Empty;
                }
               
            }

        }

        private bool _isLocalDaemon;

        public bool IsLocalDaemon
        {
            get
            {
                return _isLocalDaemon;
            }
            set
            {
                _isLocalDaemon = value;
                RaisePropertyChanged(nameof(DaemonLocation));
                RaisePropertyChanged(nameof(NotificationDaemonServiceStatusText));
                RaisePropertyChanged(nameof(StartServerVisible));
                RaisePropertyChanged(nameof(StopServerVisible));
                RaisePropertyChanged(nameof(RestartServerVisible));
                RaisePropertyChanged(nameof(StartServerEnabled));
                RaisePropertyChanged(nameof(StopServerEnabled));
                RaisePropertyChanged(nameof(RestartServerEnabled));
                RaisePropertyChanged(nameof(PossibleConfigLocalIssue));
                RaisePropertyChanged(nameof(PossibleConfigRemoteIssue));
                RaisePropertyChanged(nameof(PossibleConfigAccountIssue));
                


            }
        }

        public string DaemonLocation
        {
            get
            {
                if(RemoteServerConfig!=null)
                {
                    switch (RemoteServerConfig.ServerType)
                    {
                        case Control.Enums.ServerType.Local:
                            if (IsLocalDaemon)
                            {
                                return "Local";
                            }
                            return "Local w/ service";
                        case Control.Enums.ServerType.Remote:

                            return "Remote";
                        case Control.Enums.ServerType.Account:
                            return "Account";
                       
                          
                    }
                    

                }
                return "Unknown";
            }
        }

        public Visibility StartServerVisible
        {
            get
            {
                if(IsLocalDaemon)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        public bool StartServerEnabled {
            get
            {
                if (IsLocalDaemon)
                {
                    return NotificationDaemonServiceStatus == ServiceControllerStatus.Stopped;
                }
                return false;
            }
        }
        public Visibility StopServerVisible
        {
            get
            {
                if (IsLocalDaemon|| webConnected)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public bool StopServerEnabled 
        {
            get
            {
                if (IsLocalDaemon)
                {
                    return NotificationDaemonServiceStatus == ServiceControllerStatus.Running;
                }
                return webConnected;
            }
        }
       
        public Visibility RestartServerVisible
        {
            get
            {
                if (IsLocalDaemon|| webConnected)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public bool RestartServerEnabled
        {
            get
            {
                if (IsLocalDaemon)
                {
                    return NotificationDaemonServiceStatus == ServiceControllerStatus.Running;
                }
                return webConnected;
            }
        }



public ServiceControllerStatus NotificationDaemonServiceStatus { get; set; }

        public string NotificationDaemonServiceWebStatus
        {
            get
            {
                if(webConnected)
                {
                    return "Connected";
                }
                return "Not Connected";
            }
           
        }

        public string NotificationDaemonServiceStatusText
        {
            get
            {                
                if(IsLocalDaemon)
                {
                    return NotificationDaemonServiceStatus.ToString();
                }
                return NotificationDaemonServiceWebStatus;

            }
        }

        public ObservableCollection<SubApplication> SubApplications { get; set; } = new ObservableCollection<SubApplication>();

        private bool webConnected;
        public bool WebConnected
        {
            get { return webConnected; }
            set { 
                Set(ref webConnected, value);
                RaisePropertyChanged(nameof(NotificationDaemonServiceMessage));
                RaisePropertyChanged(nameof(NotificationDaemonServiceVisible));
                RaisePropertyChanged(nameof(StartServerVisible));
                RaisePropertyChanged(nameof(StopServerVisible));
                RaisePropertyChanged(nameof(RestartServerVisible));
                RaisePropertyChanged(nameof(StartServerEnabled));
                RaisePropertyChanged(nameof(StopServerEnabled));
                RaisePropertyChanged(nameof(RestartServerEnabled));
                RaisePropertyChanged(nameof(NotificationDaemonServiceStatusText));
                RaisePropertyChanged(nameof(NotificationDaemonServiceWebStatus));
                RaisePropertyChanged(nameof(PossibleConfigLocalIssue));
                RaisePropertyChanged(nameof(PossibleConfigRemoteIssue));
                RaisePropertyChanged(nameof(PossibleConfigAccountIssue));
            }

        }
        public StringBuffer LogBuffer { get; internal set; } = new StringBuffer();

        private bool logVisible;

        public bool LogVisible { get => logVisible; set => Set(ref logVisible, value); }
    }
}
