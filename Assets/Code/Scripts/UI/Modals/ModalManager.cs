using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ModalManager : NetworkedSingleton<ModalManager> {
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int maxModalStackSize = 10;

    private Stack<Modal> modalStack = new Stack<Modal>();
    public Stack<Modal> ModalStack => modalStack;
    private VisualElement rootElement;

    protected override void Awake() {
        base.Awake();

        if (uiDocument == null) {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    protected override void Start() {
        base.Start();
        rootElement = uiDocument.rootVisualElement.Q<VisualElement>("modal-container");
    }

    protected void Update() {
        Modal modal = GetTopModal();

        if (modal == null) {
            return;
        }

        if (modal.IsVisible() && modal.CloseOnEscapeKey && Input.GetKeyDown(KeyCode.Escape)) {
            Hide();
        }
    }

    /// <summary>
    /// Shows a modal either on all clients or just the calling client
    /// </summary>
    /// <param name="modal">The modal to show</param>
    /// <param name="showOnAllClients">If true, shows on all clients. If false, shows only on the calling client.</param>
    public void Show(Modal modal, bool showOnAllClients = false) {
        if (modal == null) {
            Debug.LogError("Cannot show null modal!");
            return;
        }

        if (showOnAllClients) {
            // Show on all clients via RPC
            if (isServer) {
                RpcShowModal(modal.GetModalId());
            }
            else {
                // If we're a client, ask the server to broadcast
                CmdRequestShowModalOnAllClients(modal.GetModalId());
            }
        }
        else {
            // Show only on this client
            ShowModalLocal(modal);
        }
    }

    /// <summary>
    /// Hides the top modal from the stack
    /// </summary>
    /// <param name="hideOnAllClients">If true, hides on all clients. If false, hides only on the calling client.</param>
    public void Hide(bool hideOnAllClients = false) {
        if (hideOnAllClients) {
            if (isServer) {
                RpcHideTopModal();
            }
            else {
                CmdRequestHideModalOnAllClients();
            }
        }
        else {
            HideTopModalLocal();
        }
    }

    /// <summary>
    /// Shows a modal locally on this client only
    /// </summary>
    private void ShowModalLocal(Modal modal) {
        if (modalStack.Count >= maxModalStackSize) {
            Debug.LogWarning($"Modal stack is full! Maximum size: {maxModalStackSize}");
            return;
        }

        // Hide the current top modal if there is one
        // if (modalStack.Count > 0) {
        //     modalStack.Peek().SetVisible(false);
        // }

        // Show the modal container
        if (rootElement != null) {
            rootElement.AddToClassList("active");
            rootElement.style.display = DisplayStyle.Flex;
        }

        // Add new modal to stack and show it
        modalStack.Push(modal);
        modal.Show(rootElement);

        Debug.Log($"Showing modal: {modal.GetType().Name}");
    }

    /// <summary>
    /// Hides the top modal locally on this client only
    /// </summary>
    private void HideTopModalLocal() {
        if (modalStack.Count == 0) {
            Debug.LogWarning("No modals to hide!");
            return;
        }

        Modal topModal = modalStack.Pop();
        topModal.Hide();

        Debug.Log($"Hiding modal: {topModal.GetType().Name}");

        // Show the previous modal if there is one
        if (modalStack.Count > 0) {
            modalStack.Peek().SetVisible(true);
        }
        else {
            // Hide the modal container if no modals are left
            if (rootElement != null) {
                rootElement.RemoveFromClassList("active");
                rootElement.style.display = DisplayStyle.None;
            }
        }
    }

    // Network Commands (Client -> Server)
    [Command(requiresAuthority = false)]
    private void CmdRequestShowModalOnAllClients(string modalId) {
        Modal modal = FindModalById(modalId);
        if (modal != null) {
            RpcShowModal(modalId);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestHideModalOnAllClients() {
        RpcHideTopModal();
    }

    // Network RPCs (Server -> All Clients)
    [ClientRpc]
    private void RpcShowModal(string modalId) {
        Modal modal = FindModalById(modalId);
        if (modal != null) {
            ShowModalLocal(modal);
        }
        else {
            Debug.LogError($"Modal with ID '{modalId}' not found!");
        }
    }

    [ClientRpc]
    private void RpcHideTopModal() {
        HideTopModalLocal();
    }

    /// <summary>
    /// Finds a modal by its ID. You'll need to implement your own modal registration system.
    /// </summary>
    private Modal FindModalById(string modalId) {
        // This is a simple implementation - you might want a more sophisticated system
        Modal[] allModals = FindObjectsByType<Modal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Modal modal in allModals) {
            if (modal.GetModalId() == modalId) {
                return modal;
            }
        }
        return null;
    }

    /// <summary>
    /// Clears all modals from the stack
    /// </summary>
    public void ClearAllModals() {
        while (modalStack.Count > 0) {
            Modal modal = modalStack.Pop();
            modal.Hide();
        }

        // Hide the modal container
        if (rootElement != null) {
            rootElement.RemoveFromClassList("active");
            rootElement.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// Gets the number of modals currently in the stack
    /// </summary>
    public int GetModalCount() {
        return modalStack.Count;
    }

    /// <summary>
    /// Gets the top modal without removing it from the stack
    /// </summary>
    public Modal GetTopModal() {
        return modalStack.Count > 0 ? modalStack.Peek() : null;
    }
}
