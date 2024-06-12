using Timer = System.Timers.Timer;
using static System.Console;

    public sealed class LoopTimer{
        private static CancellationTokenSource cT_S = new();
        private  CancellationToken cToken;
        private static readonly Lazy<LoopTimer> _lazy = new(() => new LoopTimer());
        public static LoopTimer LoopTimerInstance { get {return _lazy.Value; } } 
        private static readonly Timer timer = new();
        private static string taskName = string.Empty;
        private LoopTimer() { 
            timer.Enabled = false;
            timer.AutoReset = false;
            cToken = cT_S.Token;
        } 

        public bool TimeLoop(string name, Action action, int timeOutLength = 10_000){

            cT_S = new();
            cToken = cT_S.Token;
            taskName = name;
            timer.Interval = timeOutLength;
            try{
                Task t = Task.Run(() => {
                    timer.Start();
                    action.Invoke();
                } , 
                cToken);

                if (!t.Wait(TimeSpan.FromMilliseconds(timeOutLength)))
                {
                    cT_S.Cancel();
                    cToken.ThrowIfCancellationRequested();
                }
                else 
                {
                    StopTimingLoop();
                    return true;
                }
            }
            catch(Exception ex){
                LoopTimerExceptionhandler(ex);
            } 
            finally {
                cT_S.Dispose();
            }

            return false;
        }

        public void StartTimingLoop(Func<Task> loop, int timeOutLength = 10_000) {

            timer.Interval = timeOutLength;
            timer.Start();
            loop.Invoke();
            cToken.ThrowIfCancellationRequested();

        }

        public void StopTimingLoop() {
            timer.Stop();
        }

        public static void LoopTimerExceptionhandler(Exception ex){
             if (ex is AggregateException)
            {
                AggregateException ae = (AggregateException)ex;
                foreach (Exception e in ae.InnerExceptions) {
                    if (e is TaskCanceledException )
                        WriteLine("Task name {0} timed out.\nMessage: {1}", taskName, ((TaskCanceledException) e).Message);
                    if (e is OperationCanceledException)
                        WriteLine("Task name {0} timed out.\nMessage: {1}", taskName, ((OperationCanceledException) e).Message);
                    else
                        WriteLine("Exception: " + e.GetType().Name);
                }
            }   
            else if (ex is OperationCanceledException)
            {
                    WriteLine("Task name {0} timed out.\nMessage: {1}", taskName, ((OperationCanceledException) ex).Message);
            }
            else if (ex is TaskCanceledException)
            {
                    WriteLine("Task name {0} timed out.\nMessage: {1}", taskName, ((TaskCanceledException) ex).Message);
            }
            else 
            {
                WriteLine("Exception: " + ex.GetType().Name);
                WriteLine(ex.Message);
                WriteLine(ex.StackTrace);                    
            }
        }
    }


