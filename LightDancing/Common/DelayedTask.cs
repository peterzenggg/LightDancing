using System;
using System.Threading;
using System.Threading.Tasks;

public class DelayedTask
{
    private readonly Action _action;
    private readonly TimeSpan _delay;
    private CancellationTokenSource _cts;

    public DelayedTask(Action action, TimeSpan delay)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _delay = delay;
        _cts = new CancellationTokenSource();
    }

    public void Restart()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();

        Task.Delay(_delay, _cts.Token).ContinueWith((t) =>
        {
            if (!t.IsCanceled)
            {
                _action();
            }
        });
    }
}