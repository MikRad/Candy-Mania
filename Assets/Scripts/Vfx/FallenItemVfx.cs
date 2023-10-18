using UnityEngine;

public class FallenItemVfx : ScalerVfx
{
    protected override void UpdateScale()
    {
        float scaleDelta = _scaleFactor * (Mathf.Sin(2 * Mathf.PI * _timeToDie / _lifeTime));
        _targetTransform.localScale = new Vector3(1f + scaleDelta, 1f - 2 * scaleDelta, 1f);
    }
}