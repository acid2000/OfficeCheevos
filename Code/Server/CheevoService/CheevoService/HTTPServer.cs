using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace CheevoService
{
    class HTTPServer : IDisposable
    {
        public event ParameterizedThreadStart OnNewResponse;

        private HttpListener httpListener;

        public HTTPServer(int port)
        {
            string prefix = "http://localhost:"+port+"/";

            httpListener = new HttpListener();
            httpListener.Prefixes.Add(prefix);
        }

        public void Start()
        {
            httpListener.Start();

            while (httpListener.IsListening)
            {
                HttpListenerContext context = httpListener.GetContext();
                Thread processResponse = new Thread(OnNewResponse);
                processResponse.Start(context);
            }
        }

        public void Dispose()
        {
            httpListener.Stop();
        }
    }
}
