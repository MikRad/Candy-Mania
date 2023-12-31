﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class UIViewsController : SingletonMonoBehaviour<UIViewsController>
{
    [Header("Containers")] 
    [SerializeField]private Transform _canvasTransform;

    [Header("Prefabs")] 
    [SerializeField] private MainMenuUIElements _mainMenuElementsPrefab;
    [SerializeField] private LevelButtonsPanel _levelButtonsPanelPrefab;
    [SerializeField] private SettingsPanel _settingsPanelPrefab;
    [SerializeField] private SettingsPanel _settingsPanelMainMenuPrefab;
    [SerializeField] private LevelCompletedPanel _levelCompletedPanelPrefab;
    [SerializeField] private LevelFailedPanel _levelFailedPanelPrefab;
    [SerializeField] private LevelInfoPanel _levelInfoPanelPrefab;

    private readonly Dictionary<UIViewType, UIView> _uiViewsMap = new Dictionary<UIViewType, UIView>();

    private readonly Dictionary<UIEventType, List<Action<object>>> _uiEventsSubscribersMap =
        new Dictionary<UIEventType, List<Action<object>>>();

    protected override void Awake()
    {
        base.Awake();

        CreateUIViewsMap();
    }

    private void OnEnable()
    {
        AddUIViewsHandlers();
    }

    private void OnDisable()
    {
        RemoveUIViewsHandlers();
    }

    public void ShowUIView(UIViewType viewType)
    {
        _uiViewsMap[viewType].Show();
    }

    public void HideUIView(UIViewType viewType)
    {
        _uiViewsMap[viewType].Hide();
    }

    public void AddUIEventSubscriber(UIEventType eventType, Action<object> eventSubscriber)
    {
        if (_uiEventsSubscribersMap.TryGetValue(eventType, out List<Action<object>> subscribers))
        {
            if (!subscribers.Contains(eventSubscriber))
            {
                subscribers.Add(eventSubscriber);
            }

            return;
        }

        _uiEventsSubscribersMap.Add(eventType, new List<Action<object>>() { eventSubscriber });
    }

    public void RemoveUIEventSubscriber(UIEventType eventType, Action<object> eventSubscriber)
    {
        if (_uiEventsSubscribersMap.TryGetValue(eventType, out List<Action<object>> subscribers))
        {
            if (subscribers.Contains(eventSubscriber))
            {
                subscribers.Remove(eventSubscriber);
            }
        }
    }

    public void SetLevelButtonsInitData(int maxReachedLevel, int levelsNumberTotal)
    {
        LevelButtonsPanel lbPanel = _uiViewsMap[UIViewType.LevelButtonsPanel] as LevelButtonsPanel;
        lbPanel?.Init(maxReachedLevel, levelsNumberTotal);
    }

    public void SetLevelCompletedInitData(int levelScore, int totalScore, int maxComboCount, float levelTime)
    {
        LevelCompletedPanel lcPanel = _uiViewsMap[UIViewType.LevelCompletedPanel] as LevelCompletedPanel;
        lcPanel?.Init(levelScore, totalScore, maxComboCount, levelTime);
    }

    public void SetLevelFailedInitData(int levelScore, float levelTime)
    {
        LevelFailedPanel lfPanel = _uiViewsMap[UIViewType.LevelFailedPanel] as LevelFailedPanel;
        lfPanel?.Init(levelScore, levelTime);
    }

    public void SetLevelInfoInitData(IEnumerable<KeyValuePair<VictoryCondition.Type, VictoryCondition>> vicCons, int score, float timeRemained, int possibleMovesNum)
    {
        LevelInfoPanel liPanel = _uiViewsMap[UIViewType.LevelInfoPanel] as LevelInfoPanel;
        liPanel?.Init(vicCons, score, timeRemained, possibleMovesNum);
    }

    public void SetLevelInfoTime(float timeRemained)
    {
        LevelInfoPanel liPanel = _uiViewsMap[UIViewType.LevelInfoPanel] as LevelInfoPanel;
        liPanel?.SetTimeRemained(timeRemained);
    }
    
    public void SetLevelInfoScore(int score)
    {
        LevelInfoPanel liPanel = _uiViewsMap[UIViewType.LevelInfoPanel] as LevelInfoPanel;
        liPanel?.SetScore(score);
    }
    
    public void SetLevelInfoVictoryCondition(VictoryCondition victoryCondition)
    {
        LevelInfoPanel liPanel = _uiViewsMap[UIViewType.LevelInfoPanel] as LevelInfoPanel;
        liPanel?.SetVictoryCondition(victoryCondition);
    }
    
    public void SetLevelInfoPossibleMoves(int possibleMoves)
    {
        LevelInfoPanel liPanel = _uiViewsMap[UIViewType.LevelInfoPanel] as LevelInfoPanel;
        liPanel?.SetPossibleMoves(possibleMoves);
    }
    
    public void SetLevelInfoTimeAlarm()
    {
        LevelInfoPanel liPanel = _uiViewsMap[UIViewType.LevelInfoPanel] as LevelInfoPanel;
        liPanel?.HandleTimeAlarm();
    }
    
    private void AddUIViewsHandlers()
    {
        foreach (KeyValuePair<UIViewType, UIView> mapEntry in _uiViewsMap)
        {
            mapEntry.Value.OnUserEvent += HandleUIViewEvent;
        }
    }
    
    private void RemoveUIViewsHandlers()
    {
        foreach (KeyValuePair<UIViewType, UIView> mapEntry in _uiViewsMap)
        {
            mapEntry.Value.OnUserEvent -= HandleUIViewEvent;
        }
    }

    private void HandleUIViewEvent(UIEventType eventType, object param)
    {
        if (_uiEventsSubscribersMap.TryGetValue(eventType, out List<Action<object>> subscribers))
        {
            NotifySubscribers(subscribers, param);
        }
    }

    private void NotifySubscribers(IEnumerable<Action<object>> subscribers, object param)
    {
        foreach (Action<object> subscriber in subscribers)
        {
            subscriber?.Invoke(param);
        }
    }

    private void CreateUIViewsMap()
    {
        _uiViewsMap.Add(UIViewType.MainMenuElements, Instantiate(_mainMenuElementsPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.LevelButtonsPanel, Instantiate(_levelButtonsPanelPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.SettingsPanel, Instantiate(_settingsPanelPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.SettingsPanelMainMenu, Instantiate(_settingsPanelMainMenuPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.LevelCompletedPanel, Instantiate(_levelCompletedPanelPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.LevelFailedPanel, Instantiate(_levelFailedPanelPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.LevelInfoPanel, Instantiate(_levelInfoPanelPrefab, _canvasTransform));
    }
}
