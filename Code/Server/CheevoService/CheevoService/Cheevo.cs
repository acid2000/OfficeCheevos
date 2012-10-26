using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CheevoService
{
    class Cheevo
    {
        public int ID { get; private set; }
        public string Title { get; private set; }
        public int Points { get; private set; }
        public DateTime Awarded { get; private set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public Cheevo(string title, string description, string category, int points, DateTime awarded, int id)
        {
            Title = title;
            Points = points;
            Category = category;
            Description = description;
            Awarded = awarded;
            ID = id;
        }

        public static void CheevosAsCSV(Stream outStream)
        {
            StreamWriter sr = new StreamWriter(outStream, Encoding.UTF8);
            foreach (var cheevo in Database.GetCheevos())
            {
                var outLine = string.Format("{0},{1},{2},{3},{4}", cheevo.ID, cheevo.Title, cheevo.Description, cheevo.Category, cheevo.Points);
                sr.WriteLine(outLine);
            }
            sr.Flush();
        }
    }
}
