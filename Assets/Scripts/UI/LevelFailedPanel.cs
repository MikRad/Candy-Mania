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
    
    public void Init(int levelScore, float levelTime)
    {
        _levelScore = levelScore;
        
        _levelScoreValueText.text = "0";

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
        _restartButton.onClick.AddListener(HandleRestartClick);
    }

    protected override void RemoveElementsListeners()
    {
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

        InvokeOnUserEvent(UIEventType.LevelFailedRestartClick, null);
    }
}
