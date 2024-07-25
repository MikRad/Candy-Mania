using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : UIView
{
    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _exitButton;

    public override void Show()
    {
        SetActive(true);
        
        _tweener.Show((() => SetEnableElements(true)));
    }

    public override void Hide()
    {
        SetEnableElements(false);
        
        _tweener.Hide(HandleHideCompleted);
    }

    protected override void AddElementsListeners()
    {
        _playButton.onClick.AddListener(HandlePlayButtonClick);
        _settingsButton.onClick.AddListener(HandleSettingsButtonClick);
        _exitButton.onClick.AddListener(HandleExitButtonClick);
    }

    protected override void RemoveElementsListeners()
    {
        _playButton.onClick.RemoveListener(HandlePlayButtonClick);
        _settingsButton.onClick.RemoveListener(HandleSettingsButtonClick);
        _exitButton.onClick.RemoveListener(HandleExitButtonClick);
    }

    protected override void SetEnableElements(bool isEnabled)
    {
        _playButton.enabled = isEnabled;
        _settingsButton.enabled = isEnabled;
        _exitButton.enabled = isEnabled;
    }
    
    private void HandlePlayButtonClick()
    {
        EventBus.Get.RaiseEvent(this, new UIEvents.MainMenuPlayClicked());
    }

    private void HandleSettingsButtonClick()
    {
        EventBus.Get.RaiseEvent(this, new UIEvents.MainMenuSettingsClicked());
    }

    private void HandleExitButtonClick()
    {
        EventBus.Get.RaiseEvent(this, new UIEvents.MainMenuExitClicked());
    }
}
