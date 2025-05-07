using Assets.Code.Scripts.UI;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Code.Scripts.Network {
    public class NetworkPlayer : NetworkBehaviour {
        public NetworkVariable<Vector3> position = new();
        public NetworkVariable<int> health = new(42);
        public NetworkVariable<int> coins = new(100);

        private IngameOverlay ingameOverlay;

        public override void OnNetworkSpawn() {
            if (IsOwner) {
                Move();

                ingameOverlay = FindFirstObjectByType<IngameOverlay>();

                health.OnValueChanged += OnHealthChanged;
                coins.OnValueChanged += OnCoinsChanged;

                if (ingameOverlay != null) {
                    ingameOverlay.UpdatePlayerHealth(health.Value);
                    ingameOverlay.UpdatePlayerCoins(coins.Value);
                }
            }
        }

        public override void OnNetworkDespawn() {
            if (IsOwner) {
                health.OnValueChanged -= OnHealthChanged;
                coins.OnValueChanged -= OnCoinsChanged;
            }
        }

        private void OnHealthChanged(int previousValue, int newValue) {
            if (IsOwner && ingameOverlay != null) {
                // TODO: rename to UpdateClientHealth and UpdateClientCoins respectively create new method in IngameOverlay where the player instance is passed as a parameter to update only the affected player in the all player list
                ingameOverlay.UpdatePlayerHealth(newValue);
            }
        }

        private void OnCoinsChanged(int previousValue, int newValue) {
            if (IsOwner && ingameOverlay != null) {
                ingameOverlay.UpdatePlayerCoins(newValue);
            }
        }

        public void Move() {
            this.SubmitPositionRequestRpc();
        }

        [Rpc(SendTo.Server)]
        private void SubmitPositionRequestRpc(RpcParams rpcParams = default) {
            var randomPosition = GetRandomPositionOnPlane();
            this.transform.position = randomPosition;
            this.position.Value = randomPosition;
        }

        static Vector3 GetRandomPositionOnPlane() {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        private void Update() {
            this.transform.position = this.position.Value;
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
    }
}