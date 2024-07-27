using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] private Transform _lpcViewsContainerTransform;
    [SerializeField] private HorizontalLayoutGroup _lpcViewsContainerLayout;
    [SerializeField] private LevelPassConditionView _lpcViewPrefab;
    [SerializeField] private int __lpcViewsContainerPadding = 120;
    
    [Header("UI animation")]
    [SerializeField] private float _timeAlarmAnimationDuration = 0.3f;
    [SerializeField] private Vector3 _timeAlarmAnimationScale = new Vector3(1.1f, 1.1f, 1f);
    
    private readonly Dictionary<LevelPassCondition.Type, LevelPassConditionView> _lpcViewsMap = new Dictionary<LevelPassCondition.Type, LevelPassConditionView>();

    private Color _timeTextNormalColor;
    private Transform _timeTextTransform;

    protected override void Start()
    {
        base.Start();
        
        _timeTextTransform = _timeText.transform;
        _timeTextNormalColor = _timeText.color;
    }

    public void Init(IEnumerable<LevelPassCondition> lpConditions, int score, float timeRemained)
    {
        _timeText.color = _timeTextNormalColor;
        
        RemoveOldLPCViews();
        CreateNewLPCViews(lpConditions);
        
        SetTimeRemained(timeRemained);
        SetScore(score);
    }

    public void SetTimeRemained(float timeRemained)
    {
        int mins = ((int)timeRemained) / 60;
        int secs = ((int)timeRemained) % 60;
        string secsPrefix = (secs >= 10) ? "" : "0";

        _timeText.text = $"{mins} : {secsPrefix}{secs}";
    }

    protected override void AddElementsListeners()
    {
        EventBus.Get.Subscribe<LevelTimeExpiringEvent>(HandleTimeAlarm);
        EventBus.Get.Subscribe<PossibleMovesChangedEvent>(HandlePossibleMovesChanged);
        EventBus.Get.Subscribe<ScoreChangedEvent>(HandleScoreChanged);
        EventBus.Get.Subscribe<LevelPassConditionUpdatedEvent>(HandleLevelPassConditionUpdated);
        
        _settingsButton.onClick.AddListener(HandleSettingsClick);
    }

    protected override void RemoveElementsListeners()
    {
        EventBus.Get.Unsubscribe<LevelTimeExpiringEvent>(HandleTimeAlarm);
        EventBus.Get.Unsubscribe<PossibleMovesChangedEvent>(HandlePossibleMovesChanged);
        EventBus.Get.Unsubscribe<ScoreChangedEvent>(HandleScoreChanged);
        EventBus.Get.Unsubscribe<LevelPassConditionUpdatedEvent>(HandleLevelPassConditionUpdated);
        
        _settingsButton.onClick.RemoveListener(HandleSettingsClick);
    }

    protected override void SetEnableElements(bool isEnabled)
    {
        _settingsButton.enabled = isEnabled;
    }

    private void CreateNewLPCViews(IEnumerable<LevelPassCondition> lpConditions)
    {
        foreach (LevelPassCondition condition in lpConditions)
        {
            LevelPassConditionView lpcView = Instantiate(_lpcViewPrefab, _lpcViewsContainerTransform);
            lpcView.InitView(condition._levelPassConditionType, condition._numberNeededToComplete);
            _lpcViewsMap.Add(condition._levelPassConditionType, lpcView);
        }
        
        _lpcViewsContainerLayout.padding.left = _lpcViewsContainerLayout.padding.right = (_lpcViewsMap.Count < 3) ? __lpcViewsContainerPadding : 0;
    }

    private void RemoveOldLPCViews()
    {
        if (_lpcViewsMap.Count > 0)
        {
            foreach (KeyValuePair<LevelPassCondition.Type, LevelPassConditionView> entry in _lpcViewsMap)
            {
                Destroy(entry.Value.gameObject);
            }
            _lpcViewsMap.Clear();
        }
    }
    
    private void SetScore(int scoreValue)
    {
        _scoreText.text = $"{scoreValue}";
    }
    
    private void HandleScoreChanged(ScoreChangedEvent ev)
    {
        SetScore(ev.TotalScore);
    }

    private void HandleTimeAlarm()
    {
        _timeText.color = _timeTextAlarmColor;

        _timeTextTransform.DOScale(_timeAlarmAnimationScale, _timeAlarmAnimationDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    private void HandleLevelPassConditionUpdated(LevelPassConditionUpdatedEvent ev)
    {
        if(_lpcViewsMap.TryGetValue(ev.PassConditionType, out LevelPassConditionView lpcView))
        {
            lpcView.UpdateView(ev.NumberToComplete);
        }
    }
    
    private void HandlePossibleMovesChanged(PossibleMovesChangedEvent ev)
    {
        _possibleMovesText.text = $"DEBUG: POSSIBLE SUCCESSFUL MOVES - {ev.PossibleMoves}";
    }
    
    private void HandleSettingsClick()
    {
        if (Game.Instance.IsPaused || Game.Instance.IsLevelFinished)
            return;

        AudioController.Instance.PlaySfx(SfxType.ButtonClick);

        EventBus.Get.RaiseEvent(this, new UIEvents.LevelInfoPanelSettingsClicked());
    }
}
