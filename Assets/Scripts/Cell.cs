using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cell : MonoBehaviour
{
    [Header("Basic settings")]
    [SerializeField] private int _maxDetonationsToClear = 2;
    [SerializeField] private int _detonationsToClear;
    [SerializeField] private float _detonationDelayStep = 0.05f;

    [Header("View settings")]
    [SerializeField] private Sprite[] _stateSprites;
    [SerializeField] private float _emptyAlpha = 0.5f;

    private SpriteRenderer _spriteRenderer;
    
    public FieldIndex Index { get; private set; }
    public GameItem GameItem { get; set; }

    public bool IsEmpty => GameItem == null;
    public bool IsAvailable => _detonationsToClear >= 0;
    public int DetonationsToClear => _detonationsToClear;
    public bool IsAddedForDetonation { get; set; }
    public int DetonationDelayFactor { get; set; }
    public int CausedDetonationsNumber { get; set; }
    public GameItemType UpgradedGameItemType { get; set; } = GameItemType.Empty;
    public Transform CachedTransform { get; private set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        CachedTransform = transform;
    }

    public void Init(FieldIndex fIdx, int detsToClear, Vector2 pos, GameItem gItem = null)
    {
        _detonationsToClear = detsToClear;
        Index = fIdx;
        CachedTransform.localPosition = pos;
        GameItem = gItem;

        InitView();
    }

    public bool HandleItemDetonation()
    {
        if (DetonationDelayFactor < 0)
            DetonationDelayFactor = 0;

        DOTween.Sequence()
            .AppendInterval(_detonationDelayStep * DetonationDelayFactor)
            .AppendCallback(Detonate);

        return _detonationsToClear > 0;
    }
    
    public void RemoveGameItem()
    {
        if (IsEmpty)
            return;
        
        GameItem.Remove();
        GameItem = null;
    }

    public void SetSelected(bool isSelected)
    {
        GameItem.SetSelected(isSelected);
    }

    // for use in LevelEditor
    public void SetDetonationsToClear(int detonationsNumber)
    {
        _detonationsToClear = detonationsNumber;
        _detonationsToClear = Mathf.Clamp(_detonationsToClear, -1, _maxDetonationsToClear);

        if(_detonationsToClear < 0)
        {
            RemoveGameItem();
            _spriteRenderer.enabled = false;
        }
        else
        {
            _spriteRenderer.enabled = true;
        }

        UpdateView();
    }

    // for use in LevelEditor
    public void SetGameItem(GameItem gItem, Transform parent)
    {
        RemoveGameItem();

        if (gItem.ItemType == GameItemType.Empty)
            return;

        GameItem = Instantiate(gItem, parent);
        GameItem.transform.localPosition = CachedTransform.localPosition;
    }

    // for use in LevelEditor
    public void SetOrderInLayer(int orderNum)
    {
        if(_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = orderNum;
        }
    }
    
    private void InitView()
    {
        if(!IsAvailable)
        {
            _spriteRenderer.enabled = false;
            return;
        }

        UpdateView();
    }

    private void UpdateView()
    {
        if(_detonationsToClear >= 0)
        {
            _spriteRenderer.sprite = _stateSprites[_detonationsToClear];

            Color clr = _spriteRenderer.color;
            clr.a = (_detonationsToClear == 0) ? _emptyAlpha : 1f;

            _spriteRenderer.color = clr;
        }
    }

    private void Detonate()
    {
        if(_detonationsToClear > 0)
        {
            _detonationsToClear--;
            UpdateView();
        }

        if (!IsEmpty)
        {
            GameItem.Detonate();
        }
    }
}