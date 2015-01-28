using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingTracker.Library.Bandwidth;
using PingTracker.Library.Ping;

namespace PingTracker.Library.DataDisplay {
    /// <summary>
    /// Holds all components used to display data to the user
    /// </summary>
    public class Display : INotifyPropertyChanged {
        private string _average;

        /// <summary>
        /// Holds and manages the log of pings.
        /// </summary>
        public Loger Loger { get; set; }
        /// <summary>
        /// Used to select the URL for the pinging.
        /// </summary>
        public UrlSelector UrlSelector { get; set; }
        /// <summary>
        /// Used to do bandwidth analyzing.
        /// </summary>
        public BandwidthAnalyzer BandwidthAnalyzer { get; set; }
        /// <summary>
        /// Average latency calculated from n previous latencies.
        /// </summary>
        public string Average { get { return _average; } set { _average = value; OnPropertyChanged("Average"); } }



        /// <summary>
        /// Creates a new instance of the  <see cref="Display"/> class.
        /// </summary>
        public Display() {
            Loger = new Loger();
            UrlSelector = new UrlSelector();
            BandwidthAnalyzer = new BandwidthAnalyzer();
        }


        /// <summary>
        /// Updates the display values with new ping result
        /// </summary>
        public void Update(PingManipulatorResult pingManipulatorResult) {
            Loger.Log(pingManipulatorResult);

            string averageText = String.Format("{0} ms", Math.Round(pingManipulatorResult.Average, 2));
            Average = averageText;
        }

        /// <summary>
        /// Resets everything in preparation of new pinging start.
        /// </summary>
        public void Reset() {
            Average = String.Empty;
            Loger.LogHistory.Clear();
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
}
