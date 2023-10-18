using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class UITween
{
    public enum UIAnimType
    {
        AnchoredPosition,
        Scale,
        Rotation,
        Alpha,
        SizeDelta,
        AnchorMin,
        AnchorMax,
    }

    public RectTransform _targetTransform;
    public UIAnimType _uiAnimType = UIAnimType.AnchoredPosition;
    public Ease _easeType = Ease.OutQuad;
    public Vector3 _startValue = Vector3.zero;
    public Vector3 _endValue = Vector3.one;
    public float _duration = 0.25f;
    public float _delay;

    public float TotalDuration => _duration + _delay;
}