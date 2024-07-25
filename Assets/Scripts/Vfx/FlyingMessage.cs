using TMPro;
using UnityEngine;

public class FlyingMessage : VfxBase
{
    public enum MessageType {Positive, Negative, Warning }

    [Header("Base Settings")]
    [SerializeField] private TextMeshPro _messageText;
    [SerializeField] private Color _positiveColor = new Color(0.0f, 1.0f, 0.0f);
    [SerializeField] private Color _negativeColor = new Color(1.0f, 0.0f, 0.0f);
    [SerializeField] private Color _warningColor = new Color(1.0f, 0.5f, 0.0f);

    [Header("Flying Settings")]
    [SerializeField] private float _flyingSpeed = 1f;
    [SerializeField] private float _lifeTime = 1.5f;

    [Header("Scale Settings")]
    [SerializeField] private float _scaleTime = 0.5f;
    [SerializeField] private float _scaleMaxPercent = 25f;

    private float _timeToDisappear;
    private bool _isScaleCompleted;
    private float _startScaleFactor;

    private void Update()
    {
        UpdateLifeTime();
        UpdatePosition();
        UpdateColor();
        UpdateScale();
    }

    public override void Init(Vector3 position, Quaternion rotation)
    {
        base.Init(position, rotation);
        
        _timeToDisappear = _lifeTime;
        _startScaleFactor = _scaleMaxPercent / 100;
        _isScaleCompleted = false;
    }

    public void SetText(string text, MessageType messageType)
    {
        _messageText.text = text;
        switch (messageType)
        {
            case MessageType.Positive:
                _messageText.color = _positiveColor;
                break;
            case MessageType.Negative:
                _messageText.color = _negativeColor;
                break;
            case MessageType.Warning:
                _messageText.color = _warningColor;
                break;
        }
    }
    
    private void UpdateLifeTime()
    {
        _timeToDisappear -= Time.deltaTime;

        if(_timeToDisappear <= 0) 
        {
            Remove();
        }
    }
    
    private void UpdatePosition()
    {
        _cachedTransform.Translate(0, _flyingSpeed * Time.deltaTime, 0);
    }

    private void UpdateColor()
    {
        Color color = _messageText.color;
        float alpha = (1f + _timeToDisappear / _lifeTime) / 2;
        color.a = alpha;
        _messageText.color = color;
    }
    private void UpdateScale()
    {
        if(_isScaleCompleted)
            return;
        
        float scaleDelta = _startScaleFactor * Mathf.Sin(2 * Mathf.PI * ((_lifeTime - _timeToDisappear) / _scaleTime));
        _cachedTransform.localScale = new Vector3(1f + scaleDelta, 1f + scaleDelta, 0);

        if (_lifeTime - _timeToDisappear >= _scaleTime)
        {
            _isScaleCompleted = true;
        }
    }
}
