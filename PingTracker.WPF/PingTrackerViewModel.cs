using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingTracker.Library;
using PingTracker.Library.DataDisplay;
using PingTracker.Library.Graph;
using PingTracker.Library.Ping;

namespace PingTracker.WPF {
    /// <summary>
    /// The view model 
    /// </summary>
    public class PingTrackerViewModel : INotifyPropertyChanged {
        private System.Windows.Threading.Dispatcher _dispatcher;
        private IScrollViewerTrainer _scrollViewerTrainer;

        /// <summary>
        /// Holds the data needed to draw graph.
        /// </summary>
        public Graph Graph { get; set; }
        /// <summary>
        /// Used to get the latency in intervals and other latency logic.
        /// </summary>
        public Pinger Pinger { get; set; }
        /// <summary>
        /// Manipulates the raw pinger data in usable data.
        /// </summary>
        public PingManipulator PingManipulator { get; set; }
        /// <summary>
        /// Holds all components used to display data to the user
        /// </summary>
        public Display Display { get; set; }


        /// <summary>
        /// Creates a new instance of the  <see cref="PingTrackerViewModel"/> class.
        /// </summary>
        public PingTrackerViewModel(System.Windows.Threading.Dispatcher dispatcher, IScrollViewerTrainer scrollViewerTrainer) {
            _dispatcher = dispatcher;
            _scrollViewerTrainer = scrollViewerTrainer;

            Graph = new Graph();
            Pinger = new Pinger();
            PingManipulator = new PingManipulator();
            Display = new Library.DataDisplay.Display();

            Pinger.NewPing += Pinger_NewPing;
            Pinger.Start(Display.UrlSelector.URL);

            Display.UrlSelector.URLChanged += UrlSelector_URLChanged;
        }


        /// <summary>
        /// Stops the pinger.
        /// </summary>
        public Task Stop() {
            Pinger.NewPing -= Pinger_NewPing;
            return Pinger.Stop();
        }

        /// <summary>
        /// When new ping is calculated notifies other components
        /// </summary>
        private void Pinger_NewPing(object sender, PingerData e) {
            try {
                _dispatcher.Invoke(new Action(() => {

                    PingManipulatorResult pingManipulatorResult = PingManipulator.Calculate(e);

                    Graph.Draw(pingManipulatorResult);
                    Display.Update(pingManipulatorResult);
                    //_scrollViewerTrainer.ScrollCenter();
                }));
            }
            catch { }
        }

        /// <summary>
        /// When URL changes stops and restarts everything.
        /// </summary>
        private void UrlSelector_URLChanged(object sender, string e) {
            Pinger.Stop().ContinueWith((previousTask) => {
                _dispatcher.Invoke(new Action(() => {
                    PingManipulator.Reset();
                    Graph.Reset();
                    Display.Reset();
                    Pinger.Start(e);
                }));
            });
        }


        /// <summary>
        /// PropertyChanged executing helper method.
        /// </summary>
        protected void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Defines the class which moves the scroll bar to the correct position.
    /// </summary>
    public interface IScrollViewerTrainer {
        /// <summary>
        /// Scroll the scroll bar to center.
        /// </summary>
        void ScrollCenter();
    }
}
