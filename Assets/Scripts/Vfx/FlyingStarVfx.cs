using DG.Tweening;
// using Lean.Pool;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FlyingStarVfx : VfxBase
{
    [Header("Flying Settings")]
    [SerializeField] private Vector3 _phase1PosDelta;
    [SerializeField] private Vector3 _phase2PosDelta;
    [SerializeField] private float _phase1Duration = 1f;
    [SerializeField] private float _phase2Duration = 2f;
    [SerializeField] private float _phase2EndAlpha = 0.25f;
    [SerializeField] private Vector3 _normalScale;
    [SerializeField] private Vector3 _maxScale;

    private SpriteRenderer _spriteRenderer;
    private Color _startColor;

    // private Transform _cachedTransform;

    protected override void Awake()
    {
        base.Awake();
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startColor = _spriteRenderer.color;
        
        _cachedTransform = transform;
    }

    public override void Init(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"Flying star init !");
        
        base.Init(position, rotation);
        
        _cachedTransform.localScale = _normalScale;
        _spriteRenderer.color = _startColor;

        DOTween.Sequence()
            .Append(_cachedTransform.DOMove(_cachedTransform.position + _phase1PosDelta, _phase1Duration))
            .Join(_cachedTransform.DOScale(_maxScale, _phase1Duration))
            .Append(_cachedTransform.DOMove(_cachedTransform.position + _phase1PosDelta + _phase2PosDelta, _phase2Duration))
            .Join(_cachedTransform.DOScale(_normalScale, _phase2Duration))
            .Join(_spriteRenderer.DOFade(_phase2EndAlpha, _phase2Duration))
            .AppendCallback(HandleFlyingComplete);
    }

    private void HandleFlyingComplete()
    {
        Debug.Log($"Flying complete !");
        
        Remove();
        // VfxController.Instance.RemoveVfx(gameObject);
    }
}

