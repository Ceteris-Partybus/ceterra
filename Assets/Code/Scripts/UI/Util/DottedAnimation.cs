using UnityEngine.UIElements;

public class DottedAnimation {
    private Label label;
    private string initialText;
    private int duration;

    private IVisualElementScheduledItem animation;
    private int dotCount = 0;

    public bool isRunning => animation != null;

    public DottedAnimation(Label label, string initialText, int duration = 500) {
        this.label = label;
        this.initialText = initialText;
        this.duration = duration;
    }

    public void Start() {
        dotCount = 0;
        animation = label.schedule.Execute(Animate).Every(duration);
    }

    public void Stop() {
        animation?.Pause();
        animation = null;
    }

    private void Animate() {
        dotCount = (dotCount + 1) % 4;
        label.text = initialText + new string('.', dotCount);
    }
}