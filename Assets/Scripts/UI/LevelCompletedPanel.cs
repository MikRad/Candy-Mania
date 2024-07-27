using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCompletedPanel : UIView
{
    [Header("UI Elements")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private TextMeshProUGUI _levelScoreValueText;
    [SerializeField] private TextMeshProUGUI _totalScoreValueText;
    [SerializeField] private TextMeshProUGUI _maxComboValueText;
    [SerializeField] private TextMeshProUGUI _playedTimeValueText;

    [Header("Animation settings")]
    [SerializeField] private float _scoreCountDuration = 1f;
    
    private int _levelScore;
    private int _totalScore;

    public override void Show()
    {
        SetActive(true);

        EventBus.Get.RaiseEvent(this, new UIEvents.ViewMoving());

        _tweener.Show(StartScoreCountAnimation);
    }

    protected override void AddElementsListeners()
    {
        EventBus.Get.Subscribe<LevelCompletedEvent>(Init);
        
        _continueButton.onClick.AddListener(HandleContinueClick);
    }

    protected override void RemoveElementsListeners()
    {
        EventBus.Get.Unsubscribe<LevelCompletedEvent>(Init);
        
        _continueButton.onClick.AddListener(HandleContinueClick);
    }

    private void HandleContinueClick()
    {
        EventBus.Get.RaiseEvent(this, new UIEvents.ButtonClicked());

        Hide();
    }

    protected override void SetEnableElements(bool isEnabled)
    {
        _continueButton.enabled = isEnabled;
    }

    private void StartScoreCountAnimation()
    {
        EventBus.Get.RaiseEvent(this, new ScoreCountingEvent());

        _levelScoreValueText.DoTextInt(0, _levelScore, _scoreCountDuration);
        _totalScoreValueText.DoTextInt((_totalScore - _levelScore), _totalScore, _scoreCountDuration)
            .onComplete = () => {SetEnableElements(true);};
    }

    protected override void HandleHideCompleted()
    {
        base.HandleHideCompleted();
     
        EventBus.Get.RaiseEvent(this, new UIEvents.LevelCompletedPanelContinueClicked());
    }
    
    private void Init(LevelCompletedEvent ev)
    {
        _levelScore = ev.LevelScore;
        _totalScore = ev.TotalScore;

        _levelScoreValueText.text = "0";
        _totalScoreValueText.text = (_totalScore - _levelScore).ToString();
        _maxComboValueText.text = $"X {ev.MaxCombo}";

        int mins = ((int)ev.TimePlayed) / 60;
        int secs = ((int)ev.TimePlayed) % 60;
        string secsPrefix = (secs >= 10) ? "" : "0";

        _playedTimeValueText.text = $"{mins} : {secsPrefix}{secs}";
    }
}
