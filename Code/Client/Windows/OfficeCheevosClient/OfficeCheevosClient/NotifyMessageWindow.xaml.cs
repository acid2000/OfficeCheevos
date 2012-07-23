using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OfficeCheevosClient.Properties;

namespace NotifyMessageDemo
{
    /// <summary>
    /// Interaction logic for NotifyMessageWindow.xaml
    /// </summary>
    public partial class NotifyMessageWindow : Window
    {
        public NotifyMessageWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var x = OfficeCheevosClient.Properties.Resources.cheevo_sound;
            SoundPlayer player = new SoundPlayer(x);
            player.Play();
        }
    }
}
