using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class NewModal : NetworkBehaviour {
    [Header("Modal Settings")]
    [SerializeField] protected VisualTreeAsset visualTreeAsset;
    [SerializeField] protected string modalId;
    [SerializeField] protected bool closeOnBackgroundClick = true;
    [SerializeField] protected bool closeOnEscapeKey = true;

    protected VisualElement modalElement;
    protected VisualElement backgroundElement;
    private bool isVisible = false;

    protected virtual void Start() {
        // Generate a unique modal ID if not set
        if (string.IsNullOrEmpty(modalId)) {
            modalId = GetType().Name + "_" + GetInstanceID();
        }

        // Validate required components
        if (visualTreeAsset == null) {
            Debug.LogError($"Modal '{GetType().Name}' is missing a VisualTreeAsset!");
        }
    }

    protected virtual void Update() {
        if (isVisible && closeOnEscapeKey && Input.GetKeyDown(KeyCode.Escape)) {
            OnCloseRequested();
        }
    }

    /// <summary>
    /// Shows the modal on screen
    /// </summary>
    public virtual void Show(VisualElement parentElement) {
        if (visualTreeAsset == null || parentElement == null) {
            Debug.LogError($"Cannot show modal '{GetType().Name}' - missing VisualTreeAsset or parent element!");
            return;
        }

        // Create the modal UI from the template
        modalElement = visualTreeAsset.Instantiate();
        parentElement.Add(modalElement);

        // Setup background click detection if enabled
        if (closeOnBackgroundClick) {
            SetupBackgroundClickDetection();
        }

        // Setup close button if it exists
        SetupCloseButton();

        SetVisible(true);
        OnModalShown();
    }

    /// <summary>
    /// Hides the modal from screen
    /// </summary>
    public virtual void Hide() {
        if (modalElement != null) {
            modalElement.RemoveFromHierarchy();
            modalElement = null;
        }

        SetVisible(false);
        OnModalHidden();
    }

    /// <summary>
    /// Sets the visibility without affecting the UI hierarchy
    /// </summary>
    public virtual void SetVisible(bool visible) {
        isVisible = visible;

        if (modalElement != null) {
            modalElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    /// <summary>
    /// Gets the unique identifier for this modal
    /// </summary>
    public string GetModalId() {
        return modalId;
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
        NewModalManager.Instance?.Hide();
    }

    /// <summary>
    /// Sets up background click detection for closing the modal
    /// </summary>
    private void SetupBackgroundClickDetection() {
        if (modalElement == null) {
            return;
        }

        backgroundElement = modalElement.Q("background");
        if (backgroundElement != null) {
            backgroundElement.RegisterCallback<ClickEvent>(OnBackgroundClicked);
        }
    }

    /// <summary>
    /// Sets up the close button if it exists in the UXML
    /// </summary>
    private void SetupCloseButton() {
        if (modalElement == null) {
            return;
        }

        Button closeButton = modalElement.Q<Button>("close-button");
        if (closeButton != null) {
            closeButton.clicked += OnCloseRequested;
        }
    }

    /// <summary>
    /// Handles background click events
    /// </summary>
    private void OnBackgroundClicked(ClickEvent evt) {
        // Only close if the click was directly on the background, not a child element
        if (evt.target == backgroundElement) {
            OnCloseRequested();
        }
    }

    /// <summary>
    /// Gets whether the modal is currently visible
    /// </summary>
    public bool IsVisible() {
        return isVisible;
    }

    protected virtual void OnDestroy() {
        Hide();
    }
}