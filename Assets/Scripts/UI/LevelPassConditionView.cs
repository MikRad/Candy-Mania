using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPassConditionView : MonoBehaviour
{
    [SerializeField] private Image _lpcImage;
    [SerializeField] private TextMeshProUGUI _lpcNumberNeededToCompleteText;
    [SerializeField] private LPCSpriteInfo[] _lpcSpriteInfos;

    [Header("Animation")]
    [SerializeField] private float _updateAnimationDuration = 0.2f;
    [SerializeField] private Vector3 _updateAnimationScale = new Vector3(1.1f, 1.1f, 1f);

    private Tween _updateAnimation;
    private Transform _imageTransform;

    private void Awake()
    {
        _imageTransform = _lpcImage.transform;
    }

    public void InitView(LevelPassCondition.Type lpcType, int numberNeeded)
    {
        _lpcImage.sprite = GetSpriteByType(lpcType);
        _lpcNumberNeededToCompleteText.text = numberNeeded.ToString();
    }

    public void UpdateView(int numberNeeded)
    {
        _lpcNumberNeededToCompleteText.text = numberNeeded.ToString();

        if (_updateAnimation != null && _updateAnimation.IsActive())
            return;
        
        _updateAnimation = _imageTransform.DOScale(_updateAnimationScale, _updateAnimationDuration)
            .SetLoops(4, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
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
    
    [Serializable]
    private class LPCSpriteInfo
    {
        public LevelPassCondition.Type _lpcType;
        public Sprite _sprite;
    }    
}