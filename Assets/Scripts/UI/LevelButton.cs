using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    private const int Available = 0;
    private const int Unavailable = 1;
    
    [SerializeField] private Button _button;
    [SerializeField] private Sprite[] _stateSprites;
    [SerializeField] private TextMeshProUGUI _levelNumberText;

    private int _levelNumber;
    
    public bool IsAvailable { get; private set; }
    
    public void InitView(int levelNum, bool isAvailable, Action<int> onClickCallback)
    {
        _button.onClick.RemoveAllListeners();
        
        _levelNumber = levelNum;
        _levelNumberText.text = _levelNumber.ToString();
        IsAvailable = isAvailable;

        if(IsAvailable)
        {
            _button.image.sprite = _stateSprites[Available];
            _button.onClick.AddListener(() =>
            {
                AudioController.Instance.PlaySfx(SfxType.ButtonClick);
                onClickCallback(_levelNumber);
            });
        }
        else
        {
            _button.image.sprite = _stateSprites[Unavailable];
            SetEnable(false);
        }
    }

    public void SetEnable(bool isEnabled)
    {
        _button.enabled = isEnabled;
    }
}
