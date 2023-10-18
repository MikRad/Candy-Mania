using UnityEngine;

public class ItemSelectionVfx : VfxBase
{
    [Header("Base Settings")]
    [SerializeField] private float _lifeTime = 0.5f;

    private float _timeToDie;
    private Transform _targetTransform;
    private Vector3 _startScaleValue;
    private Vector3 _targetScaleValue;

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

    public ItemSelectionVfx SetTarget(Transform target, Vector3 targetScaleValue)
    {
        _targetTransform = target;
        _startScaleValue = _targetTransform.localScale;
        _targetScaleValue = targetScaleValue;

        return this;
    }
    
    private void UpdateLifeTime()
    {
        _timeToDie -= Time.deltaTime;
        
        if (_timeToDie <= 0)
        {
            FinishEffect();
        }
    }

    private void UpdateScale()
    {
        _targetTransform.localScale = _startScaleValue + (_targetScaleValue - _startScaleValue) * (_lifeTime - _timeToDie) / _lifeTime;
    }
    
    private void FinishEffect()
    {
        if (_targetTransform != null)
        {
            _targetTransform.localScale = _targetScaleValue;
            _targetTransform = null;
        }
        
        Remove();        
    }
}