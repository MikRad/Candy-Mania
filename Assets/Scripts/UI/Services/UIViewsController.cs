using System.Collections.Generic;
using UnityEngine;

public class UIViewsController : SingletonMonoBehaviour<UIViewsController>
{
    [Header("Containers")] 
    [SerializeField]private Transform _canvasTransform;

    [Header("Prefabs")] 
    [SerializeField] private MainMenuButtons _mainMenuButtonsPrefab;
    [SerializeField] private LevelButtonsPanel _levelButtonsPanelPrefab;
    [SerializeField] private SettingsPanel _settingsPanelPrefab;
    [SerializeField] private SettingsPanel _settingsPanelMainMenuPrefab;
    [SerializeField] private LevelCompletedPanel _levelCompletedPanelPrefab;
    [SerializeField] private LevelFailedPanel _levelFailedPanelPrefab;
    [SerializeField] private LevelInfoPanel _levelInfoPanelPrefab;

    private readonly Dictionary<UIViewType, UIView> _uiViewsMap = new Dictionary<UIViewType, UIView>();

    protected override void Awake()
    {
        base.Awake();

        CreateUIViewsMap();
    }

    public void ShowUIView(UIViewType viewType)
    {
        _uiViewsMap[viewType].Show();
    }

    public void HideUIView(UIViewType viewType)
    {
        _uiViewsMap[viewType].Hide();
    }

    public void InitLevelButtonsData(int maxReachedLevel, int levelsNumberTotal)
    {
        LevelButtonsPanel lbPanel = _uiViewsMap[UIViewType.LevelButtonsPanel] as LevelButtonsPanel;
        lbPanel?.Init(maxReachedLevel, levelsNumberTotal);
    }

    public void InitLevelInfoData(IEnumerable<LevelPassCondition> lpConditions, int score, float timeRemained)
    {
        LevelInfoPanel liPanel = _uiViewsMap[UIViewType.LevelInfoPanel] as LevelInfoPanel;
        liPanel?.Init(lpConditions, score, timeRemained);
    }

    public void SetLevelInfoTime(float timeRemained)
    {
        LevelInfoPanel liPanel = _uiViewsMap[UIViewType.LevelInfoPanel] as LevelInfoPanel;
        liPanel?.SetTimeRemained(timeRemained);
    }
    
    private void CreateUIViewsMap()
    {
        _uiViewsMap.Add(UIViewType.MainMenuElements, Instantiate(_mainMenuButtonsPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.LevelButtonsPanel, Instantiate(_levelButtonsPanelPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.SettingsPanel, Instantiate(_settingsPanelPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.SettingsPanelMainMenu, Instantiate(_settingsPanelMainMenuPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.LevelCompletedPanel, Instantiate(_levelCompletedPanelPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.LevelFailedPanel, Instantiate(_levelFailedPanelPrefab, _canvasTransform));
        _uiViewsMap.Add(UIViewType.LevelInfoPanel, Instantiate(_levelInfoPanelPrefab, _canvasTransform));
    }
}
