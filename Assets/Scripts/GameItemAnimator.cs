using DG.Tweening;
using UnityEngine;

public class GameItemAnimator : MonoBehaviour
{
    [Header("Selection settings")]
    [SerializeField] private Vector3 _selectedScale = new Vector3(1.15f, 1.15f, 1f);
    [SerializeField] private float _selectionDuration = 0.3f;
    
    [Header("Fall settings")]
    [SerializeField] private Vector3 _fallenVfxScale = new Vector3(1f, 0.85f, 1f);
    [SerializeField] private float _fallenVfxDuration = 0.4f;
    [SerializeField] private float _fallDuration = 0.25f;
    [SerializeField] private float _fallDelayStep = 0.05f;
    [SerializeField] private Ease _fallEase = Ease.Linear;
    [SerializeField] private float _startFallDuration = 0.75f;
    
    [Header("Movement settings")]
    [SerializeField] private float _swapDuration = 0.25f;
    [SerializeField] private Ease _swapEase = Ease.Linear;
    
    [Header("Detonation settings")]
    [SerializeField] private float _detonationDuration = 0.5f;
    [SerializeField] private Ease _detonationEase = Ease.Linear;

    [Header("Hint settings")]
    [SerializeField] private float _hintJumpDuration = 0.15f;
    [SerializeField] private float _hintJumpDelta = 0.25f;
    [SerializeField] private float _hintJumpInterval = 0.25f;
    [SerializeField] private float _hintFallDuration = 0.25f;
    [SerializeField] private Ease _hintJumpEase = Ease.OutCubic;
    [SerializeField] private Ease _hintFallEase = Ease.OutCubic;

    private Vector3 _originalScale;

    private Sequence _hintAnimationSequence;
    private Sequence _fallenVfxAnimation;
    private Tween _selectionAnimation;
    
    private Transform _cachedTransform;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _cachedTransform = transform;
        _originalScale = _cachedTransform.localScale;
    }

    private void OnDestroy()
    {
        _hintAnimationSequence?.Kill();
        _fallenVfxAnimation?.Kill();
        _selectionAnimation?.Kill();
    }
    
    public void AnimateSelection(bool isSelected)
    {
        _selectionAnimation?.Kill();

        Vector3 targetScale = (isSelected) ? _selectedScale : _originalScale;
        _selectionAnimation = _cachedTransform.DOScale(targetScale, _selectionDuration);
    }

    public void AnimateHint(float hintDelay, TweenCallback onCompletedCallback)
    {
        Vector3 currentPos = _cachedTransform.position;
        _hintAnimationSequence = DOTween.Sequence()
            .AppendInterval(hintDelay)
            .Append(_cachedTransform.DOLocalMoveY(currentPos.y + _hintJumpDelta, _hintJumpDuration).SetEase(_hintJumpEase))
            .Append(_cachedTransform.DOLocalMoveY(currentPos.y, _hintFallDuration).SetEase(_hintFallEase))
            .AppendInterval(_hintJumpInterval)
            .Append(_cachedTransform.DOLocalMoveY(currentPos.y + _hintJumpDelta, _hintJumpDuration).SetEase(_hintJumpEase))
            .Append(_cachedTransform.DOLocalMoveY(currentPos.y, _hintFallDuration).SetEase(_hintFallEase))
            .AppendCallback(onCompletedCallback);
    }

    public void TerminateHintAnimation()
    {
        _hintAnimationSequence?.Kill(true);
    }

    public void AnimateDetonation(TweenCallback onCompletedCallback)
    {
        DOTween.Sequence()
            .Append(_spriteRenderer.DOFade(0f, _detonationDuration).SetEase(_detonationEase))
            .AppendCallback(onCompletedCallback);
    }

    public void AnimateSwap(Vector2 destinationPos, bool isSuccessfulMove, TweenCallback onCompletedCallback)
    {
        Sequence swapSequence = DOTween.Sequence();
        swapSequence.Append(_cachedTransform.DOLocalMove(destinationPos, _swapDuration).SetEase(_swapEase));
        
        if (!isSuccessfulMove)
        {
            swapSequence.Append(_cachedTransform.DOLocalMove(_cachedTransform.position, _swapDuration)
                .SetEase(_swapEase));
        }
        
        swapSequence.AppendCallback(onCompletedCallback);
    }

    public void AnimateFall(Vector2 destinationPos, float fallDelayFactor, bool isStartFall, TweenCallback onCompletedCallback)
    {
        float duration = isStartFall ? _startFallDuration : _fallDuration;
        
        DOTween.Sequence()
            .AppendInterval(_fallDelayStep * fallDelayFactor)
            .Append(_cachedTransform.DOLocalMove(destinationPos, duration).SetEase(_fallEase))
            .AppendCallback(onCompletedCallback);
    }

    public void AnimateFallenVfx()
    {
        _fallenVfxAnimation?.Kill();
        
        _fallenVfxAnimation = DOTween.Sequence();
        _fallenVfxAnimation.Append(_cachedTransform.DOScale(_fallenVfxScale, _fallenVfxDuration / 2));
        _fallenVfxAnimation.Append(_cachedTransform.DOScale(_originalScale, _fallenVfxDuration / 2));
    }
}
