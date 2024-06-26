﻿using MrG.Daemon.Control.Communication;
using MrG.Daemon.Control.Data;
using MrG.Daemon.Control.Enums;
using System;
using System.Collections.Generic;
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

namespace MrG.Daemon.Manage
{
    /// <summary>
    /// Interaction logic for SubApplication.xaml
    /// </summary>
    public partial class SubApplicationControl : System.Windows.Controls.UserControl
    {
        public event EventHandler<BaseRequest> EventSubApplication;

        public SubApplicationControl()
        {
            InitializeComponent();
        }

        private void SendRequest(RequestTypeEnum ev)
        {
            EventSubApplication?.Invoke(this, new BaseRequest()
            {
                Request = ev,
                App = (DataContext as SubApplication)
            });
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            SendRequest(RequestTypeEnum.AppStart);

        }

        private void RestartClick(object sender, RoutedEventArgs e)
        {
            SendRequest(RequestTypeEnum.AppRestart);
        }

        private void StopClick(object sender, RoutedEventArgs e)
        {
            SendRequest(RequestTypeEnum.AppStop);
        }

        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            SendRequest(RequestTypeEnum.AppUpdate);
        }

        private void RevertChanges(object sender, RoutedEventArgs e)
        {
            SendRequest(RequestTypeEnum.AppList);
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SendRequest(RequestTypeEnum.AppConfig);
        }

        private void CheckUpdateClick(object sender, RoutedEventArgs e)
        {
            SendRequest(RequestTypeEnum.AppCheckUpdate);
        }

        private void UninstallClick(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to uninstall this application?", "Uninstall Application", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                SendRequest(RequestTypeEnum.AppUninstall);
            }
        }
    }
}
