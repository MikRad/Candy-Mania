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
        EventBus.Get.Subscribe<UIEvents.LevelCompletedPanelContinueClicked>(HandleLevelCompletedPanelContinue);
        EventBus.Get.Subscribe<UIEvents.LevelFailedPanelRestartClicked>(HandleLevelFailedPanelRestart);
        EventBus.Get.Subscribe<UIEvents.SettingsPanelContinueClicked>(HandleSettingsPanelContinue);
        EventBus.Get.Subscribe<UIEvents.SettingsPanelToMainMenuClicked>(HandleSettingsPanelToMainMenu);
        EventBus.Get.Subscribe<UIEvents.LevelInfoPanelSettingsClicked>(HandleLevelInfoPanelSettingsClicked);
        EventBus.Get.Subscribe<LevelCompletedEvent>(HandleLevelCompleted);
        EventBus.Get.Subscribe<LevelFailedEvent>(HandleLevelFailed);
        EventBus.Get.Subscribe<AllLevelsCompletedEvent>(HandleAllLevelsCompleted);
    }

    private void RemoveServicesEventHandlers()
    {
        EventBus.Get.Unsubscribe<UIEvents.LevelCompletedPanelContinueClicked>(HandleLevelCompletedPanelContinue);
        EventBus.Get.Unsubscribe<UIEvents.LevelFailedPanelRestartClicked>(HandleLevelFailedPanelRestart);
        EventBus.Get.Unsubscribe<UIEvents.SettingsPanelContinueClicked>(HandleSettingsPanelContinue);
        EventBus.Get.Unsubscribe<UIEvents.SettingsPanelToMainMenuClicked>(HandleSettingsPanelToMainMenu);
        EventBus.Get.Unsubscribe<UIEvents.LevelInfoPanelSettingsClicked>(HandleLevelInfoPanelSettingsClicked);
        EventBus.Get.Unsubscribe<LevelCompletedEvent>(HandleLevelCompleted);
        EventBus.Get.Unsubscribe<LevelFailedEvent>(HandleLevelFailed);
        EventBus.Get.Unsubscribe<AllLevelsCompletedEvent>(HandleAllLevelsCompleted);
    }

    private void HandleLevelInfoPanelSettingsClicked()
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
        
        _uiViewsController.ShowUIView(UIViewType.LevelCompletedPanel);
    }

    private IEnumerator DelayedLevelFailedPanelAppearance()
    {
        yield return new WaitForSeconds(_levelFailedPanelDelay);

        _uiViewsController.HideUIView(UIViewType.LevelInfoPanel);        
        
        _uiViewsController.ShowUIView(UIViewType.LevelFailedPanel);
    }

    private void SetPaused(bool isPaused)
    {
        IsPaused = isPaused;

        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void HandleLevelCompletedPanelContinue()
    {
        _progressController.HandleLevelCompleted();
        
        ScreenFader.Instance.FadeOut().OnCompleted(StartLevel);
    }

    private void HandleLevelFailedPanelRestart()
    {
        _progressController.HandleLevelFailed();
        
        ScreenFader.Instance.FadeOut().OnCompleted(StartLevel);
    }

    private void HandleSettingsPanelContinue()
    {
        SetPaused(false);
        
        _uiViewsController.ShowUIView(UIViewType.LevelInfoPanel);        
    }

    private void HandleSettingsPanelToMainMenu()
    {
        SetPaused(false);
        
        ScreenFader.Instance.FadeOut().OnCompleted(ExitToMainMenu);
    }

    private void ExitToMainMenu()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
