using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour {
    public NetworkVariable<Vector3> position = new();
    public NetworkVariable<int> health = new(42);
    public NetworkVariable<int> coins = new(100);

    private BoardOverlay boardOverlay;

    public override void OnNetworkSpawn() {
        boardOverlay = FindFirstObjectByType<BoardOverlay>();

        if (IsOwner) {
            health.OnValueChanged += OnHealthChanged;
            coins.OnValueChanged += OnCoinsChanged;

            if (boardOverlay != null) {
                // Initialize the overlay with the default values
                boardOverlay.SetCurrentPlayerName($"Player {NetworkObjectId}");
                boardOverlay.SetCurrentPlayerHealth(health.Value);
                boardOverlay.SetCurrentPlayerCoins(coins.Value);
            }
        }
        else {
            boardOverlay.AddPlayerToOverview(this);
        }
    }

    public override void OnNetworkDespawn() {
        if (IsOwner) {
            health.OnValueChanged -= OnHealthChanged;
            coins.OnValueChanged -= OnCoinsChanged;
        }
    }

    private void OnHealthChanged(int previousValue, int newValue) {
        if (boardOverlay != null) {
            if (IsOwner) {
                boardOverlay.SetCurrentPlayerHealth(newValue);
            }
        }
    }

    public void OnOtherHealthChanged(int newValue, ulong playerId) {
        Debug.Log($"[NetworkPlayer:46] OnOtherHealthChanged called with newValue: {newValue}, playerId: {playerId}");
        if (boardOverlay != null) {
            Debug.Log($"[NetworkPlayer:48] Updating health for player {playerId} to {newValue}");
            boardOverlay.SetPlayerHealth(newValue, playerId);
        }
    }

    private void OnCoinsChanged(int previousValue, int newValue) {
        if (boardOverlay != null) {
            if (IsOwner) {
                boardOverlay.SetCurrentPlayerCoins(newValue);
            }
        }
    }

    public void OnOtherCoinsChanged(int newValue, ulong playerId) {
        if (boardOverlay != null) {
            boardOverlay.SetPlayerCoins(newValue, playerId);
        }
    }

    // This method should be used when we're already on the server
    public void SetHealth(int value) {
        if (!IsServer) {
            Debug.LogError("SetHealth can only be called on the server");
            return;
        }

        Debug.Log($"Setting health to {value} for player {OwnerClientId}");
        value = Mathf.Clamp(value, 0, 100);
        health.Value = value;
    }

    // This method should be used when we're already on the server
    public void SetCoins(int value) {
        if (!IsServer) {
            Debug.LogError("SetCoins can only be called on the server");
            return;
        }

        Debug.Log($"Setting coins to {value} for player {OwnerClientId}");
        value = Mathf.Max(0, value);
        coins.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHealthServerRpc(int value) {
        // This RPC is intended for clients to call the server
        Debug.Log($"SetHealthServerRpc called with value: {value}");
        SetHealth(value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCoinsServerRpc(int value) {
        // This RPC is intended for clients to call the server
        Debug.Log($"SetCoinsServerRpc called with value: {value}");
        SetCoins(value);
    }

    [ClientRpc]
    public void NotifyHealthChangeClientRpc(int newHealthValue, ulong playerId) {
        // Skip if this is the player whose health changed - they'll get the update via NetworkVariable
        if (playerId != OwnerClientId) {
            OnOtherHealthChanged(newHealthValue, playerId);
        }
        // TODO: This is kinda fucked, OwnerClientId is the same as playerId, now idea why, has to be fixed later, for trivial testing purposes it's ok
        OnOtherHealthChanged(newHealthValue, playerId);
    }

    [ClientRpc]
    public void NotifyCoinsChangeClientRpc(int newCoinsValue, ulong playerId) {
        // Skip if this is the player whose coins changed - they'll get the update via NetworkVariable
        if (playerId != OwnerClientId) {
            OnOtherCoinsChanged(newCoinsValue, playerId);
        }
        // TODO: This is kinda fucked, OwnerClientId is the same as playerId, now idea why, has to be fixed later, for trivial testing purposes it's ok
        OnOtherCoinsChanged(newCoinsValue, playerId);
    }
}
