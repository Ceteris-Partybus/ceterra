using UnityEngine.UIElements;

public class IconWithLabel : VisualElement {
    private VisualElement icon;
    private Label label;
    private DottedAnimation selectingAnimation;

    public IconWithLabel(VisualElement parent, string iconElementName, string iconLabelName) {
        icon = parent.Q<VisualElement>(iconElementName);
        label = parent.Q<Label>(iconLabelName);

        selectingAnimation = new DottedAnimation(label, "Selecting");
        selectingAnimation.Start();
    }

    public void SetIconAndLabel(StyleBackground background, string text) {
        if (selectingAnimation.isRunning) { selectingAnimation.Stop(); }
        icon.style.backgroundImage = background;
        label.text = text;
    }
}