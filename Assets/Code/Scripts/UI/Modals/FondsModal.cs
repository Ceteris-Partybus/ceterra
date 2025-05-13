using UnityEngine;
using UnityEngine.UIElements;

public class FondsModal : Modal {
    public FondsModal(VisualTreeAsset contentTemplate) : base(contentTemplate) {
    }

    protected override void InitializeContent() {
        Button confirmButton = modalContent.Q<Button>("btn-confirm");

        if (confirmButton != null) {
            confirmButton.clicked += OnConfirmButtonClicked;
        }
    }

    private void OnConfirmButtonClicked() {
        Debug.Log("Confirm button in Modal 2 was clicked!");
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
