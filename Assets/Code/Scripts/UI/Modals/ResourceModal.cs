using UnityEngine;
using UnityEngine.UIElements;

public class ResourceModal : Modal {
    public ResourceModal(VisualTreeAsset contentTemplate) : base(contentTemplate) {
    }

    protected override void InitializeContent() {
        Button actionButton = modalContent.Q<Button>("btn-action");

        if (actionButton != null) {
            actionButton.clicked += OnActionButtonClicked;
        }
    }

    private void OnActionButtonClicked() {
        Debug.Log("Action button in Modal 1 was clicked!");
        // You can trigger other actions here
    }

    protected override void OnClose() {
        // Unregister events when modal is closed
        Button actionButton = modalContent.Q<Button>("btn-action");
        if (actionButton != null) {
            actionButton.clicked -= OnActionButtonClicked;
        }
    }
}

