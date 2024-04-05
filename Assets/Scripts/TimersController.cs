using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TimersController : MonoBehaviour
{
    [SerializeField]
    private int amount;
    private readonly List<Timer> _timersList = new List<Timer>();
    private static TimersController _instance = null;

    private Timer _currentTimer;


    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
    }

    public void AddTimer(Timer timer)
    {
        _timersList.Add(timer);
    }

    public void RemoveTimer(Timer timer) 
    {
        timer.OnValueChanged().RemoveAllListeners();
        timer.OnEnd().RemoveAllListeners();
        _timersList.Remove(timer);
    }
    
    void Update()
    {
        amount = _timersList.Count;
        UpdateTimers();
    }

    private void UpdateTimers()
    {
        for (int i = _timersList.Count - 1; i >= 0; i--)
        {
            _currentTimer = _timersList[i];
            UpdateTimer(_currentTimer);
        }
    }

    private void UpdateTimer(Timer timer)
    {
        if(timer.ToDelete())
        {
            RemoveTimer(timer);
        }
        else if (timer.IsActive())
        {
            timer.SetValue(timer.GetValue() - Time.deltaTime);
            timer.UpdateDelta();
            if (timer.GetValue() <= 0f)
            {
                timer.OnEnd().Invoke();
                if (timer.IsContinuous())
                {
                    timer.Restart();
                }
                else
                {
                    timer.ResetTimer();   
                }
            }
            timer.OnValueChanged().Invoke();
        }
    }

    public static TimersController GetController()
    {
        return _instance;
    }
    
}
