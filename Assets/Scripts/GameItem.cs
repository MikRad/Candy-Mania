using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(GameItemAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
public class GameItem : MonoBehaviour
{
    [Header("Basic settings")]
    [SerializeField] private GameItemType _itemType;

    [Header("View settings")]
    [SerializeField] private Sprite[] _sprites;

    private SpriteRenderer _spriteRenderer;
    private GameItemAnimator _animator;

    private Color _originalColor;
    
    private int _defaultSortingOrder;

    public GameItemType ItemType => _itemType;
    public GameItemType BaseItemType { get; private set; }

    public Transform CachedTransform { get; private set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<GameItemAnimator>();
        
        CachedTransform = transform;
        _defaultSortingOrder = _spriteRenderer.sortingOrder;
        _originalColor = _spriteRenderer.color;

        InitBaseType();
        InitView();
    }

    public void Init()
    {
        _spriteRenderer.color = _originalColor;
    }
    
    public void Remove()
    {
        gameObject.SetActive(false);
    }
    
    public void SetSelected(bool isSelected)
    {
        _animator.AnimateSelection(isSelected);
    }

    public void SetHinted(bool isHinted, float hintDelay = 0f)
    {
        if (isHinted)
        {
            _spriteRenderer.sortingOrder = _defaultSortingOrder + 1;
            _animator.AnimateHint(hintDelay, HandleHintCompleted);
        }
        else
        {
            _animator.TerminateHintAnimation();
            _spriteRenderer.sortingOrder = _defaultSortingOrder;
        }
    }

    public void FallTo(Vector2 destinationPos, float fallDelayFactor = 0, bool isStartFall = false)
    {
        _animator.AnimateFall(destinationPos, fallDelayFactor, isStartFall, HandleFallCompleted);
    }

    public void SwapTo(Vector2 destinationPos, bool isSuccessfulMove)
    {
        _animator.AnimateSwap(destinationPos, isSuccessfulMove, HandleSwapCompleted);
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
        AddDetonationFx();

        _animator.AnimateDetonation(HandleDetonationCompleted);
    }

    private void AddDetonationFx()
    {
        EventBus.Get.RaiseEvent(this, new GameItemDetonationStartedEvent(this));
        
        // if (this.IsUsual())
        // {
        //     AudioController.Instance.PlaySfx(SfxType.ItemDetonation);
        // }
        // else
        // {
        //     if(this.IsStar())
        //     {
        //         AudioController.Instance.PlaySfx(SfxType.StarDetonation);
        //     }
        //     else
        //     {
        //         AudioController.Instance.PlaySfx(SfxType.BombDetonation);
        //     }
        // }
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
    }
    
    private void HandleSwapCompleted()
    {
        EventBus.Get.RaiseEvent(this, new GameItemSwapCompletedEvent(this));
    }

    private void HandleFallCompleted()
    {
        _animator.AnimateFallenVfx();
        
        EventBus.Get.RaiseEvent(this, new GameItemFallCompletedEvent(this));
    }

    private void HandleHintCompleted()
    {
        _spriteRenderer.sortingOrder = _defaultSortingOrder;
        
        EventBus.Get.RaiseEvent(this, new GameItemHintCompletedEvent(this));
    }

    private void HandleDetonationCompleted()
    {
        EventBus.Get.RaiseEvent(this, new GameItemDetonationCompletedEvent(this));
    }
}
