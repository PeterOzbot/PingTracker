using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using PingTracker.Library.Ping;

namespace PingTracker.Library.Graph {
    /// <summary>
    /// Holds the data needed to draw graph.
    /// </summary>
    public class Graph : INotifyPropertyChanged {
        private double _width;

        /// <summary>
        /// The X Axis
        /// </summary>
        public IAxis X_Axis { get; set; }
        /// <summary>
        /// The Y Axis
        /// </summary>
        public IAxis Y_Axis { get; set; }
        /// <summary>
        ///Line for the current latency
        /// </summary>
        public Line CurrentPing { get; set; }
        /// <summary>
        /// Line for the average latency.
        /// </summary>
        public Line AveragePing { get; set; }
        /// <summary>
        /// Graph height
        /// </summary>
        public double Height { get; set; }
        /// <summary>
        /// Graph width
        /// </summary>
        public double Width { get { return _width; } set { _width = value; OnPropertyChanged("Width"); } }


        /// <summary>
        /// Creates a new instance of the  <see cref="Graph"/> class.
        /// </summary>
        public Graph() {
            X_Axis = new XAxis();
            Y_Axis = new YAxis();
            CurrentPing = new Line();
            AveragePing = new Line();

            Height = 500;
            Width = 800;

            X_Axis.GenerateNumbering(Width);
            Y_Axis.GenerateNumbering(Height);

            InitPoints();
        }


        /// <summary>
        /// Draws the points with the pingManipulatorResult data.
        /// </summary>
        public void Draw(PingManipulatorResult pingManipulatorResult) {
            AveragePing.DrawPoint(pingManipulatorResult.Time, Height - pingManipulatorResult.Ping);
            CurrentPing.DrawPoint(pingManipulatorResult.Time, Height - pingManipulatorResult.Average);
            Expand(pingManipulatorResult.Time);
        }

        /// <summary>
        /// Clears the lines and creates startup conditions.
        /// </summary>
        public void Reset() {
            Width = 800;
            X_Axis.GenerateNumbering(Width);
            InitPoints();
        }

        /// <summary>
        /// Resizes the graph if needed.
        /// </summary>
        private void Expand(double time) {
            if (time > (Width / 2)) {
                Width = Width + 1000;
                X_Axis.GenerateNumbering(Width);
            }
        }

        /// <summary>
        /// Initializes collection for points.
        /// </summary>
        private void InitPoints() {
            CurrentPing.Points.Clear();
            AveragePing.Points.Clear();
            CurrentPing.DrawPoint(0, Height);
            AveragePing.DrawPoint(0, Height);
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
