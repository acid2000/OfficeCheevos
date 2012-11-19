using OfficeCheevosClient.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OfficeCheevosClient
{
    /// <summary>
    /// Interaction logic for ProposeCheevo.xaml
    /// </summary>
    public partial class ProposeCheevo : Window
    {
        private struct Cheevo
        {
            public int id;
            public string title;
            public string category;
            public string description;
            public int points;

            public override string ToString()
            {
 	            return string.Format("{0} ({1})", title, points);
            }
        }

        private static List<Cheevo> availiableCheevos = new List<Cheevo>();
        private static List<string> users = new List<string>();

        private WebClient webClientInstance = new WebClient();

        public static void UpdateCheevos()
        {
            WebClient downloadCheevoList = new WebClient();
            downloadCheevoList.DownloadStringCompleted += gotNewCheevoList;
            downloadCheevoList.DownloadStringAsync(new Uri(string.Format("{0}cheevos", Settings.Default.serverURL)));

            WebClient downloadUserList = new WebClient();
            downloadUserList.DownloadStringCompleted += gotUserList;
            downloadUserList.DownloadStringAsync(new Uri(string.Format("{0}users", Settings.Default.serverURL)));
        }

        private static void gotNewCheevoList(object sender, DownloadStringCompletedEventArgs e)
        {
            foreach (var cheevoLine in e.Result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var data = cheevoLine.Split(new[] { ',' });

                availiableCheevos.Add(new Cheevo() { id = int.Parse(data[0]), title = data[1], description = data[2], category = data[3], points = int.Parse(data[4]) });
            }
        }

        private static void gotUserList(object sender, DownloadStringCompletedEventArgs e)
        {
            users.AddRange(e.Result.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
        }

        public ProposeCheevo()
        {
            InitializeComponent();

            foreach (var user in users)
            {
                usersBox.Items.Add(user);
            }

            foreach (var cheevo in availiableCheevos)
            {
                cheevoListBox.Items.Add(cheevo);
            }

            if (availiableCheevos.Count() > 0)
            {
                proposeButton.IsEnabled = true;
            }

            webClientInstance.DownloadStringCompleted += queryResponse;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void proposeButton_Click(object sender, RoutedEventArgs e)
        {
            if (cheevoListBox.SelectedItem is Cheevo)
            {
                var query = string.Format("{0}nominate?user={1}?proposes={2}?cheevo={3}", Settings.Default.serverURL, Environment.UserName, usersBox.Text, ((Cheevo) cheevoListBox.SelectedItem).id);

                webClientInstance.DownloadStringAsync(new Uri(query));
            }
        }

        private void queryResponse(object sender, DownloadStringCompletedEventArgs e)
        {
            switch (e.Result)
            {
                case "T":
                    MessageBox.Show("Cheevo proposed", "Proposed", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "F":
                default:
                    MessageBox.Show("Cheevo proposal error", "Proposed", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

    }
}
