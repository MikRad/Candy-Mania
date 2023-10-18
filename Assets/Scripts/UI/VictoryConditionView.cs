using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryConditionView : MonoBehaviour
{
    [SerializeField] private Image _vcImage;
    [SerializeField] private TextMeshProUGUI _vcNumberNeededToCompleteText;

    [SerializeField] private VCSpriteInfo[] _vcSpriteInfos;

    private bool _isVfxPerforming;

    private Transform _cachedTransform;

    private void Awake()
    {
        _cachedTransform = transform;
    }

    public void InitView(VictoryCondition.Type vcType, int numberNeeded)
    {
        _vcImage.sprite = GetSpriteByType(vcType);
        _vcNumberNeededToCompleteText.text = numberNeeded.ToString();
    }

    public void UpdateView(int numberNeeded)
    {
        if(!_isVfxPerforming)
        {
            _isVfxPerforming = true;

            VfxController.Instance.AddScalerVfx(VfxType.ScalerVCView)
                .SetTarget(_cachedTransform)
                .OnCompleted(HandleVfxCompleted);
        }

        _vcNumberNeededToCompleteText.text = numberNeeded.ToString();
    }

    private Sprite GetSpriteByType(VictoryCondition.Type vcType)
    {
        foreach (VCSpriteInfo spriteInfo in _vcSpriteInfos)
        {
            if (spriteInfo._vcType == vcType)
                return spriteInfo._sprite;
        }

        return null;
    }
    
    private void HandleVfxCompleted()
    {
        _isVfxPerforming = false;
    }
}