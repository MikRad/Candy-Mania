using DG.Tweening;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteRenderer))]
public class GameItem : MonoBehaviour
{
    [Header("Basic settings")]
    [SerializeField] private GameItemType _itemType;

    [Header("View settings")]
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Vector3 _normalScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 _selectedScale = new Vector3(1.15f, 1.15f, 1f);

    [Header("Movement settings")]
    [SerializeField] private float _swapDuration = 0.25f;
    [SerializeField] private Ease _swapEase = Ease.Linear;
    [SerializeField] private float _fallDuration = 0.25f;
    [SerializeField] private float _startFallDuration = 0.75f;
    [SerializeField] private float _fallDelayStep = 0.05f;
    [SerializeField] private Ease _fallEase = Ease.Linear;

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

    private SpriteRenderer _spriteRenderer;

    private Color _normalColor;
    
    private int _defaultSortingOrder;
    private Sequence _hintAnimationSequence;

    public GameItemType ItemType => _itemType;
    public GameItemType BaseItemType { get; private set; }

    public Transform CachedTransform { get; private set; }

    public event Action<GameItem> OnDetonationCompleted;
    public event Action<GameItem> OnFallCompleted;
    public event Action<GameItem> OnDestroyed;
    public event Action<GameItem> OnSwapCompleted;
    public event Action<GameItem> OnHintCompleted;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        CachedTransform = transform;
        _defaultSortingOrder = _spriteRenderer.sortingOrder;
        _normalColor = _spriteRenderer.color;

        InitBaseType();
        InitView();
    }

    private void OnDestroy()
    {
        _hintAnimationSequence?.Kill();
        
        OnDestroyed?.Invoke(this);
    }

    public void Init()
    {
        _spriteRenderer.color = _normalColor;
    }
    
    public void Remove()
    {
        gameObject.SetActive(false);
        OnDestroyed?.Invoke(this);
    }
    
    public void SetSelected(bool isSelected)
    {
        VfxController.Instance.AddItemSelectionVfx()
            .SetTarget(CachedTransform, isSelected ? _selectedScale : _normalScale);
    }

    public void SetHinted(bool isHinted, float hintDelay = 0f)
    {
        if (isHinted)
        {
            _spriteRenderer.sortingOrder = _defaultSortingOrder + 1;
            
            Vector3 currentPos = CachedTransform.position;
            _hintAnimationSequence = DOTween.Sequence()
                .AppendInterval(hintDelay)
                .Append(CachedTransform.DOLocalMoveY(currentPos.y + _hintJumpDelta, _hintJumpDuration).SetEase(_hintJumpEase))
                .Append(CachedTransform.DOLocalMoveY(currentPos.y, _hintFallDuration).SetEase(_hintFallEase))
                .AppendInterval(_hintJumpInterval)
                .Append(CachedTransform.DOLocalMoveY(currentPos.y + _hintJumpDelta, _hintJumpDuration).SetEase(_hintJumpEase))
                .Append(CachedTransform.DOLocalMoveY(currentPos.y, _hintFallDuration).SetEase(_hintFallEase))
                .AppendCallback(HandleHintCompleted);
        }
        else
        {
            _hintAnimationSequence?.Kill(true);
            _spriteRenderer.sortingOrder = _defaultSortingOrder;
        }
    }

    public void FallTo(Vector2 destinationPos, float fallDelayFactor = 0, bool isStartFall = false)
    {
        float duration = isStartFall ? _startFallDuration : _fallDuration;
        DOTween.Sequence()
            .AppendInterval(_fallDelayStep * fallDelayFactor)
            .Append(CachedTransform.DOLocalMove(destinationPos, duration).SetEase(_fallEase))
            .AppendCallback(HandleFallCompleted);
    }

    public void SwapTo(Vector2 destinationPos)
    {
        DOTween.Sequence()
            .Append(CachedTransform.DOLocalMove(destinationPos, _swapDuration).SetEase(_swapEase))
            .AppendCallback(HandleSwapCompleted);
    }

    public void SwapToAndReturn(Vector2 destinationPos)
    {
        DOTween.Sequence()
            .Append(CachedTransform.DOLocalMove(destinationPos, _swapDuration).SetEase(_swapEase))
            .Append(CachedTransform.DOLocalMove(CachedTransform.position, _swapDuration).SetEase(_swapEase))
            .AppendCallback(HandleSwapCompleted);
    }

    // for use in LevelEditor
    public void SetOrderInLayer(int orderNum)
    {
        if(_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = orderNum;
        }
    }
    
    public void Detonate()
    {
        PlayDetonationFx();

        DOTween.Sequence()
            .Append(_spriteRenderer.DOFade(0f, _detonationDuration).SetEase(_detonationEase))
            .AppendCallback(HandleDetonationCompleted);
    }

    private void PlayDetonationFx()
    {
        Vector3 position = CachedTransform.position;
        
        if (this.IsUsual())
        {
            AudioController.Instance.PlaySfx(SfxType.ItemDetonation);
            VfxController.Instance.AddGameItemDetonationVfx(BaseItemType, position);
        }
        else
        {
            if(this.IsStar())
            {
                AudioController.Instance.PlaySfx(SfxType.StarDetonation);
                VfxController.Instance.AddStarCollectVfx(position);
                VfxController.Instance.AddFlyingStarVfx(position);
            }
            else
            {
                AudioController.Instance.PlaySfx(SfxType.BombDetonation);
                VfxController.Instance.AddBombDetonationVfx(position);
            }
        }
    }

    private void InitBaseType()
    {
        BaseItemType = this.GetBaseType();
    }
    
    private void InitView()
    {
        int spriteIndex = 0;
        if (_sprites.Length > 1)
        {
            spriteIndex = Random.Range(0, _sprites.Length);
        }

        _spriteRenderer.sprite = _sprites[spriteIndex];

        CachedTransform.localScale = _normalScale;
    }
    
    private void HandleSwapCompleted()
    {
        OnSwapCompleted?.Invoke(this);
    }

    private void HandleFallCompleted()
    {
        VfxController.Instance.AddFallenItemVfx().SetTarget(CachedTransform);
        OnFallCompleted?.Invoke(this);
    }

    private void HandleHintCompleted()
    {
        _spriteRenderer.sortingOrder = _defaultSortingOrder;
        OnHintCompleted?.Invoke(this);
    }

    private void HandleDetonationCompleted()
    {
        OnDetonationCompleted?.Invoke(this);
    }
}
