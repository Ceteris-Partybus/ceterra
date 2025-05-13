using UnityEngine;
using UnityEngine.UIElements;

public class InvestModal : Modal {
    public InvestModal(VisualTreeAsset contentTemplate) : base(contentTemplate) {
    }

    protected override void InitializeContent() {
        Button confirmButton = modalContent.Q<Button>("btn-confirm");

        if (confirmButton != null) {
            confirmButton.clicked += OnConfirmButtonClicked;
        }
    }

    private void OnConfirmButtonClicked() {
        Debug.Log("Confirm button in Modal 3 was clicked!");
        // You can trigger other actions here
    }

    protected override void OnClose() {
        // Unregister events when modal is closed
        Button confirmButton = modalContent.Q<Button>("btn-confirm");
        if (confirmButton != null) {
            confirmButton.clicked -= OnConfirmButtonClicked;
        }
    }
}
