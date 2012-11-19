using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CheevoService
{
    class CheevoTracker
    {
        public Dictionary<string, CheevoUser> userLookup;

        public CheevoTracker()
        {
            userLookup = Database.LoadPoppedCheevos();
        }

        public void RequestReceived(string user, Stream outputStream)
        {
            lock (userLookup)
            {
                if (userLookup.ContainsKey(user))
                {
                    RSSGenerator.Generate(userLookup[user], outputStream);
                }
                else
                {
                    RSSGenerator.Generate(outputStream);
                }
            }
        }

        public bool ProposeCheevo(string user, string proposes, int id)
        {
            // you cannot propose yourself
            if (user == proposes)
            {
                return false;
            }

            // make sure cheevo has not already been given
            if (userLookup.ContainsKey(proposes))
            {
                foreach (var cheevo in userLookup[proposes].ObtainedCheevos)
                {
                    if (cheevo.ID == id)
                    {
                        return true;
                    }
                }
            }

            if (Database.ProposeCheevo(user, proposes, id))
            {
                lock (userLookup)
                {
                    userLookup = Database.LoadPoppedCheevos();
                    return true;
                }
            }
            return false;
        }

        public void ListUsersAsCSV(MemoryStream memStream)
        {
            var data = Encoding.ASCII.GetBytes(string.Join(",", userLookup.Keys));
            memStream.Write(data, 0, data.Length);
        }
    }
}
