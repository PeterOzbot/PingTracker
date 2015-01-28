using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PingTracker.Library.Framework;

namespace PingTracker.Library.Bandwidth {
    /// <summary>
    /// Used to do bandwidth analyzing.
    /// TODO: This logic is not implemented correctly yet. For now it only displays the current MB uploaded/downloaded
    /// </summary>
    public class BandwidthAnalyzer : INotifyPropertyChanged {
        private Command _startAnalysisCommand;
        private Command _stopAnalysisCommand;

        private bool _working;
        private BandwidthAnalizerResult _resultData;

        private Task _workingTask;
        private Task stopingTask;
        private CancellationTokenSource _cancellationTokenSource;

        private double _bytesReceived = 0;
        private double _bytesSent = 0;


        /// <summary>
        /// Command used to start analysis
        /// </summary>
        public ICommand StartAnalysisCommand { get { return _startAnalysisCommand; } }
        /// <summary>
        /// Command used to stop analysis
        /// </summary>
        public ICommand StopAnalysisCommand { get { return _stopAnalysisCommand; } }
        /// <summary>
        /// Flag indicating if analyzer is working
        /// </summary>
        public bool Working { get { return _working; } set { _working = value; OnPropertyChanged("Working"); } }
        /// <summary>
        /// Data generated after the analysis is done
        /// </summary>
        public BandwidthAnalizerResult ResultData { get { return _resultData; } set { _resultData = value; OnPropertyChanged("ResultData"); } }
        /// <summary>
        /// Used for selecting the desired interface(Internet adapter)
        /// </summary>
        public InterfaceSelector InterfaceSelector { get; set; }




        /// <summary>
        /// Creates a new instance of the  <see cref="BandwidthAnalyzer"/> class.
        /// </summary>
        public BandwidthAnalyzer() {
            _resultData = new BandwidthAnalizerResult();
            InterfaceSelector = new InterfaceSelector();

            _startAnalysisCommand = new Command(StartAnalysis, CanStartAnalysis);
            _stopAnalysisCommand = new Command(StopAnalysis, CanStopAnalysis);

            // hook on interfaces change for command refresh
            InterfaceSelector.InterfacesChanged += (sender, e) => { UpdateCommands(); };
            // just do the initial interface initialization
            InterfaceSelector.RefreshInterfaces();
        }



        /// <summary>
        /// Prepares everything to start the analysis
        /// </summary>
        private void StartAnalysis() {
            // create cancellation token
            _cancellationTokenSource = new CancellationTokenSource();
            // create SynchronizationContext
            SynchronizationContext synchronizationContext = SynchronizationContext.Current;
            // set working
            Working = true;
            // set empty
            ResultData = new BandwidthAnalizerResult();
            // start ping in interval
            _workingTask = Repeat.Interval(TimeSpan.FromSeconds(2), () => { StartAnalysisInternal(synchronizationContext); }, _cancellationTokenSource.Token);
            // continue with task setting falg to IsRunning false to know when task stoped
            _workingTask = _workingTask.ContinueWith((previousTask) => {
                if (previousTask.Status != TaskStatus.RanToCompletion) {
                    Working = false;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Does the analysis
        /// </summary>
        private void StartAnalysisInternal(SynchronizationContext synchronizationContext) {
            // get data
            IPv4InterfaceStatistics interfaceStatistics = GetBandwidthData();

            // create result
            BandwidthAnalizerResult bandwidthAnalizerResult = null;

            //// calculate speed
            //if (_bytesReceived != 0 && _bytesSent != 0) {

            //    //int bytesReceivedSpeed = (int) (_bytesReceived - interfaceStatistics.BytesReceived) / 1024 / 1024;
            //    //int bytesSentSpeed = (int) (_bytesSent - interfaceStatistics.BytesSent) / 1024 / 1024;

            //    // create result
            //    bandwidthAnalizerResult = new BandwidthAnalizerResult(bytesSentSpeed, bytesReceivedSpeed);
            //}
            //else {
            //    // create result
            //    bandwidthAnalizerResult = new BandwidthAnalizerResult();
            //}

            //// remember current values
            //_bytesReceived = interfaceStatistics.BytesReceived;
            //_bytesSent = interfaceStatistics.BytesSent;


            int bytesReceivedSpeed = (int) (interfaceStatistics.BytesReceived) / 1024 / 1024;
            int bytesSentSpeed = (int) (interfaceStatistics.BytesSent) / 1024 / 1024;

            // create result
            bandwidthAnalizerResult = new BandwidthAnalizerResult(bytesSentSpeed, bytesReceivedSpeed);

            // set result
            synchronizationContext.Send((objState) => {
                ResultData = bandwidthAnalizerResult;
            }, synchronizationContext);
        }

        /// <summary>
        /// Method for checking if StartAnalysisCommand can be executed.
        /// </summary>
        private bool CanStartAnalysis() {
            return (_workingTask == null || _workingTask.IsCompleted)
                && (stopingTask == null || stopingTask.IsCompleted)
                && InterfaceSelector.SelectedInterface != null;
        }

        /// <summary>
        /// Starts stopping the analysis
        /// </summary>
        private void StopAnalysis() {
            // nothing to stop
            if (_cancellationTokenSource == null) { return; }
            // trigger cancel
            _cancellationTokenSource.Cancel();
            // start stopping
            stopingTask = Task.Factory.StartNew(() => {
                _workingTask.Wait();
            }).ContinueWith((previousTask) => {
                Working = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Method for checking if StopAnalysisCommand can be executed.
        /// </summary>
        private bool CanStopAnalysis() {
            return (stopingTask == null || stopingTask.IsCompleted);
        }

        /// <summary>
        /// Gets IPv4InterfaceStatistics from the selected interface.
        /// </summary>
        private IPv4InterfaceStatistics GetBandwidthData() {
            // Grab NetworkInterface object that describes the current interface
            NetworkInterface nic = InterfaceSelector.SelectedInterface;

            // Grab the stats for that interface
            IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();
            return interfaceStats;
        }

        /// <summary>
        /// Updates command's can execute by calling OnCanExecuteChanged.
        /// </summary>
        private void UpdateCommands() {
            _startAnalysisCommand.OnCanExecuteChanged();
            _stopAnalysisCommand.OnCanExecuteChanged();
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
    /// Represents the results generated by the analysis
    /// </summary>
    public class BandwidthAnalizerResult {
        /// <summary>
        /// Upload data in user friendly string
        /// </summary>
        public string Upload { get; set; }
        /// <summary>
        /// Download data in user friendly string
        /// </summary>
        public string Download { get; set; }



        /// <summary>
        /// Creates a new instance of the  <see cref="BandwidthAnalizerResult"/> class.
        /// </summary>
        public BandwidthAnalizerResult(double upload, double download) {
            Upload = String.Format("{0} MB", upload);
            Download = String.Format("{0} MB", download);
        }

        /// <summary>
        /// Creates a new instance of the  <see cref="BandwidthAnalizerResult"/> class.
        /// This is used when there is no data.
        /// </summary>
        public BandwidthAnalizerResult() {
            Upload = "N/A";
            Download = "N/A";
        }
    }
}
