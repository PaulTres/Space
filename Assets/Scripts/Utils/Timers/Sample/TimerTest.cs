using Timers;
using UnityEngine;
public class TimerTest : MonoBehaviour
{
    Timer countdown1 = new CountdownTimer(3), countdown2 = new CountdownTimer(5);
    IntervalTimer interval1 = new(float.PositiveInfinity, 1);
    void Start()
    {
        interval1.OnInterval += () => Debug.Log("Interval");
        countdown1.OnTimerStart += () => Debug.Log("Timer1 started.");
        countdown1.OnTimerStop += () => Debug.Log("Timer1 stopped.");
        countdown2.OnTimerStart += () => Debug.Log("Timer2 started.");
        countdown2.OnTimerStop += () => Debug.Log("Timer2 stopped.");
    }
    public bool b;
    protected void Update()
    {
        if (b)
        {
            b = false;
            interval1.Start();
            countdown1.Start();
            countdown2.Start();
        }
    }

}
