using System;
using UnityEngine;

public class ScalerVfx : VfxBase
{
    [Header("Base Settings")]
    [SerializeField] protected float _lifeTime = 0.5f;
    [SerializeField] protected float _maxScalePercent = 10f;

    protected float _timeToDie;
    protected float _scaleFactor;
    protected Vector3 _normalScale;
    protected Transform _targetTransform;

    protected Action _onCompletedCallback;

    protected override void Awake()
    {
        base.Awake();
        
        _scaleFactor = (_maxScalePercent / 100);
    }

    private void Update()
    {
        if (_targetTransform == null)
        {
            FinishEffect();
            return;
        }
        
        UpdateScale();
        UpdateLifeTime();
    }

    public override void Init(Vector3 position, Quaternion rotation)
    {
        base.Init(position, rotation);
        
        _timeToDie = _lifeTime;
    }

    public ScalerVfx SetTarget(Transform target)
    {
        _targetTransform = target;
        _normalScale = _targetTransform.localScale;

        return this;
    }
    
    public void OnCompleted(Action onCompletedCallback)
    {
        _onCompletedCallback = onCompletedCallback;
    }
    
    private void UpdateLifeTime()
    {
        _timeToDie -= Time.deltaTime;
        
        if (_timeToDie <= 0)
        {
            FinishEffect();
        }
    }

    private void FinishEffect()
    {
        if (_targetTransform != null)
        {
            _targetTransform.localScale = _normalScale;
            _targetTransform = null;
        }
        
        _onCompletedCallback?.Invoke();
        _onCompletedCallback = null;

        // VfxController.Instance.RemoveVfx(gameObject);
        Remove();
    }

    protected virtual void UpdateScale()
    {
        float scaleDelta = _scaleFactor * (Mathf.Sin(2 * Mathf.PI * _timeToDie / _lifeTime));
        _targetTransform.localScale = new Vector3(1f + scaleDelta, 1f + scaleDelta, 0);
    }
}
