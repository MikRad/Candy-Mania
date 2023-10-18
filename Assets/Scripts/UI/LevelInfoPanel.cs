using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelInfoPanel : UIView
{
    [Header("UI elements")]
    [SerializeField] private Color _timeTextAlarmColor;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Text _possibleMovesText; // DEBUG
    [SerializeField] private Transform _vcViewsContainerTransform;
    [SerializeField] private HorizontalLayoutGroup _vcViewsContainerLayout;
    [SerializeField] private VictoryConditionView _vcViewPrefab;
    [SerializeField] private int __vcViewsContainerPadding = 120;
    
    private readonly Dictionary<VictoryCondition.Type, VictoryConditionView> _vcViewsMap = new Dictionary<VictoryCondition.Type, VictoryConditionView>();

    private Color _timeTextNormalColor;
    private Transform _timeTextTransform;

    protected override void Start()
    {
        base.Start();
        
        _timeTextTransform = _timeText.transform;
        _timeTextNormalColor = _timeText.color;
    }

    public void Init(IEnumerable<KeyValuePair<VictoryCondition.Type, VictoryCondition>> vicCons, int score, float timeRemained, int possibleMovesNum)
    {
        _timeText.color = _timeTextNormalColor;
        
        RemoveOldVCViews();
        CreateNewVCViews(vicCons);
        
        SetTimeRemained(timeRemained);
        SetScore(score);
        SetPossibleMoves(possibleMovesNum);
    }

    public void SetVictoryCondition(VictoryCondition vicCondition)
    {
        if(_vcViewsMap.TryGetValue(vicCondition._victoryConditionType, out VictoryConditionView vConView))
        {
            vConView.UpdateView(vicCondition._numberNeededToComplete);
        }
    }

    public void SetTimeRemained(float timeRemained)
    {
        int mins = ((int)timeRemained) / 60;
        int secs = ((int)timeRemained) % 60;
        string secsPrefix = (secs >= 10) ? "" : "0";

        _timeText.text = $"{mins} : {secsPrefix}{secs}";
    }

    public void HandleTimeAlarm()
    {
        _timeText.color = _timeTextAlarmColor;

        VfxController.Instance.AddScalerVfx(VfxType.Scaler)
            .SetTarget(_timeTextTransform);
    }

    public void SetPossibleMoves(int movesNumber)
    {
        _possibleMovesText.text = $"DEBUG: POSSIBLE SUCCESSFUL MOVES - {movesNumber}";
    }

    public void SetScore(int scoreValue)
    {
        _scoreText.text = $"{scoreValue}";
    }

    protected override void AddElementsListeners()
    {
        _settingsButton.onClick.AddListener(HandleSettingsClick);
    }

    protected override void RemoveElementsListeners()
    {
        _settingsButton.onClick.RemoveListener(HandleSettingsClick);
    }
    
    protected override void SetEnableElements(bool isEnabled)
    {
        _settingsButton.enabled = isEnabled;
    }

    private void CreateNewVCViews(IEnumerable<KeyValuePair<VictoryCondition.Type, VictoryCondition>> vicCons)
    {
        foreach (KeyValuePair<VictoryCondition.Type, VictoryCondition> entry in vicCons)
        {
            VictoryConditionView vcView = Instantiate(_vcViewPrefab, _vcViewsContainerTransform);
            vcView.InitView(entry.Key, entry.Value._numberNeededToComplete);
            _vcViewsMap.Add(entry.Key, vcView);
        }
        
        _vcViewsContainerLayout.padding.left = _vcViewsContainerLayout.padding.right = (_vcViewsMap.Count < 3) ? __vcViewsContainerPadding : 0;
    }

    private void RemoveOldVCViews()
    {
        if (_vcViewsMap.Count > 0)
        {
            foreach (KeyValuePair<VictoryCondition.Type, VictoryConditionView> entry in _vcViewsMap)
            {
                Destroy(entry.Value.gameObject);
            }
            _vcViewsMap.Clear();
        }
    }

    
    private void HandleSettingsClick()
    {
        if (Game.Instance.IsPaused || Game.Instance.IsLevelFinished)
            return;

        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        InvokeOnUserEvent(UIEventType.LevelInfoSettingsClick, null);
    }
}
