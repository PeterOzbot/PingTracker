using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PingTracker.Library.Graph {
    /// <summary>
    /// Defines the axis of the graph.
    /// </summary>
    public interface IAxis {
        /// <summary>
        /// Generates the numbering depending on the range. The axis range is from 0 to parameter range.
        /// </summary>
        void GenerateNumbering(double range);
        /// <summary>
        /// Axis's points 
        /// </summary>
        IEnumerable<IAxisPoint> Numbering { get; }
    }

    /// <summary>
    /// Base implementation of the IAxis.
    /// </summary>
    public abstract class Axis : IAxis, INotifyPropertyChanged {
        protected IEnumerable<IAxisPoint> _numbering;
        /// <summary>
        /// Axis background color. Used for debugging.
        /// </summary>
        public Brush Background { get { return Brushes.Transparent; } }
        /// <summary>
        /// Axis's points 
        /// </summary>
        public IEnumerable<IAxisPoint> Numbering { get { return _numbering; } set { _numbering = value; OnPropertyChanged("Numbering"); } }
        /// <summary>
        /// How many indicators are on the axis
        /// </summary>
        public abstract int NumberOfSteps { get; }



        /// <summary>
        /// Generates the numbering depending on the range. The axis range is from 0 to parameter range.
        /// </summary>
        public abstract void GenerateNumbering(double range);

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
    /// X Axis implementation. Difference between the Y axis is the order of the points(left-right, bottom-top).
    /// </summary>
    public class XAxis : Axis {
        /// <summary>
        /// How many indicators are on the axis
        /// </summary>
        public override int NumberOfSteps { get { return 8; } }


        /// <summary>
        /// Creates a new instance of the  <see cref="XAxis"/> class.
        /// </summary>
        public XAxis() { }



        /// <summary>
        /// Generates the numbering depending on the range. The axis range is from 0 to parameter range.
        /// </summary>
        public override void GenerateNumbering(double range) {
            List<AxisPoint<string>> numbering = new List<AxisPoint<string>>();

            int roundedRange = Convert.ToInt32(range);

            int step = Convert.ToInt32(range / NumberOfSteps);

            for (int i = 0 ; i < roundedRange ; i += step) {
                numbering.Add(new AxisPoint<string>(CalculateTime(i / 5), Convert.ToDouble(step)));//5 is used to make it macth with data
            }

            Numbering = numbering;
        }

        private string CalculateTime(int i) {
            if (i < 60) {
                return String.Format("{0} sec", i);
            }
            else {
                int min = i / 60;
                int sec = i % 60;
                return String.Format("{0} min {1} sec", min, sec);
            }
        }
    }
    /// <summary>
    /// Y Axis implementation. Difference between the X axis is the order of the points(left-right, bottom-top).
    /// </summary>
    public class YAxis : Axis {
        /// <summary>
        /// How many indicators are on the axis
        /// </summary>
        public override int NumberOfSteps { get { return 15; } }


        /// <summary>
        /// Creates a new instance of the  <see cref="YAxis"/> class.
        /// </summary>
        public YAxis() { }



        /// <summary>
        /// Generates the numbering depending on the range. The axis range is from 0 to parameter range.
        /// </summary>
        public override void GenerateNumbering(double range) {
            List<AxisPoint<int>> numbering = new List<AxisPoint<int>>();

            int step = Convert.ToInt32(range / NumberOfSteps);

            for (int i = 0 ; i < range ; i += step) {
                numbering.Add(new AxisPoint<int>(i, Convert.ToDouble(step)));
            }

            numbering.Reverse();

            Numbering = numbering;
        }
    }


    /// <summary>
    /// Defines the single axis point
    /// </summary>
    public interface IAxisPoint { }


    /// <summary>
    /// Implementation of the IAxisPoint
    /// </summary>
    public class AxisPoint<T> : IAxisPoint {
        /// <summary>
        /// Value displayed.
        /// </summary>
        public T Value { get; protected set; }
        /// <summary>
        /// Points width when drawn.
        /// </summary>
        public double Width { get; protected set; }



        /// <summary>
        /// Creates a new instance of the  <see cref="BatchInfo"/> class.
        /// </summary>
        public AxisPoint(T value, double width) {
            Value = value;
            Width = width;
        }
    }
}
