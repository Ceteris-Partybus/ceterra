using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ModalManager : NetworkedSingleton<ModalManager> {
    [SerializeField] private UIDocument uiDocument;
    private VisualElement modalContainer;
    private readonly List<Modal> activeModals = new List<Modal>();

    protected override void Awake() {
        base.Awake();

        if (this.uiDocument == null) {
            this.uiDocument = this.GetComponent<UIDocument>();
        }

        // Get the modal container from the UI Document
        this.modalContainer = this.uiDocument.rootVisualElement.Q<VisualElement>("modal-container");
        if (this.modalContainer == null) {
            Debug.LogError("Modal container not found in UI Document. Make sure there's a VisualElement with name 'modal-container'");
        }
    }

    public void ShowModal(Modal modal) {
        if (this.modalContainer == null) {
            Debug.LogError("Modal container not found");
            return;
        }

        Debug.Log("Showing modal: " + modal.GetType().Name);

        // Initialize the modal
        modal.Initialize(this.modalContainer);

        // Check if the modal container is already active
        if (!this.IsModalContainerActive()) {
            this.ToggleModalContainer();
        }

        // Add the modal to the active modals list
        this.activeModals.Add(modal);

        // Show the modal
        modal.Show();
    }

    private bool IsModalContainerActive() {
        return this.modalContainer != null && this.modalContainer.ClassListContains("active");
    }

    private void ToggleModalContainer() {
        this.modalContainer?.ToggleInClassList("active");
    }

    public void CloseModal(Modal modal) {
        if (this.activeModals.Contains(modal)) {
            modal.Close();
            this.activeModals.Remove(modal);
        }
        if (this.activeModals.Count == 0) {
            if (this.IsModalContainerActive()) {
                this.ToggleModalContainer();
            }
        }
    }

    public void CloseAllModals() {
        foreach (var modal in this.activeModals.ToArray()) {
            modal.Close();
        }
        this.activeModals.Clear();
        if (this.IsModalContainerActive()) {
            this.ToggleModalContainer();
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (this.activeModals.Count > 0) {
                this.CloseModal(this.activeModals[this.activeModals.Count - 1]);
            }
        }
    }
}
