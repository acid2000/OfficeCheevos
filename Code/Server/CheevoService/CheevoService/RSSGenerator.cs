using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Argotic.Syndication;

namespace CheevoService
{
    class RSSGenerator
    {
        private const string Title = "Office Cheevos RSS server";
        private const string Description = "Office Cheevos";
        private const string Link = "https://github.com/acid2000/OfficeCheevos";

        public static void Generate(CheevoUser user, Stream output)
        {
            RssFeed feed = new RssFeed();

            feed.Channel.Link = new Uri(Link);
            feed.Channel.Title = Title;
            feed.Channel.Description = Description;
            // the last build date must correspond to the 'newest' cheevo
            feed.Channel.LastBuildDate = user.GetLastUpdateTime();
            feed.Channel.PublicationDate = user.GetLastUpdateTime();

            foreach (var cheevo in user.ObtainedCheevos)
            {
                RssItem item = new RssItem();
                item.Title = cheevo.Title;
                item.Link = new Uri(Link);
                item.Description = cheevo.Points.ToString();
                item.PublicationDate = cheevo.Awarded;

                feed.Channel.AddItem(item);
            }

            feed.Save(output);
        }

        public static void Generate(Stream output)
        {
            RssFeed feed = new RssFeed();

            feed.Channel.Link = new Uri(Link);
            feed.Channel.Title = Title;
            feed.Channel.Description = Description;

            feed.Save(output);
        }
    }
}
