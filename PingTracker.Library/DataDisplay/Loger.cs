using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingTracker.Library.Ping;

namespace PingTracker.Library.DataDisplay {
    /// <summary>
    /// Holds and manages the log of pings.
    /// </summary>
    public class Loger : INotifyPropertyChanged {

        /// <summary>
        /// Log messages collection.
        /// </summary>
        public ObservableCollection<Message> LogHistory { get; private set; }



        /// <summary>
        /// Creates a new instance of the  <see cref="Loger"/> class.
        /// </summary>
        public Loger() {
            LogHistory = new ObservableCollection<Message>();
        }


        /// <summary>
        /// Logs the pingManipulatorResult. Creates the Message with latency and depending on the status information text.
        /// </summary>
        public void Log(PingManipulatorResult pingManipulatorResult) {
            // local variable for detailed information if there is any
            string detailedInformation = null;

            // get message
            string message = String.Empty;

            // depending on status ...
            if (pingManipulatorResult.PingerData.PingStatus == PingStatus.Success) {

                // generate normal message
                message = String.Format("{0} ms", Math.Round(pingManipulatorResult.Ping, 2));
            }
            else if (pingManipulatorResult.PingerData.PingStatus == PingStatus.Timeout) {
                // generate timeout message
                message = "Timeout";
                detailedInformation = "Timeout just happend.";
            }
            else {
                // generate unknown error message
                message = "Unknown error!";
                detailedInformation = "Unknown error happend. Dunno what happend.";
            }

            // get status
            MessageStatus status = MessageStatus.Normal;

            if (pingManipulatorResult.PingerData.PingStatus == PingStatus.Success) {

                if (pingManipulatorResult.Ping > pingManipulatorResult.Average) {

                    if (pingManipulatorResult.Ping > (pingManipulatorResult.Average * 2)) {
                        status = MessageStatus.Critical;
                        detailedInformation = "Latency is more than twice the average.";
                    }
                    else {
                        status = MessageStatus.Warning;
                        detailedInformation = "Latency is higher than average.";
                    }
                }
            }
            else {
                status = MessageStatus.Critical;
            }

            // make previous not current
            if (LogHistory.Count > 0) {
                LogHistory.First().MarkNotCurrent();
            }

            // log it
            LogHistory.Insert(0, new Message(message, status, detailedInformation));
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
    /// Represents the single row in log.
    /// </summary>
    public class Message : INotifyPropertyChanged {
        private bool _current;
        /// <summary>
        /// Text of the log message. Main value displayed.
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// Message details
        /// </summary>
        public MessageStatusDetails MessageStatusDetails { get; private set; }
        /// <summary>
        /// Flag indicating if message is the most relevant.
        /// </summary>
        public bool Current { get { return _current; } private set { _current = value; OnPropertyChanged("Current"); } }


        /// <summary>
        /// Creates a new instance of the  <see cref="Message"/> class.
        /// </summary>
        public Message(string text, MessageStatus status, string detailedInformation = null) {
            Text = text;
            MessageStatusDetails = new MessageStatusDetails(status, detailedInformation);
            Current = true;
        }

        /// <summary>
        /// Sets the current flag to false.
        /// </summary>
        public void MarkNotCurrent() {
            Current = false;
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
    /// holds the detailed information about the log message.
    /// </summary>
    public class MessageStatusDetails {
        /// <summary>
        /// Message status
        /// </summary>
        public MessageStatus Status { get; private set; }
        /// <summary>
        /// Detailed info
        /// </summary>
        public string DetailedInformation { get; private set; }



        /// <summary>
        /// Creates a new instance of the  <see cref="MessageStatusDetails"/> class.
        /// </summary>
        public MessageStatusDetails(MessageStatus status, string detailedInformation = null) {
            Status = status;
            DetailedInformation = detailedInformation;
        }
    }
    /// <summary>
    /// Available message statuses
    /// </summary>
    public enum MessageStatus {
        /// <summary>
        /// Normal message
        /// </summary>
        Normal,
        /// <summary>
        /// Message is warning about something.
        /// </summary>
        Warning,
        /// <summary>
        /// Message is representing something critical.
        /// </summary>
        Critical
    }
}
