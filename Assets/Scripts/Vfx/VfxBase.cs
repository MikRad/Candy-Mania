using System;
using UnityEngine;

public class VfxBase : MonoBehaviour
{
    [SerializeField] private VfxType _type;
    
    private VfxDestroyChecker _destroyChecker;
    protected Transform _cachedTransform;

    public VfxType Type => _type;

    protected virtual void Awake()
    {
        _cachedTransform = transform;
        
        _destroyChecker = GetComponent<VfxDestroyChecker>();
        if (_destroyChecker != null)
        {
            _destroyChecker.OnDestroyNeeded += Remove;
        }
    }

    private void OnDestroy()
    {
        if (_destroyChecker != null)
        {
            _destroyChecker.OnDestroyNeeded -= Remove;
        }
    }

    public virtual void Init(Vector3 position, Quaternion rotation)
    {
        _cachedTransform.position = position;
        _cachedTransform.rotation = rotation;

        if (_destroyChecker != null)
            _destroyChecker.Init();
    }
    
    protected void Remove()
    {
        gameObject.SetActive(false);
    }
}
