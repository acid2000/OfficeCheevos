using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace NotifyMessageDemo
{
    public class NotifyMessageManager
    {
        private readonly object _syncRoot = new object();
        protected int MaxPopup { get; set; }
        protected List<AnimatedLocation> DisplayLocations { get; set; }
        protected ConcurrentQueue<NotifyMessage> QueuedMessages { get; set; }
        protected NotifyMessageViewModel[] DisplayMessages { get; set; }
        private CancellationTokenSource _cts;
        private bool _isStarted;

        private delegate void MethodInvoker();

        public NotifyMessageManager(double screenWidth, double screenHeight, double popupWidth, double popupHeight)
        {
            MaxPopup = Convert.ToInt32(screenHeight / popupHeight) - 1;
            DisplayLocations = new List<AnimatedLocation>(MaxPopup);
            DisplayMessages = new NotifyMessageViewModel[MaxPopup];
            QueuedMessages = new ConcurrentQueue<NotifyMessage>();

            double left = (screenWidth - popupWidth) /2;
            double top = 0;

            for (int index = 0; index < MaxPopup; index++)
            {
                if (index == 0)
                {
                    DisplayLocations.Add(new AnimatedLocation(left, left, 0, 0));
                }
                else
                {
                    var previousLocation = DisplayLocations[index - 1];
                    DisplayLocations.Add(new AnimatedLocation(
                        left, left, previousLocation.ToTop, previousLocation.ToTop + popupHeight));
                }
            }
            _isStarted = false;
        }

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public void Start()
        {
            lock (_syncRoot)
            {
                if (!_isStarted)
                {
                    _cts = new CancellationTokenSource();
                    StartService(_cts.Token);
                    _isStarted = true;
                }
            }
        }

        private Task StartService(CancellationToken cancellationToken)
        {
            var dispatcher = Application.Current.MainWindow.Dispatcher;

            Application.Current.MainWindow.Activate();

            return Task.Factory.StartNew(() =>
            {
                do
                {
                    // Gets the next display location in the screen
                    int nextLocation = FindNextLocation();

                    if (nextLocation > -1)
                    {
                        NotifyMessage msg = null;
                        //  Retrieve the message from the queue
                        if (QueuedMessages.TryDequeue(out msg))
                        {
                            //  construct a View Model and binds it to the Popup Window
                            var viewModel = new NotifyMessageViewModel(msg, 
                                DisplayLocations[nextLocation], 
                                () => DisplayMessages[nextLocation] = null);    // Free the display location when the popup window is closed
                            DisplayMessages[nextLocation] = viewModel;

                            //  Use Application.Current.MainWindow.Dispatcher to switch back to the UI Thread to create popup window
                            dispatcher.BeginInvoke(
                                new MethodInvoker(() => 
                                {
                                    var window = new NotifyMessageWindow() 
                                    { 
                                        Owner = Application.Current.MainWindow,
                                        DataContext = viewModel,
                                        ShowInTaskbar = false
                                    };
                                    window.Show();
                                }), DispatcherPriority.Background);
                        }
                    }
                    Thread.Sleep(1000);

                } while (QueuedMessages.Count > 0 && !cancellationToken.IsCancellationRequested);

                Stop();
            });
        }

        public void Stop()
        {
            lock (_syncRoot)
            {
                if (_isStarted)
                {
                    StopService();
                    _isStarted = false;
                }
            }
        }

        private void StopService()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private int FindNextLocation()
        {
            for (int index = 0; index < DisplayMessages.Length; index++)
            {
                if (DisplayMessages[index] == null)
                    return index;
            }
            return -1;
        }

        public void EnqueueMessage(NotifyMessage msg)
        {
            QueuedMessages.Enqueue(msg);
            Start();
        }
    }
}
