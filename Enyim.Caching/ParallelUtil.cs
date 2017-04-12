using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enyim.Caching
{
    internal static class ParallelUtil
    {

        public static void Execute(params Action[] actions) => Execute((IEnumerable<Action>)actions);
        public static void Execute(IEnumerable<Action> actions)
        {
            List<Exception> errors = new List<Exception>();
            var promises = new List<Action>();
            using (var e = actions.GetEnumerator())
            {
                if (!e.MoveNext()) return;
                promises.Add(e.Current); // do the first one on this thread
                while (e.MoveNext())
                {
                    promises.Add(GetPromise(e.Current)); // queue the rest
                }
            }
            foreach (var promise in promises)
            {
                try
                {
                    promise();
                }
                catch (Exception e)
                {
                    errors.Add(e);
                }
            }
            if (errors.Count > 0) throw new AggregateException(errors.ToArray());
        }

        private static Action GetPromise(Action action)
        {
            // if a thread pool thread doesn't get to it in time
            // it will run on the invoking thread. that way we
            // won't deadlock if the threadpool gets backed up
            int started = 0;
            var cancelToken = new CancellationTokenSource();
            var t = Task.Run(() =>
            {
                if (Interlocked.Increment(ref started) == 1)
                {
                    action();
                }
            }, cancelToken.Token);
            return () =>
            {
                if (Interlocked.Increment(ref started) == 1)
                {
                    cancelToken.Cancel();
                    action();
                }
                else
                {
                    t.Wait();
                }
            };
        }
    }
}
