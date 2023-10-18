using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : UIView
{
    [Header("UI Elements")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _mainMenuButton;

    private UIEventType _userEventType = UIEventType.Undefined;

    protected override void Start()
    {
        base.Start();
        
        InitSlidersValues();
    }

    protected override void AddElementsListeners()
    {
        _continueButton.onClick.AddListener(ContinueClickHandler);
        _mainMenuButton?.onClick.AddListener(MainMenuClickHandler);
        
        _musicSlider.onValueChanged.AddListener(MusicVolumeChangeHandler);
        _sfxSlider.onValueChanged.AddListener(SfxVolumeChangeHandler);
    }

    protected override void RemoveElementsListeners()
    {
        _continueButton.onClick.RemoveListener(ContinueClickHandler);
        _mainMenuButton?.onClick.RemoveListener(MainMenuClickHandler);
        
        _musicSlider.onValueChanged.RemoveListener(MusicVolumeChangeHandler);
        _sfxSlider.onValueChanged.RemoveListener(SfxVolumeChangeHandler);
    }

    private void ContinueClickHandler()
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        Hide();
        
        _userEventType = UIEventType.SettingsContinueClick;
    }

    private void MainMenuClickHandler()
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        Hide();
        
        _userEventType = UIEventType.SettingsMainMenuClick;
    }

    private void InitSlidersValues()
    {
        _musicSlider.value = AudioController.Instance.MusicVolume;
        _sfxSlider.value = AudioController.Instance.SfxVolume;
    }
    
    private void MusicVolumeChangeHandler(float value)
    {
        AudioController.Instance.MusicVolume = value;
    }

    private void SfxVolumeChangeHandler(float value)
    {
        AudioController.Instance.SfxVolume = value;
    }

    protected override void SetEnableElements(bool isEnabled)
    {
        _continueButton.enabled = isEnabled;
        
        if(_mainMenuButton != null)
        {
            _mainMenuButton.enabled = isEnabled;
        }
        
        _sfxSlider.enabled = isEnabled;
        _musicSlider.enabled = isEnabled;
    }

    protected override void HandleHideCompleted()
    {
        base.HandleHideCompleted();
        
        InvokeOnUserEvent(_userEventType, null);

        _userEventType = UIEventType.Undefined;
    }
}
