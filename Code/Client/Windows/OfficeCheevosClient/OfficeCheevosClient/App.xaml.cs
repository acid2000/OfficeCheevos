using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace OfficeCheevosClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static NotifyIcon icon;

        protected override void OnStartup(StartupEventArgs e)
        {
            App.icon = new NotifyIcon();
            icon.Icon = OfficeCheevosClient.Properties.Resources.tray;
            icon.Visible = true;

            icon.ContextMenu = new ContextMenu();
            icon.ContextMenu.MenuItems.Add("Propose", onPropose);
            icon.ContextMenu.MenuItems.Add("Cheevo list", onList);
            icon.ContextMenu.MenuItems.Add("Exit", onExit);

            base.OnStartup(e);

            ProposeCheevo.UpdateCheevos();
        }

        private void onExit(object sender, EventArgs e)
        {
        }

        private void onList(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void onPropose(object sender, EventArgs e)
        {
            ProposeCheevo cheevoWindow = new ProposeCheevo();
            cheevoWindow.ShowDialog();
                
        }
    }
}
