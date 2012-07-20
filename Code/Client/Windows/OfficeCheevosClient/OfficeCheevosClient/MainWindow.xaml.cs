using System;
using System.Collections.Generic;
using System.Linq;
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
using NotifyMessageDemo;

namespace OfficeCheevosClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyMessageManager notifyMessageMgr = new NotifyMessageManager(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height, 200, 150 );

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var msg = new NotifyMessage("", "Office Cheevo", "Show popup on screen (100pts)", ()=> MessageBox.Show(""));
            notifyMessageMgr.EnqueueMessage(msg);
        }
    }
}
