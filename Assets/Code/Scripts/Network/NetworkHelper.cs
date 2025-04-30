using UnityEngine;

using Unity.Netcode;

namespace Assets.Code.Scripts.Network {
    public class PlayerManager : MonoBehaviour {
        private NetworkManager networkManager;

        private void Awake() {
            networkManager = GetComponent<NetworkManager>();
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!networkManager.IsClient && !networkManager.IsServer) {
                StartButtons();
            }
            else {
                StatusLabels();

                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        private void StartButtons() {
            if (GUILayout.Button("Host")) {
                networkManager.StartHost();
            }

            if (GUILayout.Button("Client")) {
                networkManager.StartClient();
            }

            if (GUILayout.Button("Server")) {
                networkManager.StartServer();
            }
        }

        private void StatusLabels() {
            var mode = networkManager.IsHost ?
                "Host" : networkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                networkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        private void SubmitNewPosition() {
            if (GUILayout.Button(networkManager.IsServer ? "Move" : "Request Position Change")) {
                if (networkManager.IsServer && !networkManager.IsClient) {
                    //! Das bewegt gerade alle Spieler, die nicht der Host sind. Für unser Spiel ist das eigentlich irrelevant, weil der Server sowas so oder so nicht machen sollte. Ist nur aus dem Tutorial gezogen. Siehe https://docs-multiplayer.unity3d.com/netcode/current/tutorials/get-started-ngo/#adding-netcode-script-to-your-player-prefab
                    foreach (ulong uid in networkManager.ConnectedClientsIds) {
                        networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<NetworkPlayer>().Move();
                    }
                }
                else {
                    var playerObject = networkManager.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<NetworkPlayer>();
                    player.Move();
                }
            }
        }
    }
}