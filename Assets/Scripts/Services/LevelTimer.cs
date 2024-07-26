using UnityEngine;

public class LevelTimer
{
    private float _timeLimit;
    private bool _isTimeExpiring;
    private readonly float _alarmValue;
    
    public float TimeRemained { get; private set; }
    public float TimePlayed => (_timeLimit - TimeRemained);

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
            EventBus.Get.RaiseEvent(this, new LevelTimeExpiringEvent());
        }
        if (TimeRemained <= 0)
        {
            EventBus.Get.RaiseEvent(this, new LevelTimeExpiredEvent());
        }
    }
}
