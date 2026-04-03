using UnityEngine.Events;

public class Timer
{
    float timestamp = 0;
    public float Timestamp { get { return timestamp; } }
    bool paused = false;
    public bool currentResponse { get; private set; }
    public bool Paused
    {
        get { return paused; }
        set { paused = value;}
    }
    public UnityEvent timedEvent = new UnityEvent();
    public void SetTimer(float time)
    {
        timestamp = time;
    }
    public bool timer(float time, float timeIncrement, bool defautResponse, bool autoReset)
    {
        if (!Paused)
        {
            timestamp += timeIncrement;
        }
        if (timestamp >= time)
        {
            if (autoReset)
            {
                SetTimer(0);
            }
            if (!defautResponse && !paused)
            {
                timedEvent.Invoke();
            }
            currentResponse = !defautResponse;
            return !defautResponse;
        }
        else
        {
            if (defautResponse)
            {
                timedEvent.Invoke();
            }
            currentResponse = defautResponse;
            return defautResponse;
        }
    }
    public bool timer(float time, float timeIncrement, bool defautResponse, float TimeBeforeReset)
    {
        if (!Paused)
        {
            timestamp += timeIncrement;
        }
        if (timestamp >= time)
        {
            if (timestamp >= timeIncrement+TimeBeforeReset)
            {
                SetTimer(0);
            }
            if (!defautResponse && !paused)
            {
                timedEvent.Invoke();
            }
            return !defautResponse;
        }
        else
        {
            if (defautResponse)
            {
                timedEvent.Invoke();
            }
            return defautResponse;
        }
    }
}
