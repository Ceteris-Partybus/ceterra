using UnityEngine.UIElements;

public abstract class Modal {
    protected VisualElement root;
    protected VisualElement modalContent;
    public VisualElement ModalContent => modalContent;
    protected Button closeButton;
    protected VisualTreeAsset contentTemplate;

    public Modal(VisualTreeAsset contentTemplate) {
        this.contentTemplate = contentTemplate;
    }

    public virtual void Initialize(VisualElement container) {
        // Create the modal background
        root = new VisualElement();
        root.name = "modal-background";
        root.AddToClassList("modal-background");

        // Create the modal container
        var modalContainer = new VisualElement();
        modalContainer.name = "modal-box";
        modalContainer.AddToClassList("modal-box");
        root.Add(modalContainer);

        // Create header with close button
        var header = new VisualElement();
        header.name = "modal-header";
        header.AddToClassList("modal-header");
        modalContainer.Add(header);

        closeButton = new Button(() => ModalManager.Instance.CloseModal(this));
        closeButton.text = "X";
        closeButton.name = "modal-close-button";
        closeButton.AddToClassList("modal-close-button");
        header.Add(closeButton);

        // Create content container
        var contentContainer = new VisualElement();
        contentContainer.name = "modal-content-container";
        contentContainer.AddToClassList("modal-content-container");
        modalContainer.Add(contentContainer);

        // Instantiate the content template
        modalContent = contentTemplate.Instantiate();
        modalContent.AddToClassList("modal-content");
        contentContainer.Add(modalContent);

        // Add the root to the container
        container.Add(root);

        // Initialize content
        InitializeContent();

        // Hide initially
        root.style.display = DisplayStyle.None;
    }

    public virtual void Show() {
        root.style.display = DisplayStyle.Flex;
        OnShow();
    }

    public virtual void Close() {
        OnClose();
        if (root != null && root.parent != null) {
            root.parent.Remove(root);
        }
    }

    // Override this method to initialize content
    protected abstract void InitializeContent();

    // Override these methods to add custom show/close behavior
    protected virtual void OnShow() { }
    protected virtual void OnClose() { }
}

