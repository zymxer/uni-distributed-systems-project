using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    private float _value;
    private bool _isActive;
    private bool _isContinuous;
    private UnityEvent _onValueChanged = new UnityEvent();
    private UnityEvent _onEnd = new UnityEvent();

    private float _startValue;
    private float _prevoiosValue;
    private float _delta;
    private bool _toDelete = false;

    private void Start()
    {
        _startValue = _value;
        _prevoiosValue = _value;
        TimersController.GetController().AddTimer(this);
    }

    //active = false, continuous = false
    public void SetTimer(float timerValue)
    {
        _value = timerValue;
        _isActive = false;
        _isContinuous = false;
    }
    public void SetTimer(float timerValue, bool timerActive, bool timerContinuous)
    {
        _value = timerValue;
        _isActive = timerActive;
        _isContinuous = timerContinuous;
    }

    public float GetValue()
    {
        return _value;
    }

    public float TimePast()
    {
        return Mathf.Max(_startValue - _value, 0.0f);
    }

    public float TimePastPercent()
    {
        return (_startValue - _value) / _startValue;
    }

    public float GetDelta()
    {
        return _delta;
    }

    public void UpdateDelta()
    {
        _delta = _prevoiosValue - _value;
        _delta = Mathf.Max(_delta, 0.0f);
        _prevoiosValue = _value;
    }
    
    public UnityEvent OnValueChanged()
    {
        return _onValueChanged;
    }
    public UnityEvent OnEnd()
    {
        return _onEnd;
    }

    public void SetValue(float newValue)
    {
        _value = newValue;
    }

    public bool IsActive()
    {
        return _isActive;
    }
    
    public void SetActive(bool isActive)
    {
        this._isActive = isActive;
    }

    public bool IsContinuous()
    {
        return _isContinuous;
    }

    public void SetContinuous(bool isContinuous)
    {
        this._isContinuous = isContinuous;
    }

    public void ResetValue(float newValue)
    {
        _startValue = newValue;
        _value = newValue;
        _prevoiosValue = newValue;
    }
    
    public void ResetTimer()
    {
        _value = _startValue;
        _prevoiosValue = _startValue;
        _isActive = false;
    }

    public void Restart()
    {
        _value = _startValue;
        _prevoiosValue = _startValue;
        _isActive = true;
    }
    
    public void Stop()
    {
        _isActive = false;
    }

    public void End()
    {
        _value = -1f;
    }

    public void Continue() //Doesn't invoke OnStart
    {
        _isActive = true;
    }

    public void Activate() //Invokes OnStart
    {
        _isActive = true;
    }


    public void Remove()
    {
        _toDelete = true;
    }

    public bool ToDelete()
    {
        return _toDelete;
    }

}
