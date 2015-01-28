using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PingTracker.Library.Framework {
    /// <summary>
    /// Implementation of the "never ending task" which executes the action  every pollInterval.
    /// </summary>
    internal static class Repeat {
        public static Task Interval(TimeSpan pollInterval, Action action, CancellationToken token) {
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            // ???
            return Task.Factory.StartNew(
                () => {
                    while (true) {
                        action();

                        if (token.WaitHandle.WaitOne(pollInterval))
                            break;
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
