using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PingTracker.Library.Framework;

namespace PingTracker.Library.Bandwidth {
    /// <summary>
    /// Used for selecting the desired interface(Internet adapter)
    /// </summary>
    public class InterfaceSelector : INotifyPropertyChanged {
        private List<NetworkInterface> _interfaces;
        private NetworkInterface _selectedInterface;
        private ICommand _refreshInterfacesCommand;

        private Task _interfaceRefreshTask;


        /// <summary>
        /// Command for refreshing available interfaces.
        /// </summary>
        public ICommand RefreshInterfacesCommand { get { return _refreshInterfacesCommand; } }
        /// List of available interfaces.
        /// </summary>
        public List<NetworkInterface> NetworkInterfaces { get { return _interfaces; } set { _interfaces = value; OnPropertyChanged("NetworkInterfaces"); } }
        /// <summary>
        /// Currently selected interface
        /// </summary>
        public NetworkInterface SelectedInterface { get { return _selectedInterface; } set { _selectedInterface = value; OnPropertyChanged("SelectedInterface"); } }



        /// <summary>
        /// Creates a new instance of the  <see cref="InterfaceSelector"/> class.
        /// </summary>
        public InterfaceSelector() {
            _refreshInterfacesCommand = new Command(RefreshInterfaces, CanRefreshInterfaces);
        }



        /// <summary>
        ///  Initialize all network interfaces on this computer
        /// </summary>
        public void RefreshInterfaces() {
            _interfaceRefreshTask = Task.Factory.StartNew<NetworkInterface[]>(() => {
                // Grab all local interfaces to this computer
                return NetworkInterface.GetAllNetworkInterfaces();

            }).ContinueWith((previousTask) => {
                // set data
                NetworkInterfaces = new List<NetworkInterface>(previousTask.Result);
                // set selected first one
                if (NetworkInterfaces.Count > 0) {
                    SelectedInterface = NetworkInterfaces.First();
                }
                // notify
                OnInterfacesChanged();
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        /// <summary>
        /// Method for checking if RefreshInterfacesCommand can be executed.
        /// </summary>
        private bool CanRefreshInterfaces() {
            return (_interfaceRefreshTask == null || _interfaceRefreshTask.IsCompleted);
        }

        /// <summary>
        /// InterfacesChanged executing helper method.
        /// </summary>
        protected void OnInterfacesChanged() {
            if (InterfacesChanged != null) {
                InterfacesChanged(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// When user selects different interface this event fires.
        /// </summary>
        public event EventHandler InterfacesChanged;
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
