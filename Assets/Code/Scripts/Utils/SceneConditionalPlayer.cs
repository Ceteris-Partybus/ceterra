using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SceneConditionalPlayer : NetworkBehaviour {
    [SyncVar]
    [SerializeField]
    private int id = -1;
    public int Id {
        get => id;
        set {
            if (id == -1) {
                id = value;
            }
        }
    }

    private string playerName;
    public string PlayerName {
        get => playerName;
        set => playerName = value;
    }

    [SyncVar(hook = nameof(OnEnabledChanged))]
    [SerializeField]
    private bool isEnabled = true;

    public bool IsEnabled => isEnabled;
    public bool IsDisabled => !isEnabled;

    private static readonly Dictionary<GameObject, List<SceneConditionalPlayer>> playerScripts =
        new Dictionary<GameObject, List<SceneConditionalPlayer>>();

    private bool hasInitialized = false;
    private bool isWaitingForDataTransfer = false;

    private void OnEnabledChanged(bool _, bool newValue) {
        Debug.Log($"SceneConditionalPlayer {name} enabled state changed to {newValue}");
        enabled = newValue;
    }

    protected virtual void Start() {
        Recalculate();
    }

    public void Recalculate() {
        Debug.Log($"Recalculating SceneConditionalPlayer {name} for scene {NetworkManager.networkSceneName}");
        RegisterScript();

        if (IsEnabledForScene(NetworkManager.networkSceneName)) {
            Enable();
            StartCoroutine(InitializeWhenReady());
        }
        else {
            StartCoroutine(DisableWhenDataTransferred());
        }
    }

    private void RegisterScript() {
        Debug.Log($"Registering SceneConditionalPlayer {name} for scene {NetworkManager.networkSceneName}");
        if (!playerScripts.ContainsKey(gameObject)) {
            playerScripts[gameObject] = new List<SceneConditionalPlayer>();
        }
        playerScripts[gameObject].Add(this);
    }

    private void OnDestroy() {
        if (playerScripts.ContainsKey(gameObject)) {
            playerScripts[gameObject].Remove(this);
            if (playerScripts[gameObject].Count == 0) {
                playerScripts.Remove(gameObject);
            }
        }
    }

    private IEnumerator InitializeWhenReady() {
        yield return new WaitUntil(() => AreAllDataTransfersComplete());

        if (!hasInitialized) {
            hasInitialized = true;
            Initialize();

            OnScriptInitialized();
        }
    }

    private IEnumerator DisableWhenDataTransferred() {
        if (ShouldWaitForDataTransfer()) {
            isWaitingForDataTransfer = true;

            yield return new WaitUntil(() => HasEnabledScriptInitialized());

            yield return new WaitForEndOfFrame();
        }

        Disable();
    }

    private bool ShouldWaitForDataTransfer() {
        if (!playerScripts.ContainsKey(gameObject)) {
            return false;
        }

        return playerScripts[gameObject].Any(script =>
            script != this &&
            script.IsEnabledForScene(NetworkManager.networkSceneName));
    }

    private bool AreAllDataTransfersComplete() {
        if (!playerScripts.ContainsKey(gameObject)) {
            return true;
        }

        return playerScripts[gameObject]
            .Where(script => !script.IsEnabledForScene(NetworkManager.networkSceneName))
            .All(script => !script.isWaitingForDataTransfer);
    }

    private bool HasEnabledScriptInitialized() {
        if (!playerScripts.ContainsKey(gameObject)) {
            return false;
        }

        return playerScripts[gameObject].Any(script =>
            script != this &&
            script.IsEnabledForScene(NetworkManager.networkSceneName) &&
            script.hasInitialized);
    }

    private void OnScriptInitialized() {
        if (!playerScripts.ContainsKey(gameObject)) {
            return;
        }

        var scriptsToDisable = playerScripts[gameObject]
            .Where(script =>
                script != this &&
                !script.IsEnabledForScene(NetworkManager.networkSceneName) &&
                script.isWaitingForDataTransfer)
            .ToList();

        foreach (var script in scriptsToDisable) {
            script.InternalTransferDataTo(this);
            script.isWaitingForDataTransfer = false;
        }
    }

    private void InternalTransferDataTo(SceneConditionalPlayer enabledScript) {
        enabledScript.Id = Id;
        enabledScript.PlayerName = PlayerName;
        OnTransferDataTo(enabledScript);
    }

    /// <summary>
    /// Called when this script should transfer its data to the newly enabled script.
    /// Override this method to implement data transfer logic.
    /// The id and playerName properties are automatically transferred internally.
    /// </summary>
    /// <param name="enabledScript">The script that is being enabled and should receive the data</param>
    protected abstract void OnTransferDataTo(SceneConditionalPlayer enabledScript);

    /// <summary>
    /// Called when this script is being disabled to reset/cleanup its state.
    /// Override this method to implement custom cleanup logic (e.g., reset scores, clear temporary data).
    /// This is the opposite of Initialize() and should restore the script to a clean state.
    /// </summary>
    protected abstract void Cleanup();

    /// <summary>
    /// Called when this script is initializing and can receive data from other scripts.
    /// Override this method to implement custom initialization logic.
    /// </summary>
    protected abstract void Initialize();

    public abstract bool IsEnabledForScene(string sceneName);

    private void Enable() {
        if (isEnabled) {
            return;
        }

        isEnabled = true;
    }

    private void Disable() {
        if (!isEnabled) {
            return;
        }

        Cleanup();
        hasInitialized = false;
        isEnabled = false;
    }
}