using System;
using System.Collections.Generic;
using UnityEngine;

public class GameProgressController
{
    private const string GameProgressStringKey = "CandyManiaGameProgressData";

    private static GameProgressController _instance;
    private LevelsScoresData _levelsScoresData;

    private readonly Dictionary<int, int> _scoresMap = new Dictionary<int, int>();
    
    public int CurrentLevelNumber { get; private set; } = 1;
    public int MaxReachedLevelNumber { get; private set; } = 1;
    public int LevelsNumberTotal { get; private set; } = 1;

    public int CurrentLevelScore { get; private set; }
    public int TotalScore { get; private set; }
    
    public static GameProgressController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameProgressController();
            }
            
            return _instance;
        }
    }

    private GameProgressController()
    {
        Init();
    }

    public void SetStartLevel(int levelNumber)
    {
        CurrentLevelNumber = levelNumber;
    }

    public void SetTotalLevelsNumber(int levelNumberTotal)
    {
        LevelsNumberTotal = levelNumberTotal;
    }
    
    public void AddScore(Cell cell)
    {
        if(_scoresMap.TryGetValue(cell.CausedDetonationsNumber, out int score))
        {
            AudioController.Instance.PlaySfx(SfxType.ScoreAdd);

            CurrentLevelScore += score;
            TotalScore += score;
            
            EventBus.Get.RaiseEvent(this, new ScoreChangedEvent(CurrentLevelScore, TotalScore));
            
            VfxController.Instance.AddFlyingScoreVfx(cell.CachedTransform.position, Quaternion.identity) 
                .SetText($"+{score}", FlyingMessage.MessageType.Positive);
        }
    }
    
    public void HandleLevelCompleted()
    {
        if(CurrentLevelNumber > _levelsScoresData._levelScores.Count)
        {
            _levelsScoresData._levelScores.Add(TotalScore);
        }
        else if (TotalScore > _levelsScoresData._levelScores[CurrentLevelNumber - 1])
        {
            _levelsScoresData._levelScores[CurrentLevelNumber - 1] = TotalScore;
        }

        string scoresDataStr = JsonUtility.ToJson(_levelsScoresData);
        PlayerPrefs.SetString(GameProgressStringKey, scoresDataStr);

        if (CurrentLevelNumber == LevelsNumberTotal)
        {
            EventBus.Get.RaiseEvent(this, new AllLevelsCompletedEvent());
        }
        else
        {
            CurrentLevelNumber++;
            MaxReachedLevelNumber = CurrentLevelNumber;
        }

        CurrentLevelScore = 0;
    }

    public void HandleLevelFailed()
    {
        TotalScore -= CurrentLevelScore;
        CurrentLevelScore = 0;
    }

    private void Init()
    {
        string progressStringValue = PlayerPrefs.GetString(GameProgressStringKey);
        
        if(progressStringValue.Length == 0)
        {
            _levelsScoresData = new LevelsScoresData();
        }
        else
        {
            _levelsScoresData = JsonUtility.FromJson(progressStringValue, typeof(LevelsScoresData)) as LevelsScoresData;
        }

        MaxReachedLevelNumber = (_levelsScoresData != null) ? _levelsScoresData._levelScores.Count + 1 : 1;
        CurrentLevelScore = GetStartScore();
        
        FillScoresMap();
    }
    
    private int GetStartScore()
    {
        return (CurrentLevelNumber == 1) ? 0 : _levelsScoresData._levelScores[CurrentLevelNumber - 2];
    }
    
    private void FillScoresMap()
    {
        _scoresMap.Add(3, 25);
        _scoresMap.Add(4, 50);
        _scoresMap.Add(5, 100);
        _scoresMap.Add(6, 250);
        _scoresMap.Add(7, 500);
        _scoresMap.Add(8, 1000);
    }
}