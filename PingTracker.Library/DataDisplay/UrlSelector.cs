using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PingTracker.Library.Framework;

namespace PingTracker.Library.DataDisplay {
    /// <summary>
    /// Used to select the URL for the pinging.
    /// </summary>
    public class UrlSelector : INotifyPropertyChanged {
        private ICommand _useURLCommand;
        private string _url;

        /// <summary>
        /// Current URL
        /// </summary>
        public string URL { get { return _url; } set { _url = value; OnPropertyChanged("URL"); } }

        /// <summary>
        /// Command used to use the URL.
        /// </summary>
        public ICommand UseURL { get { return _useURLCommand; } }



        /// <summary>
        /// Creates a new instance of the  <see cref="UrlSelector"/> class.
        /// </summary>
        public UrlSelector() {
            _url = "www.google.com";
            _useURLCommand = new Command(OnUseURL, CanUseURL);
        }


        /// <summary>
        /// Notifies the URL change.
        /// </summary>
        private void OnUseURL() {
            OnURLChanged(URL);
        }

        /// <summary>
        /// Method for checking if UseURLCommand can be executed.
        /// </summary>
        private bool CanUseURL() {
            return !String.IsNullOrEmpty(URL);
        }

        /// <summary>
        /// When URL changes this event fires.
        /// </summary>
        public event EventHandler<string> URLChanged;
        /// <summary>
        /// URLChanged executing helper method.
        /// </summary>
        public void OnURLChanged(string url) {
            if (URLChanged != null) {
                URLChanged(this, url);
            }
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
