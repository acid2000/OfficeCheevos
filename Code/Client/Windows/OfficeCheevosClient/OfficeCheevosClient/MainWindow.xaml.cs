using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using NotifyMessageDemo;
using OfficeCheevosClient.Properties;
using Timer = System.Timers.Timer;

namespace OfficeCheevosClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WebClient webClientInstance = new WebClient();
        private NotifyMessageManager notifyMessageMgr = new NotifyMessageManager(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height, 420, 96 );
        private Timer updateTimer = new Timer(Settings.Default.updateTimeInSeconds * 1000);

        public MainWindow()
        {
            InitializeComponent();
            webClientInstance.DownloadStringCompleted += rssDataDownloaded;
            updateTimer.Elapsed += queryServer;

            updateTimer.Start();
        }

        private void AddCheevo(NotifyMessage msg)
        {
            notifyMessageMgr.EnqueueMessage(msg);
        }

        void rssDataDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {
            XmlReader reader = XmlReader.Create(new StringReader(e.Result));
            SyndicationFeed downloadFeed = SyndicationFeed.Load(reader);
            if (downloadFeed != null && 
                downloadFeed.LastUpdatedTime > Settings.Default.lastUpdated)
            {
                // we have a new cheevo!
                string name = "Time to call helpdesk!";
                var icon = OfficeCheevosClient.Properties.Resources.cheevo;

                var msg = new NotifyMessage("/OfficeCheevosClient;component/Resources/cheevo.png", name, name, () => MessageBox.Show(""));
                var t = new Thread(() =>
                                   Dispatcher.Invoke(DispatcherPriority.Normal, new Action<NotifyMessage>(AddCheevo), msg));
                t.Start();
            }

            Settings.Default.lastUpdated = DateTime.Now;
            Settings.Default.Save();
        }

        void queryServer(object sender, ElapsedEventArgs e)
        {
            if (!webClientInstance.IsBusy)
            {
                webClientInstance.DownloadStringAsync(new Uri(Settings.Default.serverRSSURL));
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var msg = new NotifyMessage("/OfficeCheevosClient;component/Resources/cheevo.png", "Office Cheevo", "Show popup on screen (100pts)", () => MessageBox.Show(""));
            notifyMessageMgr.EnqueueMessage(msg);
        }
    }
}
