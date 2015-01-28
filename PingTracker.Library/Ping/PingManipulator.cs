using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingTracker.Library.Ping {
    /// <summary>
    /// Manipulates the raw pinger data in usable data.
    /// </summary>
    public class PingManipulator {
        private Queue<double> _pingQueue = new Queue<double>();

        /// <summary>
        /// Creates a new instance of the  <see cref="PingManipulator"/> class.
        /// </summary>
        public PingManipulator() { }



        /// <summary>
        /// Manipulates the raw data into PingManipulatorResult.
        /// Calculates latency, average latency, time between pings.
        /// </summary>
        public PingManipulatorResult Calculate(PingerData pingerData) {

            // get ping
            double manipulatedPing = CalculatePing(pingerData);

            // handle average calculation
            double averagePing = ClaculateAverage(manipulatedPing, pingerData);

            // calculate time
            double time = CalculateTime(pingerData.Time);

            // return result
            return new PingManipulatorResult(pingerData, manipulatedPing, averagePing, time);
        }

        /// <summary>
        /// Clears the latency history
        /// </summary>
        public void Reset() {
            _pingQueue = new Queue<double>();
        }

        /// <summary>
        /// Calculates the average from the latency queue and handle the queue size -> number of pings from which the average is calculated.
        /// </summary>
        private double ClaculateAverage(double manipulatedPing, PingerData pingerData) {
            // put value in queue, if not in queue its not used
            if (pingerData.PingStatus == PingStatus.Success) {
                _pingQueue.Enqueue(manipulatedPing);
            }

            // initialize sum and count counters
            double sum = 0;
            int count = 0;

            // sum all values and count them
            _pingQueue.ToList().ForEach(new Action<double>((value) => { sum += value; count++; }));

            // calculate average
            double averagePing = sum / count;

            // dequeue old values if there is more than 50 stored
            if (_pingQueue.Count > 50) {
                _pingQueue.Dequeue();
            }


            // return result
            return averagePing;
        }

        /// <summary>
        /// If pinger data is success just converts the value if not returns the last successful.
        /// </summary>
        private double CalculatePing(PingerData pingerData) {
            if (pingerData.PingStatus == PingStatus.Success) {
                return Convert.ToDouble(pingerData.Ping);
            }
            else {
                return _pingQueue.Peek();
            }
        }

        /// <summary>
        /// Modifies the time to look better in the graph.
        /// </summary>
        private double CalculateTime(TimeSpan timePassed) {
            // return seconds
            return Convert.ToDouble(timePassed.TotalSeconds * 5);// 5 is used to make graph wider
        }
    }

    /// <summary>
    ///Holds the manipulated pinger data.
    /// </summary>
    public class PingManipulatorResult {
        public PingerData PingerData { get; private set; }
        /// <summary>
        /// Latency value.
        /// </summary>
        public double Ping { get; private set; }
        /// <summary>
        /// Average value.
        /// </summary>
        public double Average { get; private set; }
        /// <summary>
        /// Time of the latency -> its modified to look great on graph.
        /// </summary>
        public double Time { get; private set; }



        /// <summary>
        /// Creates a new instance of the  <see cref="PingManipulatorResult"/> class.
        /// </summary>
        public PingManipulatorResult(PingerData pingerData, double ping, double average, double time) {
            PingerData = pingerData;
            Ping = ping;
            Average = average;
            Time = time;
        }
    }
}
