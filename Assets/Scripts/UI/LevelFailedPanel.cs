using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelFailedPanel : UIView
{
    [Header("UI Elements")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private TextMeshProUGUI _levelScoreValueText;
    [SerializeField] private TextMeshProUGUI _playedTimeValueText;

    [Header("Animation settings")]
    [SerializeField] private float _scoreCountDuration = 1f;

    private int _levelScore;
    
    public override void Show()
    {
        SetActive(true);
        
        AudioController.Instance.PlaySfx(SfxType.UIPanelSlide);

        _tweener.Show(StartScoreCountAnimation);
    }

    protected override void AddElementsListeners()
    {
        EventBus.Get.Subscribe<LevelFailedEvent>(Init);
        
        _restartButton.onClick.AddListener(HandleRestartClick);
    }

    protected override void RemoveElementsListeners()
    {
        EventBus.Get.Unsubscribe<LevelFailedEvent>(Init);
        
        _restartButton.onClick.RemoveListener(HandleRestartClick);
    }

    private void HandleRestartClick()
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        Hide();
    }

    protected override void SetEnableElements(bool isEnabled)
    {
        _restartButton.enabled = isEnabled;
    }

    private void StartScoreCountAnimation()
    {
        AudioController.Instance.PlaySfx(SfxType.ScoreCount);

        _levelScoreValueText.DoTextInt(0, _levelScore, _scoreCountDuration)
            .onComplete = () => {SetEnableElements(true);};
    }

    protected override void HandleHideCompleted()
    {
        base.HandleHideCompleted();

        EventBus.Get.RaiseEvent(this, new UIEvents.LevelFailedPanelRestartClicked());
    }

    private void Init(LevelFailedEvent ev)
    {
        _levelScore = ev.LevelScore;
        
        _levelScoreValueText.text = "0";

        int mins = ((int)ev.TimePlayed) / 60;
        int secs = ((int)ev.TimePlayed) % 60;
        string secsPrefix = (secs >= 10) ? "" : "0";

        _playedTimeValueText.text = $"{mins} : {secsPrefix}{secs}";
    }
}
