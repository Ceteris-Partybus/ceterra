using UnityEngine;
using UnityEngine.UIElements;

public class ResourceModal : Modal {

    private Button resourcesHistoryButton;
    private VisualTreeAsset resourceHistoryModalTemplate;

    public ResourceModal(VisualTreeAsset contentTemplate, VisualTreeAsset resourceHistoryModalTemplate) : base(contentTemplate) {
        this.resourceHistoryModalTemplate = resourceHistoryModalTemplate;
    }

    protected override void InitializeContent() {
        this.resourcesHistoryButton = this.modalContent.Q<Button>("resources-history-button");

        if (this.resourcesHistoryButton != null) {
            this.resourcesHistoryButton.clicked += this.OnResourcesHistoryButtonClicked;
        }
    }

    private void OnResourcesHistoryButtonClicked() {
        Debug.Log("Action button in Modal 1 was clicked!");
        ModalManager.Instance.ShowModal(new ResourceHistoryModal(this.resourceHistoryModalTemplate));
    }

    protected override void OnClose() {
        // Unregister events when modal is closed
        if (this.resourcesHistoryButton != null) {
            this.resourcesHistoryButton.clicked -= this.OnResourcesHistoryButtonClicked;
        }
    }
}

