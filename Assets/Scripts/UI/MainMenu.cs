using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("App settings")]
    [SerializeField] private AppSettings _settings;

    private GameProgressController _progressController;
    private UIViewsController _uiViewsController;

    private void Start()
    {
        Init();
        AddUIEventsHandlers();

        ScreenFader.Instance.FadeIn().OnCompleted(() =>
        {
            _uiViewsController.ShowUIView(UIViewType.MainMenuElements);
        });
    }

    private void OnDestroy()
    {
        RemoveUIEventsHandlers();
    }

    private void Init()
    {
        _progressController = GameProgressController.Instance;
        _progressController.SetTotalLevelsNumber(_settings._levelsNumberTotal);
        _uiViewsController = UIViewsController.Instance;
        _uiViewsController.InitLevelButtonsData(_progressController.MaxReachedLevelNumber, _progressController.LevelsNumberTotal);
    }

    private void AddUIEventsHandlers()
    {
        EventBus.Get.Subscribe<UIEvents.MainMenuPlayClicked>(HandlePlayButtonClicked);
        EventBus.Get.Subscribe<UIEvents.MainMenuSettingsClicked>(HandleSettingsButtonClicked);
        EventBus.Get.Subscribe<UIEvents.MainMenuExitClicked>(HandleExitButtonClicked);
        EventBus.Get.Subscribe<UIEvents.LevelButtonsPanelBackClicked>(HandleLevelButtonsPanelBackClicked);
        EventBus.Get.Subscribe<UIEvents.LevelButtonsPanelLevelSelected>(HandleLevelSelected);
        EventBus.Get.Subscribe<UIEvents.SettingsPanelContinueClicked>(HandleSettingsPanelContinueClicked);
    }
    
    private void RemoveUIEventsHandlers()
    {
        EventBus.Get.Unsubscribe<UIEvents.MainMenuPlayClicked>(HandlePlayButtonClicked);
        EventBus.Get.Unsubscribe<UIEvents.MainMenuSettingsClicked>(HandleSettingsButtonClicked);
        EventBus.Get.Unsubscribe<UIEvents.MainMenuExitClicked>(HandleExitButtonClicked);
        EventBus.Get.Unsubscribe<UIEvents.LevelButtonsPanelBackClicked>(HandleLevelButtonsPanelBackClicked);
        EventBus.Get.Unsubscribe<UIEvents.LevelButtonsPanelLevelSelected>(HandleLevelSelected);
        EventBus.Get.Unsubscribe<UIEvents.SettingsPanelContinueClicked>(HandleSettingsPanelContinueClicked);
    }
    
    private void HandlePlayButtonClicked()
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        _uiViewsController.HideUIView(UIViewType.MainMenuElements);
        _uiViewsController.ShowUIView(UIViewType.LevelButtonsPanel);
    }

    private void HandleSettingsButtonClicked()
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        _uiViewsController.HideUIView(UIViewType.MainMenuElements);
        _uiViewsController.ShowUIView(UIViewType.SettingsPanelMainMenu);
    }

    private void HandleExitButtonClicked()
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);
        
        _uiViewsController.HideUIView(UIViewType.MainMenuElements);
        
        ScreenFader.Instance.FadeOut().OnCompleted(ExitApplication);
    }

    private void HandleSettingsPanelContinueClicked()
    {
        _uiViewsController.ShowUIView(UIViewType.MainMenuElements);
    }

    private void HandleLevelButtonsPanelBackClicked()
    { 
        _uiViewsController.ShowUIView(UIViewType.MainMenuElements);
    }
    
    private void HandleLevelSelected(UIEvents.LevelButtonsPanelLevelSelected ev)
    {
        _progressController.SetStartLevel(ev.LevelNumber);
        
        ScreenFader.Instance.FadeOut().OnCompleted(StartGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene(SceneNames.Game);
    }

    private void ExitApplication()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }
}
