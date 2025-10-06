using Edgegap;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A NetworkBehaviour that manages player components based on the current scene.
/// Only one component per GameObject is active at a time, with automatic data transfer.
/// </summary>
public abstract class SceneConditionalPlayer : NetworkBehaviour {

    #region Core Properties
    [SyncVar]
    [SerializeField]
    private int playerId = -1;
    public int PlayerId {
        get => playerId;
        private set => playerId = value;
    }

    [SyncVar]
    [SerializeField]
    private string playerName = "";
    public string PlayerName {
        get => playerName;
        private set => playerName = value;
    }

    [SyncVar(hook = nameof(OnActiveStateChanged))]
    private bool isActiveForCurrentScene = false;
    public bool IsActiveForCurrentScene => isActiveForCurrentScene;
    #endregion

    #region Server-Only State
    private static Dictionary<GameObject, ScenePlayerManager> playerManagers =
        new Dictionary<GameObject, ScenePlayerManager>();

    private bool isInitialized = false;
    #endregion

    #region Lifecycle
    public override void OnStartServer() {
        base.OnStartServer();
        GetOrCreateManager().RegisterComponent(this);
    }

    public override void OnStopServer() {
        base.OnStopServer();
        if (playerManagers.ContainsKey(gameObject)) {
            playerManagers[gameObject].UnregisterComponent(this);
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();
        // Apply initial state on clients
        OnActiveStateChanged(false, isActiveForCurrentScene);
    }
    #endregion

    #region Server Management
    [Server]
    private ScenePlayerManager GetOrCreateManager() {
        if (!playerManagers.ContainsKey(gameObject)) {
            playerManagers[gameObject] = new ScenePlayerManager(gameObject);
        }
        return playerManagers[gameObject];
    }

    [Server]
    public void HandleSceneChange(string newSceneName) {
        GetOrCreateManager().HandleSceneChange(newSceneName);
    }

    [Server]
    internal void SetActiveState(bool active) {
        if (isActiveForCurrentScene == active) {
            return;
        }

        isActiveForCurrentScene = active;

        if (active) {
            if (!isInitialized) {
                OnServerInitialize();
                isInitialized = true;
            }
        }
        else {
            if (isInitialized) {
                OnServerCleanup();
                isInitialized = false;
            }
        }
    }

    [Server]
    internal void TransferDataFrom(SceneConditionalPlayer source) {
        // Transfer core data
        PlayerId = source.PlayerId;
        PlayerName = source.PlayerName;

        // Transfer custom data
        OnServerReceiveData(source);
    }

    [Server]
    public virtual void SetPlayerData(int id, string name) {
        if (PlayerId == -1) {
            PlayerId = id;
        }

        PlayerName = name;
    }
    #endregion

    public void Hide() {
    }

    public void Show() {
    }

    #region Client Sync
    private void OnActiveStateChanged(bool oldValue, bool newValue) {
        enabled = newValue;
        OnClientActiveStateChanged(newValue);
        Debug.Log($"[Client] {GetType().Name} on {name} is now {(newValue ? "active" : "inactive")}");
    }
    #endregion

    #region Abstract/Virtual Methods
    /// <summary>
    /// Determines if this component should be active for the given scene
    /// </summary>
    public abstract bool ShouldBeActiveInScene(string sceneName);

    /// <summary>
    /// Called on server when this component becomes active
    /// </summary>
    [Server]
    protected virtual void OnServerInitialize() { }

    /// <summary>
    /// Called on server when this component becomes inactive
    /// </summary>
    [Server]
    protected virtual void OnServerCleanup() { }

    /// <summary>
    /// Called on server to receive data from previously active component
    /// </summary>
    [Server]
    protected virtual void OnServerReceiveData(SceneConditionalPlayer source) { }

    /// <summary>
    /// Called on client when active state changes
    /// </summary>
    protected virtual void OnClientActiveStateChanged(bool isActive) { }
    #endregion
}

/// <summary>
/// Server-only manager that handles component switching for a single GameObject
/// </summary>
internal class ScenePlayerManager {
    private readonly GameObject gameObject;
    private readonly List<SceneConditionalPlayer> components = new List<SceneConditionalPlayer>();
    private SceneConditionalPlayer activeComponent;
    private string currentScene;

    public ScenePlayerManager(GameObject gameObject) {
        this.gameObject = gameObject;
        this.currentScene = NetworkManager.networkSceneName;
    }

    public void RegisterComponent(SceneConditionalPlayer component) {
        if (!components.Contains(component)) {
            components.Add(component);
            Debug.Log($"[Server] Registered {component.GetType().Name} on {gameObject.name}");

            // Check if this component should be active in current scene
            UpdateActiveComponent();
        }
    }

    public void UnregisterComponent(SceneConditionalPlayer component) {
        components.Remove(component);
        if (activeComponent == component) {
            activeComponent = null;
        }
    }

    public void HandleSceneChange(string newSceneName) {
        if (currentScene == newSceneName) {
            return;
        }

        string previousScene = currentScene;
        currentScene = newSceneName;

        Debug.Log($"[Server] Scene changed from {previousScene} to {newSceneName} for {gameObject.name}");
        UpdateActiveComponent();
    }

    private void UpdateActiveComponent() {
        // Find component that should be active in current scene
        var targetComponent = components.FirstOrDefault(c => c.ShouldBeActiveInScene(currentScene));

        if (targetComponent == activeComponent) {
            return; // No change needed
        }

        // Transfer data from old to new component
        if (activeComponent != null && targetComponent != null) {
            Debug.Log($"[Server] Transferring data from {activeComponent.GetType().Name} to {targetComponent.GetType().Name}");
            targetComponent.TransferDataFrom(activeComponent);
        }

        // Deactivate old component
        if (activeComponent != null) {
            activeComponent.SetActiveState(false);
        }

        // Activate new component
        activeComponent = targetComponent;
        if (activeComponent != null) {
            activeComponent.SetActiveState(true);
        }

        Debug.Log($"[Server] Active component for {gameObject.name} is now {activeComponent?.GetType().Name ?? "none"}");
    }
}