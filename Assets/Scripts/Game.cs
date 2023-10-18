using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [Header("App settings")]
    [SerializeField] private AppSettings _settings;

    [Header("UI Panels appearance delays")]
    [SerializeField] private float _levelCompletedPanelDelay = 2f;
    [SerializeField] private float _levelFailedPanelDelay = 2f;

    private UIViewsController _uiViewsController;
    private LevelController _levelController;
    private GameProgressController _progressController;

    public static Game Instance { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsLevelFinished { get; private set; }

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        _levelController = GetComponentInChildren<LevelController>();
        
        _progressController = GameProgressController.Instance;
        _progressController.SetTotalLevelsNumber(_settings._levelsNumberTotal);
        _levelController.Init(_settings, _progressController);
        _uiViewsController = UIViewsController.Instance;

        AddServicesEventHandlers();
        
        StartLevel();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            RemoveServicesEventHandlers();
            Instance = null;
        }
    }

    private void StartLevel()
    {
        _levelController.InitLevel(_progressController.CurrentLevelNumber);
        IsLevelFinished = false;
        
        _uiViewsController.ShowUIView(UIViewType.LevelInfoPanel);
        
        ScreenFader.Instance.FadeIn();
    }

    private void AddServicesEventHandlers()
    {
        _uiViewsController.AddUIEventSubscriber(UIEventType.LevelCompletedContinueClick, HandleLevelCompletedPanelContinueClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.LevelFailedRestartClick, HandleLevelFailedPanelRestartClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.SettingsContinueClick, HandleSettingsPanelContinueClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.SettingsMainMenuClick, HandleSettingsPanelMainMenuClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.LevelInfoSettingsClick, HandleSettingsButtonClick);
        
        _levelController.OnLevelCompleted += HandleLevelCompleted;
        _levelController.OnLevelFailed += HandleLevelFailed;
        _progressController.OnAllLevelsCompleted += HandleAllLevelsCompleted;
    }

    private void RemoveServicesEventHandlers()
    {
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.LevelCompletedContinueClick, HandleLevelCompletedPanelContinueClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.LevelFailedRestartClick, HandleLevelFailedPanelRestartClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.LevelInfoSettingsClick, HandleSettingsButtonClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.SettingsContinueClick, HandleSettingsPanelContinueClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.SettingsMainMenuClick, HandleSettingsPanelMainMenuClick);
        
        _levelController.OnLevelCompleted -= HandleLevelCompleted;
        _levelController.OnLevelFailed -= HandleLevelFailed;
        _progressController.OnAllLevelsCompleted -= HandleAllLevelsCompleted;
    }

    private void HandleSettingsButtonClick(object param)
    {
        SetPaused(true);

        _uiViewsController.HideUIView(UIViewType.LevelInfoPanel);
        _uiViewsController.ShowUIView(UIViewType.SettingsPanel);
    }

    private void HandleLevelCompleted()
    {
        IsLevelFinished = true;

        StartCoroutine(DelayedLevelCompletedPanelAppearance());
    }

    private void HandleLevelFailed()
    {
        IsLevelFinished = true;

        StartCoroutine(DelayedLevelFailedPanelAppearance());
    }

    private void HandleAllLevelsCompleted()
    {
    }
        
    private IEnumerator DelayedLevelCompletedPanelAppearance()
    {
        yield return new WaitForSeconds(_levelCompletedPanelDelay);

        _uiViewsController.HideUIView(UIViewType.LevelInfoPanel);
        
        _uiViewsController.SetLevelCompletedInitData(_progressController.CurrentLevelScore, _progressController.TotalScore, _levelController.MaxComboCount, _levelController.LevelTimePlayed);
        _uiViewsController.ShowUIView(UIViewType.LevelCompletedPanel);
    }

    private IEnumerator DelayedLevelFailedPanelAppearance()
    {
        yield return new WaitForSeconds(_levelFailedPanelDelay);

        _uiViewsController.HideUIView(UIViewType.LevelInfoPanel);        
        
        _uiViewsController.SetLevelFailedInitData(_progressController.CurrentLevelScore, _levelController.LevelTimePlayed);
        _uiViewsController.ShowUIView(UIViewType.LevelFailedPanel);
    }

    private void SetPaused(bool isPaused)
    {
        IsPaused = isPaused;

        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void HandleLevelCompletedPanelContinueClick(object param)
    {
        _progressController.HandleLevelCompleted();
        
        ScreenFader.Instance.FadeOut().OnCompleted(StartLevel);
    }

    private void HandleLevelFailedPanelRestartClick(object param)
    {
        _progressController.HandleLevelFailed();
        
        ScreenFader.Instance.FadeOut().OnCompleted(StartLevel);
    }

    private void HandleSettingsPanelContinueClick(object param)
    {
        SetPaused(false);
        
        _uiViewsController.ShowUIView(UIViewType.LevelInfoPanel);        
    }

    private void HandleSettingsPanelMainMenuClick(object param)
    {
        SetPaused(false);
        
        ScreenFader.Instance.FadeOut().OnCompleted(ExitToMainMenu);
    }

    private void ExitToMainMenu()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
