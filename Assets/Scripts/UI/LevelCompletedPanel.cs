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
    
    public void Init(int levelScore, int totalScore, int maxComboCount, float levelTime)
    {
        _levelScore = levelScore;
        _totalScore = totalScore;

        _levelScoreValueText.text = "0";
        _totalScoreValueText.text = (totalScore - levelScore).ToString();
        _maxComboValueText.text = $"X {maxComboCount}";

        int mins = ((int)levelTime) / 60;
        int secs = ((int)levelTime) % 60;
        string secsPrefix = (secs >= 10) ? "" : "0";

        _playedTimeValueText.text = $"{mins} : {secsPrefix}{secs}";
    }

    public override void Show()
    {
        SetActive(true);
        
        AudioController.Instance.PlaySfx(SfxType.UIPanelSlide);

        _tweener.Show(StartScoreCountAnimation);
    }

    protected override void AddElementsListeners()
    {
        _continueButton.onClick.AddListener(HandleContinueClick);
    }

    protected override void RemoveElementsListeners()
    {
        _continueButton.onClick.AddListener(HandleContinueClick);
    }

    private void HandleContinueClick()
    {
        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        Hide();
    }

    protected override void SetEnableElements(bool isEnabled)
    {
        _continueButton.enabled = isEnabled;
    }

    private void StartScoreCountAnimation()
    {
        AudioController.Instance.PlaySfx(SfxType.ScoreCount);

        _levelScoreValueText.DoTextInt(0, _levelScore, _scoreCountDuration);
        _totalScoreValueText.DoTextInt((_totalScore - _levelScore), _totalScore, _scoreCountDuration)
            .onComplete = () => {SetEnableElements(true);};
    }

    protected override void HandleHideCompleted()
    {
        base.HandleHideCompleted();
     
        EventBus.Get.RaiseEvent(this, new UIEvents.LevelCompletedPanelContinueClicked());
    }
}
