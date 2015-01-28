using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using PingTracker.Library.Framework;

namespace PingTracker.Library.Ping {
    /// <summary>
    /// Used to get the latency in intervals and other latency logic.
    /// </summary>
    public class Pinger : INotifyPropertyChanged {
        private Task _pingerTask;
        //private Task _pausingTask;
        //private Task _startingTask;
        private Task _managingTask;
        private DateTime _startTime;
        private System.Net.NetworkInformation.Ping _p;
        private CancellationTokenSource _cancellationTokenSource;
        private string _url;
        private ICommand _pauseCommand;

        private RunningStatus _runningStatus;

        /// <summary>
        /// Status what pinger is doing.
        /// </summary>
        public RunningStatus RunningStatus { get { return _runningStatus; } set { _runningStatus = value; OnPropertyChanged("RunningStatus"); } }
        /// <summary>
        /// Command to pause the pinger work.
        /// </summary>
        public ICommand PauseCommand { get { return _pauseCommand; } }



        /// <summary>
        /// Creates a new instance of the  <see cref="Pinger"/> class.
        /// </summary>
        public Pinger() {
            _p = new System.Net.NetworkInformation.Ping();
            _pauseCommand = new Command(Pause, null);
        }


        
        /// <summary>
        /// Starts the pinging. If pinging is in progress first stops.
        /// </summary>
        public void Start(string url) {
            // if starting is already in a effect do nothing
            if (_managingTask != null && !_managingTask.IsCompleted) { return; }

            _managingTask = Task.Factory.StartNew(() => {
                Stop().Wait();
            }).ContinueWith((previousTask) => {

                _url = url;
                _startTime = DateTime.UtcNow;
                RunningStatus = RunningStatus.Running;

                StartInternal();

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        /// <summary>
        /// Stops the pinging.
        /// </summary>
        public Task Stop() {
            return Task.Run(() => {
                if (_cancellationTokenSource != null) {
                    _cancellationTokenSource.Cancel();
                }
                if (_pingerTask != null) {
                    _pingerTask.Wait();
                }
            });
        }

        /// <summary>
        /// Pauses the pinging.
        /// </summary>
        private void Pause() {
            // if pause is already in a effect do nothing
            if (_managingTask != null && !_managingTask.IsCompleted) { return; }

            // if it si allready paused just start
            if (RunningStatus == Library.Ping.RunningStatus.Paused) {
                // set flag
                RunningStatus = Library.Ping.RunningStatus.Running;

                StartInternal();
            }
            // pause the pinger
            else {

                // set flag
                RunningStatus = Library.Ping.RunningStatus.Paused;

                _managingTask = Task.Factory.StartNew(() => { Stop().Wait(); });
            }
        }

        /// <summary>
        /// Internal start. Prepares interval action and last task.
        /// </summary>
        private void StartInternal() {
            // create cancellation token
            _cancellationTokenSource = new CancellationTokenSource();
            // start ping in interval
            _pingerTask = Repeat.Interval(TimeSpan.FromSeconds(1), Ping, _cancellationTokenSource.Token);
            // continue with task setting falg to IsRunning false to know when task stoped
            _pingerTask = _pingerTask.ContinueWith((previousTask) => {
                if (previousTask.Status == TaskStatus.RanToCompletion) {
                    RunningStatus = RunningStatus.Paused;
                }
                else { RunningStatus = RunningStatus.StopedFaulted; }
            });
        }

        /// <summary>
        /// Gets latency data.
        /// </summary>
        private void Ping() {
            // start pinging
            PingReply pingReply = _p.Send(_url);

            // get ping
            long ping = pingReply.RoundtripTime;

            // calculate time passed
            TimeSpan timeDifference = DateTime.UtcNow - _startTime;

            // create pinger data
            PingerData pingerData = null;

            switch (pingReply.Status) {
                case IPStatus.Success:
                    pingerData = PingerData.CreateSuccess(ping, timeDifference);
                    break;
                case IPStatus.TimedOut:
                default:
                    pingerData = PingerData.CreateTimeout(timeDifference);
                    break;

            }

            // notify
            OnNewPing(pingerData);
        }


        /// <summary>
        /// When new ping is calculated this event fires.
        /// </summary>
        public event EventHandler<PingerData> NewPing;
        /// <summary>
        /// URLChangedOnNewPing executing helper method.
        /// </summary>
        public void OnNewPing(PingerData pingerData) {
            if (NewPing != null) {
                NewPing(this, pingerData);
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

    /// <summary>
    /// Represent the raw ping data.
    /// </summary>
    public class PingerData {
        /// <summary>
        /// Current ping.
        /// </summary>
        public long Ping { get; private set; }
        /// <summary>
        /// Time from previous ping.
        /// </summary>
        public TimeSpan Time { get; private set; }
        /// <summary>
        /// Status of the ping(timeout, success, ...)
        /// </summary>
        public PingStatus PingStatus { get; private set; }


        /// <summary>
        /// Creates a new instance of the  <see cref="PingerData"/> class.
        /// </summary>
        public PingerData(long ping, TimeSpan time) {
            Ping = ping;
            Time = time;
        }


        /// <summary>
        /// Creates the PingerData when pinging is a success.
        /// </summary>
        public static PingerData CreateSuccess(long ping, TimeSpan time) {
            return new PingerData(ping, time) { PingStatus = PingStatus.Success };
        }
        /// <summary>
        /// Creates the PingerData when pinging time outs.
        /// </summary>
        public static PingerData CreateTimeout(TimeSpan time) {
            return new PingerData(0, time) { PingStatus = PingStatus.Timeout };
        }
        /// <summary>
        /// Creates the PingerData when pinging fails.
        /// </summary>
        public static PingerData CreateUnknownError(TimeSpan time) {
            return new PingerData(0, time) { PingStatus = PingStatus.UnknownError };
        }
    }

    /// <summary>
    /// Available ping statuses
    /// </summary>
    public enum PingStatus {
        /// <summary>
        /// Pinging was a success.
        /// </summary>
        Success,
        /// <summary>
        /// Timeout happened.
        /// </summary>
        Timeout,
        /// <summary>
        /// Pinging failed.
        /// </summary>
        UnknownError
    }


    /// <summary>
    /// Defines available Pinger states.
    /// </summary>
    public enum RunningStatus {
        /// <summary>
        /// Pinging failed.
        /// </summary>
        StopedFaulted,
        /// <summary>
        /// Pinging was paused by the user.
        /// </summary>
        Paused,
        /// <summary>
        /// Pinging is running normaly.
        /// </summary>
        Running
    }
}
