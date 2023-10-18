using System;
using UnityEngine;

public class LevelTimer
{
    private float _timeLimit;
    private bool _isTimeExpiring;
    private readonly float _alarmValue;
    
    public float TimeRemained { get; private set; }
    public float TimePlayed => (_timeLimit - TimeRemained);

    public event Action OnTimeExpiring;
    public event Action OnTimeExpired;

    public LevelTimer(float alarmValue)
    {
        _alarmValue = alarmValue;
    }
    public void Init(float levelTimeLimit)
    {
        _isTimeExpiring = false;
        _timeLimit = levelTimeLimit;
        TimeRemained = levelTimeLimit;
    }

    public void Tick()
    {
        TimeRemained -= Time.deltaTime;
        TimeRemained = Mathf.Max(TimeRemained, 0);

        if (!_isTimeExpiring && (TimeRemained <= _alarmValue))
        {
            _isTimeExpiring = true;
            OnTimeExpiring?.Invoke();
        }
        if (TimeRemained <= 0)
        {
            OnTimeExpired?.Invoke();
        }
    }
}
