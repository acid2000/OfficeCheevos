using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;

namespace CheevoService
{
    static class Program
    {
        private static HTTPServer httpServer = new HTTPServer(5001);
        private static CheevoTracker tracker;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new Service1() 
            //};
            //ServiceBase.Run(ServicesToRun);

            var dbFilename = Properties.Settings.Default.ConnectionString.Replace("Data Source=", "");
            dbFilename = dbFilename.Remove(dbFilename.IndexOf(";"));

            FileInfo dbFileInfo = new FileInfo(dbFilename);

            if (!dbFileInfo.Exists)
            {
                Database.Setup();
            }

            foreach (var cheevoFile in Directory.GetFiles(Properties.Settings.Default.CheevoPacksDirectory))
            {
                foreach (var cheevo in File.ReadAllLines(cheevoFile))
                {
                    //Test1,Test1Description,Test1Category,500
                    var data = cheevo.Split(new[] { ',' });
                    Database.AddCheevo(data[0], data[1], data[2], int.Parse(data[3]));
                }
            }

            tracker = new CheevoTracker();

            httpServer.OnNewResponse += processRequestResponse;
            httpServer.Start();
        }

        static void processRequestResponse(object data)
        {
            var context = data as HttpListenerContext;

            HttpListenerResponse response = context.Response;

            do
            {
                var memStream = new MemoryStream();

                try
                {
                    if (context.Request.Url.LocalPath.StartsWith("/listing"))
                    {
                        const string user_parameter = "?user=";
                        string queryFull = context.Request.Url.Query;

                        if (queryFull.StartsWith(user_parameter))
                        {
                            // remove the ?user= string then check it looks like a username
                            string potentialUsername = queryFull.Remove(0, user_parameter.Length);

                            if (potentialUsername.Length == 0)
                            {
                                break;
                            }

                            tracker.RequestReceived(potentialUsername, memStream);
                        }
                    }
                    else if (context.Request.Url.LocalPath.StartsWith("/cheevos"))
                    {
                        Cheevo.CheevosAsCSV(memStream);
                    }
                    else if (context.Request.Url.LocalPath.StartsWith("/nominate"))
                    {
                        var queryFull = context.Request.Url.Query.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                        if (queryFull.Length == 3)
                        {
                            string user = "";
                            string proposes = "";
                            int cheevo = -1;

                            const string userStr = "user=";
                            const string proposesStr = "proposes=";
                            const string cheevoStr = "cheevo=";

                            if (queryFull[0].StartsWith(userStr))
                            {
                                user = queryFull[0].Remove(0, userStr.Length);
                            }
                            if (queryFull[1].StartsWith(proposesStr))
                            {
                                proposes = queryFull[1].Remove(0, proposesStr.Length);
                            }
                            if (queryFull[2].StartsWith(cheevoStr))
                            {
                                var id = queryFull[2].Remove(0, cheevoStr.Length);
                                cheevo = int.Parse(id);
                            }

                            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(proposes) && cheevo != -1)
                            {
                                tracker.ProposeCheevo(user, proposes, cheevo);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }

                var outData = memStream.ToArray();
                context.Response.ContentLength64 = outData.Length;
                context.Response.OutputStream.Write(outData, 0, outData.Length);
            }
            while (false);

            context.Response.OutputStream.Close();
        }
    }
}
