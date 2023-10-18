using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UITweener : MonoBehaviour
{
    [SerializeField] private float _showStartDelay;
    [SerializeField] private float _hideStartDelay;
    [SerializeField] private UITween[] _showTweens = new UITween[1];
    [SerializeField] private UITween[] _hideTweens;

    private UITween _longestShowTween;
    private UITween _longestHideTween;
    
    private void Awake()
    {
        _longestShowTween = GetLongestTween(_showTweens);
        _longestHideTween = GetLongestTween(_hideTweens);
    }

    public void Show(TweenCallback onComplete = null)
    {
        ApplyTweens(_showTweens, _showStartDelay, onComplete);
    }

    public void Hide(TweenCallback onComplete = null)
    {
        ApplyTweens(_hideTweens, _hideStartDelay, onComplete);
    }
    
    private void ApplyTween(UITween tween, float startDelay, TweenCallback onComplete)
    {
        RectTransform rect = tween._targetTransform;        
        switch (tween._uiAnimType)
        {
            case UITween.UIAnimType.AnchoredPosition:
                rect.anchoredPosition = tween._startValue;
                rect.DOAnchorPos(tween._endValue, tween._duration).SetUpdate(true).SetEase(tween._easeType).SetDelay(startDelay + tween._delay).OnComplete(onComplete);
                break;

            case UITween.UIAnimType.Scale:
                rect.localScale = tween._startValue;
                rect.DOScale(tween._endValue, tween._duration).SetUpdate(true).SetEase(tween._easeType).SetDelay(startDelay + tween._delay).OnComplete(onComplete);
                break;

            case UITween.UIAnimType.Rotation:
                rect.localRotation = Quaternion.Euler(tween._startValue);
                rect.DORotate(tween._endValue, tween._duration).SetUpdate(true).SetEase(tween._easeType).SetDelay(startDelay + tween._delay).OnComplete(onComplete);
                break;

            case UITween.UIAnimType.Alpha:
                UnityEngine.UI.Graphic uiGfx = rect.GetComponent<UnityEngine.UI.Graphic>();
                if (uiGfx != null)
                {
                    Color color = uiGfx.color;
                    color.a = tween._startValue.x;
                    uiGfx.color = color;
                    DOTween.ToAlpha(() => uiGfx.color, (c) => uiGfx.color = c,
                        tween._endValue.x, tween._duration).SetUpdate(true).SetEase(tween._easeType).SetDelay(startDelay + tween._delay).OnComplete(onComplete);
                }
                else
                {
                    CanvasGroup canvasGroup = rect.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = tween._startValue.x;
                        DOTween.To(() => canvasGroup.alpha, (a) => canvasGroup.alpha = a,
                            tween._endValue.x, tween._duration).SetUpdate(true).SetEase(tween._easeType).SetDelay(startDelay + tween._delay).OnComplete(onComplete);
                    }
                    else
                    {
                        onComplete?.Invoke();
                    }
                }
                break;

            case UITween.UIAnimType.SizeDelta:
                rect.sizeDelta = new Vector2(tween._startValue.x, tween._startValue.y);
                rect.DOSizeDelta(new Vector2(tween._endValue.x, tween._endValue.y), 
                    tween._duration).SetUpdate(true).SetEase(tween._easeType).SetDelay(tween._delay + startDelay).OnComplete(onComplete);
                break;

            case UITween.UIAnimType.AnchorMin:
                rect.anchorMin = new Vector2(tween._startValue.x, tween._startValue.y);
                rect.DOAnchorMin(new Vector2(tween._endValue.x, tween._endValue.y),
                    tween._duration).SetUpdate(true).SetEase(tween._easeType).SetDelay(tween._delay + startDelay).OnComplete(onComplete);
                break;

            case UITween.UIAnimType.AnchorMax:
                rect.anchorMax = new Vector2(tween._startValue.x, tween._startValue.y);
                rect.DOAnchorMax(new Vector2(tween._endValue.x, tween._endValue.y),
                    tween._duration).SetUpdate(true).SetEase(tween._easeType).SetDelay(tween._delay + startDelay).OnComplete(onComplete);
                break;
        }
    }

    private void ApplyTweens(UITween [] tweens, float startDelay, TweenCallback onComplete)
    {
        foreach (UITween tween in tweens)
        {
            ApplyTween(tween, startDelay, () =>
            {
                if (tween == _longestShowTween || tween == _longestHideTween)
                {
                    onComplete?.Invoke();
                }
            });
        }
    }

    private void HideHierachy(TweenCallback onComplete = null)
    {
         UITweener[] tweeners = GetComponentsInChildren<UITweener>();

        int hideCount = tweeners.Length;

        foreach (UITweener t in tweeners)
        {
            t.Hide(() => 
            {
                hideCount--;
                if (hideCount <= 0)
                    onComplete?.Invoke();
            });
        }
    }

    private void ShowHierarchy(TweenCallback onComplete = null)
    {
        UITweener[] tweeners = GetComponentsInChildren<UITweener>();

        int showCount = tweeners.Length;
        
        foreach (UITweener t in tweeners)
        {
            t.Show(() =>
            {
                showCount--;
                if (showCount <= 0)
                    onComplete?.Invoke();
            });
        }
    }

    private UITween GetLongestTween(UITween[] tweens)
    {
        if ((tweens == null) || (tweens.Length == 0))
            return null;
        
        UITween longest = tweens[0];
        for (int i = 1; i < tweens.Length; i++)
        {
            if (tweens[i].TotalDuration > longest.TotalDuration)
                longest = tweens[i];
        }

        return longest;
    }
}
