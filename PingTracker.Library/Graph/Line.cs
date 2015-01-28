using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PingTracker.Library.Graph {
    /// <summary>
    /// Represents the line of points in graph.
    /// </summary>
    public class Line : INotifyPropertyChanged {
        /// <summary>
        /// Points constructing this line
        /// </summary>
        public PointCollection Points { get; set; }


        /// <summary>
        /// Creates a new instance of the  <see cref="Line"/> class.
        /// </summary>
        public Line() {
            Points = new PointCollection();
        }


        /// <summary>
        /// Adds point to the collection which draws it.
        /// </summary>
        public void DrawPoint(double x, double y) {
            Points.Add(new Point(x, y));
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
