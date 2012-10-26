using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheevoService
{
    class CheevoUser
    {
        public string Username { get; private set; }
        
        private List<Cheevo> currentCheevos = new List<Cheevo>();

        public CheevoUser(string user)
        {
            Username = user;
        }

        public void Add(Cheevo cheevo)
        {
            currentCheevos.Add(cheevo);
        }

        public DateTime GetLastUpdateTime()
        {
            DateTime time = DateTime.MinValue;
            foreach (var cheevo in currentCheevos)
            {
                if (cheevo.Awarded > time)
                {
                    time = cheevo.Awarded;
                }
            }
            return time;
        }

        public IEnumerable<Cheevo> ObtainedCheevos
        {
            get
            {
                return currentCheevos;
            }
        }
    }
}
