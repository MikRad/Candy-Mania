using System;
using System.Collections;
using UnityEngine;

public class VfxDestroyChecker : MonoBehaviour
{
    [SerializeField] private float _destructionDelay = 2f;

    private Coroutine _destructionUpdater;

    public event Action OnDestroyNeeded;
    
    public void Init()
    {
        _destructionUpdater = StartCoroutine(UpdateDestructionDelay());
    }
    
    private IEnumerator UpdateDestructionDelay()
    {
        yield return new WaitForSeconds(_destructionDelay);

        StopCoroutine(_destructionUpdater);
        
        OnDestroyNeeded?.Invoke();
    }
}
