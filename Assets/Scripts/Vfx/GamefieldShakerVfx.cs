using DG.Tweening;
using UnityEngine;

public class GamefieldShakerVfx : MonoBehaviour
{
    [Header("GameField shake settings")]
    [SerializeField] private Transform _gameFieldTransform;
    [SerializeField] private float _gameFieldShakeDuration = 0.3f;
    [SerializeField] private float _gameFieldShakeDelta = 0.1f;
    [SerializeField] private Ease _gameFieldShakeEase = Ease.OutCubic;

    public void Shake()
    {
        // DOTween.Sequence()
        //     .Append(_gameFieldTransform.DOMoveY(_gameFieldTransform.position.y - _gameFieldShakeDelta, _gameFieldShakeDuration)).SetEase(_gameFieldShakeEase)
        //     .Append(_gameFieldTransform.DOMoveY(_gameFieldTransform.position.y, _gameFieldShakeDuration)).SetEase(_gameFieldShakeEase);
    }
}
