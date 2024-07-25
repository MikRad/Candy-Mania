using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonsPanel : UIView
{
    [Header("UI Elements")]
    [SerializeField] private Transform _levelButtonsContainer;
    [SerializeField] private LevelButton _levelButtonPrefab;
    [SerializeField] private Button _backButton;

    private readonly List<LevelButton> _levelButtons = new List<LevelButton>();

    private int _selectedLevelNumber;

    public void Init(int maxReachedLevelNumber, int levelsNumberTotal)
    {
        if (_levelButtons.Count > 0)
        {
            SetAvailableLevelsNumber(maxReachedLevelNumber);
            return;
        }
        
        for (int i = 1; i <= levelsNumberTotal; i++)
        {
            LevelButton lBtn = Instantiate(_levelButtonPrefab, _levelButtonsContainer);
            lBtn.InitView(i, (i <= maxReachedLevelNumber), HandleLevelButtonClick);
            _levelButtons.Add(lBtn);
        }
    }

    protected override void AddElementsListeners()
    {
        _backButton.onClick.AddListener(HandleBackButtonClick);
    }

    protected override void RemoveElementsListeners()
    {
        _backButton.onClick.RemoveListener(HandleBackButtonClick);
    }

    protected override void SetEnableElements(bool isEnabled)
    {
        foreach (LevelButton lButton in _levelButtons)
        {
            if (lButton.IsAvailable)
            {
                lButton.SetEnable(isEnabled);
            }
        }
    }
    
    private void SetAvailableLevelsNumber(int availableLevelsNumber)
    {
        for (int i = 0; i < _levelButtons.Count; i++)
        {
            _levelButtons[i].InitView(i + 1, (i < availableLevelsNumber), HandleLevelButtonClick);
        }
    }
    
    private void HandleLevelButtonClick(int levelNumber)
    {
        _selectedLevelNumber = levelNumber;
        
        SetEnableElements(false);
        Hide();
    }
    
    private void HandleBackButtonClick()
    {
        _selectedLevelNumber = 0;
        
        SetEnableElements(false);
        Hide();
        
        EventBus.Get.RaiseEvent(this, new UIEvents.LevelButtonsPanelBackClicked());
    }

    protected override void HandleHideCompleted()
    {
        base.HandleHideCompleted();

        if (_selectedLevelNumber > 0)
        {
            EventBus.Get.RaiseEvent(this, new UIEvents.LevelButtonsPanelLevelSelected(_selectedLevelNumber));
        }
    }
}
