using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Common_Tasks
{
    public static class ProcessExtensions
    {
        public static Task<bool> WaitForExitAsync(this Process process, int millisecondsTimeout = -1)
        {
            var tcs = new TaskCompletionSource<bool>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(true);
            if (millisecondsTimeout > 0)
            {
                new System.Threading.Timer(state => tcs.TrySetResult(false), null, millisecondsTimeout, Timeout.Infinite);
            }

            return tcs.Task;
        }
    }
}
