using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System;

public static class UIAnimationUtils {
    public const float SLIDE_OUT_POSITION = -300f;
    public const float BUTTON_FINAL_POSITION = -10f;
    public const float INFO_PANEL_FINAL_POSITION = -40f;

    public static Sequence SlideInFromLeft(VisualElement element, float targetLeft, float duration = .6f, Ease easing = Ease.OutBack, Action onComplete = null) {
        element.style.display = DisplayStyle.Flex;
        element.style.left = SLIDE_OUT_POSITION;
        element.style.opacity = 0f;

        return DOTween.Sequence()
            .Join(DOTween.To(() => element.style.left.value.value, x => element.style.left = x, targetLeft, duration).SetEase(easing))
            .Join(DOTween.To(() => element.style.opacity.value, x => element.style.opacity = x, 1f, duration).SetEase(Ease.OutCubic))
            .OnComplete(() => element.pickingMode = PickingMode.Position)
            .OnComplete(() => onComplete?.Invoke());
    }

    public static Sequence SlideOutToLeft(VisualElement element, float duration = .3f, float delayBeforeHide = .15f, Action onComplete = null) {
        element.pickingMode = PickingMode.Ignore;

        return DOTween.Sequence()
             .Join(DOTween.To(() => element.style.left.value.value, x => element.style.left = x, SLIDE_OUT_POSITION, duration).SetEase(Ease.InCubic))
             .Join(DOTween.To(() => element.style.opacity.value, x => element.style.opacity = x, 0f, duration).SetEase(Ease.InCubic))
             .AppendInterval(delayBeforeHide)
             .OnComplete(() => element.style.display = DisplayStyle.None)
             .OnComplete(() => onComplete?.Invoke());
    }

    public static Sequence AnimateScale(VisualElement element, float fromScale, float toScale, float duration = .8f, Ease easing = Ease.OutBack, Action onComplete = null) {
        element.style.scale = new Scale(Vector3.one * fromScale);

        var currentScale = fromScale;
        return DOTween.Sequence()
            .Append(DOTween.To(() => currentScale, x => { currentScale = x; element.style.scale = new Scale(Vector3.one * x); }, toScale, duration).SetEase(easing))
            .OnComplete(() => onComplete?.Invoke());
    }

    public static Sequence AnimateFade(VisualElement element, float fromOpacity, float toOpacity, float duration = .6f, Ease easing = Ease.OutCubic, Action onComplete = null) {
        element.style.opacity = fromOpacity;

        return DOTween.Sequence()
            .Append(DOTween.To(() => element.style.opacity.value, x => element.style.opacity = x, toOpacity, duration).SetEase(easing))
            .OnComplete(() => onComplete?.Invoke());
    }

    public static Sequence ScaleAndFadeIn(VisualElement element, float fromScale = .5f, float toScale = 1f, float duration = .8f, Ease scaleEasing = Ease.OutBack, Ease fadeEasing = Ease.OutCubic, Action onComplete = null) {
        element.style.scale = new Scale(Vector3.one * fromScale);
        element.style.opacity = 0f;

        var currentScale = fromScale;
        return DOTween.Sequence()
            .Join(DOTween.To(() => currentScale, x => { currentScale = x; element.style.scale = new Scale(Vector3.one * x); }, toScale, duration).SetEase(scaleEasing))
            .Join(DOTween.To(() => element.style.opacity.value, x => element.style.opacity = x, 1f, duration).SetEase(fadeEasing))
            .OnComplete(() => onComplete?.Invoke());
    }
}