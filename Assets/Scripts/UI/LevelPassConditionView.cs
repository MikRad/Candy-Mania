using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPassConditionView : MonoBehaviour
{
    [SerializeField] private Image _lpcImage;
    [SerializeField] private TextMeshProUGUI _lpcNumberNeededToCompleteText;

    [SerializeField] private LPCSpriteInfo[] _lpcSpriteInfos;

    private bool _isVfxPerforming;

    private Transform _cachedTransform;

    private void Awake()
    {
        _cachedTransform = transform;
    }

    public void InitView(LevelPassCondition.Type lpcType, int numberNeeded)
    {
        _lpcImage.sprite = GetSpriteByType(lpcType);
        _lpcNumberNeededToCompleteText.text = numberNeeded.ToString();
    }

    public void UpdateView(int numberNeeded)
    {
        if(!_isVfxPerforming)
        {
            _isVfxPerforming = true;

            VfxController.Instance.AddScalerVfx(VfxType.ScalerLPCView)
                .SetTarget(_cachedTransform)
                .OnCompleted(HandleVfxCompleted);
        }

        _lpcNumberNeededToCompleteText.text = numberNeeded.ToString();
    }

    private Sprite GetSpriteByType(LevelPassCondition.Type lpcType)
    {
        foreach (LPCSpriteInfo spriteInfo in _lpcSpriteInfos)
        {
            if (spriteInfo._lpcType == lpcType)
                return spriteInfo._sprite;
        }

        return null;
    }
    
    private void HandleVfxCompleted()
    {
        _isVfxPerforming = false;
    }
    
    [Serializable]
    private class LPCSpriteInfo
    {
        public LevelPassCondition.Type _lpcType;
        public Sprite _sprite;
    }    
}