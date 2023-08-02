using System;

public class ThrottledAction
{
    private DateTime _lastRunTime;
    private readonly int _intervalMilliseconds;

    public ThrottledAction(int intervalMilliseconds)
    {
        _intervalMilliseconds = intervalMilliseconds;
    }

    public bool CanInvoke => (DateTime.Now - _lastRunTime).TotalMilliseconds >= _intervalMilliseconds;

    public void Invoke(Action action)
    {
        if (!CanInvoke) return;

        action.Invoke();

        _lastRunTime = DateTime.Now;
    }
}