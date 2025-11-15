using UnityEngine.UIElements;

public class DottedAnimation {
    private TextElement element;
    private string initialText;
    private int duration;

    private IVisualElementScheduledItem animation;
    private int dotCount = 0;

    public bool isRunning => animation != null;

    public DottedAnimation(TextElement element, string initialText, int duration = 500) {
        this.element = element;
        this.initialText = initialText;
        this.duration = duration;
    }

    public void Start() {
        dotCount = 0;
        animation = element.schedule.Execute(Animate).Every(duration);
    }

    public void Stop() {
        animation?.Pause();
        animation = null;
    }

    private void Animate() {
        dotCount = (dotCount + 1) % 4;
        element.text = initialText + new string('.', dotCount);
    }
}