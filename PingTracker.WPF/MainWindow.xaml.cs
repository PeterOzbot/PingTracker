using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PingTracker.WPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private PingTrackerViewModel _pingTrackerViewModel;
        private Task _stoppingTask;



        /// <summary>
        /// Creates a new instance of the  <see cref="MainWindow"/> window.
        /// </summary>
        public MainWindow() {
            InitializeComponent();

            DataContext = _pingTrackerViewModel = new PingTrackerViewModel(Dispatcher, new ScrollViewerTrainer(GraphScrollViewer));

            polyline.Points = _pingTrackerViewModel.Graph.CurrentPing.Points;
            average.Points = _pingTrackerViewModel.Graph.AveragePing.Points;
        }

        /// <summary>
        /// Used to enable moving the border less window.
        /// </summary>
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        /// <summary>
        /// Custom implementation of closing the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e) {
            if (_stoppingTask == null) {
                _stoppingTask = _pingTrackerViewModel.Stop();

                _stoppingTask.ContinueWith((previousTask) => {
                    Dispatcher.Invoke(new Action(() => {
                        this.Close();
                    }));
                });
            }

        }

        /// <summary>
        /// Implementation of the IScrollViewerTrainer.
        /// </summary>
        public class ScrollViewerTrainer : IScrollViewerTrainer {
            private ScrollViewer _scrollViewer;


            /// <summary>
            /// Creates a new instance of the  <see cref="ScrollViewerTrainer"/> class.
            /// </summary>
            public ScrollViewerTrainer(ScrollViewer scrollViewer) {
                _scrollViewer = scrollViewer;
            }


            /// <summary>
            /// Scroll the scroll bar to center.
            /// </summary>
            public void ScrollCenter() {
                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.ScrollableWidth / 2);
            }
        }
    }
}
