using System;
using DG.Tweening;
using TMPro;

public static class TextMeshTweenExtension
{
    private static Tweener DoTextInt(this TextMeshProUGUI text, int initialValue, int finalValue, float duration, Func<int, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration);
    }

    public static Tweener DoTextInt(this TextMeshProUGUI text, int initialValue, int finalValue, float duration)
    {
        return DoTextInt(text, initialValue, finalValue, duration, it => it.ToString());
    }

    private static Tweener DoTextFloat(this TextMeshProUGUI text, float initialValue, float finalValue, float duration, Func<float, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration);
    }

    public static Tweener DoTextFloat(this TextMeshProUGUI text, float initialValue, float finalValue, float duration)
    {
        return DoTextFloat(text, initialValue, finalValue, duration, it => it.ToString());
    }

    private static Tweener DoTextLong(this TextMeshProUGUI text, long initialValue, long finalValue, float duration, Func<long, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration);
    }

    public static Tweener DoTextLong(this TextMeshProUGUI text, long initialValue, long finalValue, float duration)
    {
        return DoTextLong(text, initialValue, finalValue, duration, it => it.ToString());
    }

    private static Tweener DoTextDouble(this TextMeshProUGUI text, double initialValue, double finalValue, float duration, Func<double, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration);
    }

    public static Tweener DoTextDouble(this TextMeshProUGUI text, double initialValue, double finalValue, float duration)
    {
        return DoTextDouble(text, initialValue, finalValue, duration, it => it.ToString());
    }
}
