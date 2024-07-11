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
        AddUIViewsEventsHandlers();

        ScreenFader.Instance.FadeIn().OnCompleted(() =>
        {
            _uiViewsController.ShowUIView(UIViewType.MainMenuElements);
        });
    }

    private void OnDestroy()
    {
        RemoveUIViewsEventsHandlers();
    }

    private void Init()
    {
        _progressController = GameProgressController.Instance;
        _progressController.SetTotalLevelsNumber(_settings._levelsNumberTotal);
        _uiViewsController = UIViewsController.Instance;
        _uiViewsController.SetLevelButtonsInitData(_progressController.MaxReachedLevelNumber, _progressController.LevelsNumberTotal);
    }

    private void AddUIViewsEventsHandlers()
    {
        _uiViewsController.AddUIEventSubscriber(UIEventType.SettingsContinueClick, HandleSettingsPanelContinueClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.LevelButtonsBackClick, HandleLevelButtonsPanelBackClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.LevelButtonsLevelSelected, HandleLevelButtonClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.MainMenuElementsPlayClick, HandlePlayButtonClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.MainMenuElementsSettingsClick, HandleSettingsButtonClick);
        _uiViewsController.AddUIEventSubscriber(UIEventType.MainMenuElementsExitClick, HandleExitButtonClick);
    }
    
    private void RemoveUIViewsEventsHandlers()
    {
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.SettingsContinueClick, HandleSettingsPanelContinueClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.LevelButtonsBackClick, HandleLevelButtonsPanelBackClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.LevelButtonsLevelSelected, HandleLevelButtonClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.MainMenuElementsPlayClick, HandlePlayButtonClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.MainMenuElementsSettingsClick, HandleSettingsButtonClick);
        _uiViewsController.RemoveUIEventSubscriber(UIEventType.MainMenuElementsExitClick, HandleExitButtonClick);
    }
    
    private void HandlePlayButtonClick(object param)
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        _uiViewsController.HideUIView(UIViewType.MainMenuElements);
        _uiViewsController.ShowUIView(UIViewType.LevelButtonsPanel);
    }

    private void HandleSettingsButtonClick(object param)
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        _uiViewsController.HideUIView(UIViewType.MainMenuElements);
        _uiViewsController.ShowUIView(UIViewType.SettingsPanelMainMenu);
    }

    private void HandleExitButtonClick(object param)
    {
        // AudioController.Instance.PlaySfx(SfxType.ButtonClick);
        //
        // _uiViewsController.HideUIView(UIViewType.MainMenuElements);
        //
        // ScreenFader.Instance.FadeOut().OnCompleted(ExitApplication);
    }

    private void HandleSettingsPanelContinueClick(object param)
    {
        _uiViewsController.ShowUIView(UIViewType.MainMenuElements);
    }

    private void HandleLevelButtonsPanelBackClick(object param)
    { 
        _uiViewsController.ShowUIView(UIViewType.MainMenuElements);
    }
    
    private void HandleLevelButtonClick(object param)
    {
        _progressController.SetStartLevel((int)param);
        
        ScreenFader.Instance.FadeOut().OnCompleted(StartGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene(SceneNames.Game);
    }

    private void ExitApplication()
    {
//
// #if UNITY_EDITOR
//         UnityEditor.EditorApplication.isPlaying = false;
// #else
//         Application.Quit();
// #endif
//
    }
}
