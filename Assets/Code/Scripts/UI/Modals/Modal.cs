using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Modal : NetworkBehaviour {
    [Header("Modal Settings")]
    [SerializeField] protected VisualTreeAsset visualTreeAsset;
    [SerializeField] protected string modalId;
    [SerializeField] protected bool closeOnBackgroundClick = true;
    [SerializeField] protected bool closeOnEscapeKey = true;
    public bool CloseOnEscapeKey => closeOnEscapeKey;
    protected bool showCloseButton = true;
    protected bool showModalTypeInHeader = false;
    private static readonly Dictionary<System.Type, Modal> instances = new Dictionary<System.Type, Modal>();
    private static readonly object lockObj = new();

    public static T GetInstance<T>() where T : Modal {
        lock (lockObj) {
            if (instances.TryGetValue(typeof(T), out var instance)) {
                return instance as T;
            }
            return null;
        }
    }

    protected virtual void Awake() {
        var type = GetType();
        lock (lockObj) {
            if (!instances.ContainsKey(type)) {
                instances[type] = this;
            }
            else if (instances[type] != this) {
                Destroy(gameObject);
                return;
            }
        }
    }

    protected virtual void OnDestroy() {
        var type = GetType();
        lock (lockObj) {
            if (instances.TryGetValue(type, out var instance) && instance == this) {
                instances.Remove(type);
            }
        }
        Hide();
    }

    protected VisualElement modalElement;
    public VisualElement ModalElement => modalElement;
    protected VisualElement modalBoxWrapper;
    protected VisualElement modalBackground;
    protected VisualElement backgroundElement;
    protected Button closeButton;
    private bool isVisible = false;

    protected virtual void Start() {
        // Generate a unique modal ID if not set
        if (string.IsNullOrEmpty(modalId)) {
            modalId = GetType().Name;
        }

        // Validate required components
        if (visualTreeAsset == null) {
            Debug.LogError($"Modal '{GetType().Name}' is missing a VisualTreeAsset!");
        }
    }

    // Currently being handled by the ModalManager to only close the top modal on ESC 
    // protected virtual void Update() {
    //     if (isVisible && closeOnEscapeKey && Input.GetKeyDown(KeyCode.Escape)) {
    //         OnCloseRequested();
    //     }
    // }

    /// <summary>
    /// Shows the modal on screen
    /// </summary>
    public virtual void Show(VisualElement parentElement) {
        if (visualTreeAsset == null) {
            Debug.LogError($"Cannot show modal '{GetType().Name}' - missing VisualTreeAsset!");
            return;
        }

        if (parentElement == null) {
            Debug.LogError($"Cannot show modal '{GetType().Name}' - parentElement is null!");
            return;
        }

        // Clean up any existing modal first
        if (modalBackground != null) {
            Hide();
        }

        // Create the modal background (for overlay and click detection)
        modalBackground = new VisualElement();
        modalBackground.name = "modal-background";
        modalBackground.AddToClassList("modal-background");

        // Create the modal box container
        modalBoxWrapper = new VisualElement();
        modalBoxWrapper.name = "modal-box";
        modalBoxWrapper.AddToClassList("modal-box");
        modalBackground.Add(modalBoxWrapper);

        // Create header with close button

        if (showCloseButton || showModalTypeInHeader) {
            var header = new VisualElement();
            header.name = "modal-header";
            header.AddToClassList("modal-header");
            modalBoxWrapper.Add(header);

            if (showModalTypeInHeader) {
                var titleLabel = new Label(modalId);
                titleLabel.name = "modal-header-title";
                titleLabel.text = GetHeaderTitle();
                titleLabel.AddToClassList("modal-header-title");
                header.Add(titleLabel);
            }
            if (showCloseButton) {
                closeButton = new Button(() => {
                    Audiomanager.Instance?.PlayClickSound();
                    OnCloseRequested();
                });
                closeButton.text = "×";
                closeButton.name = "close-button";
                closeButton.AddToClassList("modal-close-button");
                if (!showModalTypeInHeader) {
                    header.AddToClassList("no-title");
                }
                header.Add(closeButton);
            }
        }

        // Create content container
        var contentContainer = new VisualElement();
        contentContainer.name = "modal-content-container";
        contentContainer.AddToClassList("modal-content-container");
        modalBoxWrapper.Add(contentContainer);

        // Create the modal UI from the template and add to content container
        modalElement = visualTreeAsset.Instantiate();
        modalElement.AddToClassList("modal-content");
        contentContainer.Add(modalElement);

        // Add the background to the parent (modal-container)
        parentElement.Add(modalBackground);

        // Setup background click detection if enabled
        if (closeOnBackgroundClick) {
            SetupBackgroundClickDetection();
        }

        SetVisible(true);
        BoardOverlay.Instance?.FillModalsWithCurrentValues();
        OnModalShown();
    }

    /// <summary>
    /// Hides the modal from screen
    /// </summary>
    public virtual void Hide() {
        if (modalBackground != null) {
            modalBackground.RemoveFromHierarchy();
            modalBackground = null;
        }

        modalBoxWrapper = null;
        modalElement = null;
        closeButton = null;
        SetVisible(false);
        OnModalHidden();
    }    /// <summary>
         /// Sets the visibility without affecting the UI hierarchy
         /// </summary>
    public virtual void SetVisible(bool visible) {
        isVisible = visible;

        if (modalBackground != null) {
            modalBackground.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    /// <summary>
    /// Gets the unique identifier for this modal
    /// </summary>
    public string GetModalId() {
        return modalId;
    }

    protected virtual string GetHeaderTitle() {
        return "";
    }

    /// <summary>
    /// Called when the modal is shown
    /// </summary>
    protected virtual void OnModalShown() {
        // Override in derived classes for custom behavior
    }

    /// <summary>
    /// Called when the modal is hidden
    /// </summary>
    protected virtual void OnModalHidden() {
        // Override in derived classes for custom behavior
    }

    /// <summary>
    /// Called when the user requests to close the modal
    /// </summary>
    protected virtual void OnCloseRequested() {
        ModalManager.Instance?.Hide();
    }

    /// <summary>
    /// Sets up background click detection for closing the modal
    /// </summary>
    private void SetupBackgroundClickDetection() {
        if (modalBackground == null) {
            return;
        }

        // Use the modal background itself for click detection
        modalBackground.RegisterCallback<ClickEvent>(OnBackgroundClicked);
    }

    /// <summary>
    /// Handles background click events
    /// </summary>
    private void OnBackgroundClicked(ClickEvent evt) {
        // Only close if the click was directly on the modal background, not a child element
        if (evt.target == modalBackground) {
            OnCloseRequested();
        }
    }

    /// <summary>
    /// Gets whether the modal is currently visible
    /// </summary>
    public bool IsVisible() {
        return isVisible;
    }
}