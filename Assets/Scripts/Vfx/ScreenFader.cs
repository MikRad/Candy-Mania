using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : SingletonMonoBehaviour<ScreenFader>
{
    private enum State {FadeIn, FadeOut, Idle}

    [Header("Base settings")]
    [SerializeField] private float _fadeInDuration = 1f;
    [SerializeField] private float _fadeOutDuration = 1f;
    [SerializeField] private Image _fadeImage;

    private Tweener _fadeAnimation;
    private State _state;

    public bool IsFading => _state != State.Idle;  
    
    private Action _onVfxCompletedCallback;

    protected override void Awake()
    {
        base.Awake();
        
        _fadeImage.gameObject.SetActive(true);
    }

    public ScreenFader FadeIn()
    {
        StartVfx(State.FadeIn);
        return this;
    }

    public ScreenFader FadeOut()
    {
        StartVfx(State.FadeOut);
        return this;
    }

    public void OnCompleted(Action onCompletedCallback)
    {
        _onVfxCompletedCallback = onCompletedCallback;
    }
    
    private void StartVfx(State state)
    {
        if (state == State.Idle)
            return;
        
        _state = state;
        
        _fadeAnimation?.Kill();
        _fadeAnimation = (state == State.FadeIn) ? _fadeImage.DOFade(0f, _fadeInDuration) : 
                                                _fadeImage.DOFade(1f, _fadeOutDuration);
        
        _fadeAnimation.onComplete = (() =>
        {
            _onVfxCompletedCallback?.Invoke();
            _onVfxCompletedCallback = null;
            
            _state = State.Idle;
        });
    }
}
