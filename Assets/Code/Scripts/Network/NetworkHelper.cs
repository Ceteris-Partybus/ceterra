using UnityEngine;

using Unity.Netcode;
using System.Collections.Generic;

public class NetworkHelper : MonoBehaviour {
    private NetworkManager networkManager;
    private string healthInput = "100";
    private string coinsInput = "100";
    private int selectedClientIndex = 0;
    private string[] clientOptions = new string[0];
    private ulong[] clientIds = new ulong[0];

    private void Awake() {
        networkManager = GetComponent<NetworkManager>();
    }

    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 700, 300, 300));
        if (!networkManager.IsClient && !networkManager.IsServer) {
            StartButtons();
        }
        else {
            StatusLabels();

            SubmitNewPosition();

            // Only show admin controls if we're the server
            if (networkManager.IsServer) {
                ServerControls();
            }
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

    private void ServerControls() {
        GUILayout.Space(10);
        GUILayout.Label("Server Controls", GUI.skin.box);

        UpdateClientList();

        GUILayout.Label("All Players", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Health:", GUILayout.Width(50));
        healthInput = GUILayout.TextField(healthInput, GUILayout.Width(50));
        if (GUILayout.Button("Set All Health", GUILayout.Width(120))) {
            SetAllPlayersHealth();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Coins:", GUILayout.Width(50));
        coinsInput = GUILayout.TextField(coinsInput, GUILayout.Width(50));
        if (GUILayout.Button("Set All Coins", GUILayout.Width(120))) {
            SetAllPlayersCoins();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Single Player", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Player:", GUILayout.Width(50));
        if (clientOptions.Length > 0) {
            selectedClientIndex = GUILayout.SelectionGrid(
                selectedClientIndex,
                clientOptions,
                clientOptions.Length,
                GUI.skin.toggle);
        }
        else {
            GUILayout.Label("No clients connected");
        }
        GUILayout.EndHorizontal();

        if (clientOptions.Length > 0) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Health:", GUILayout.Width(50));
            healthInput = GUILayout.TextField(healthInput, GUILayout.Width(50));
            if (GUILayout.Button("Set Health", GUILayout.Width(120))) {
                SetSinglePlayerHealth();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Coins:", GUILayout.Width(50));
            coinsInput = GUILayout.TextField(coinsInput, GUILayout.Width(50));
            if (GUILayout.Button("Set Coins", GUILayout.Width(120))) {
                SetSinglePlayerCoins();
            }
            GUILayout.EndHorizontal();
        }
    }

    private void UpdateClientList() {
        // Only update occasionally to avoid excessive rebuilding
        if (Time.frameCount % 30 == 0 || clientOptions.Length != networkManager.ConnectedClientsIds.Count) {
            List<string> options = new List<string>();
            List<ulong> ids = new List<ulong>();

            foreach (ulong clientId in networkManager.ConnectedClientsIds) {
                options.Add($"Player {clientId}");
                ids.Add(clientId);
            }

            clientOptions = options.ToArray();
            clientIds = ids.ToArray();

            if (selectedClientIndex >= clientOptions.Length && clientOptions.Length > 0) {
                selectedClientIndex = 0;
            }
        }
    }

    private void SetSinglePlayerHealth() {
        if (!networkManager.IsServer || clientIds.Length == 0 || selectedClientIndex >= clientIds.Length) {
            return;
        }

        if (int.TryParse(healthInput, out int healthValue)) {
            ulong targetClientId = clientIds[selectedClientIndex];
            var playerObject = networkManager.SpawnManager.GetPlayerNetworkObject(targetClientId);

            if (playerObject != null) {
                NetworkPlayer player = playerObject.GetComponent<NetworkPlayer>();
                if (player != null) {
                    Debug.Log($"Setting health to {healthValue} for player {targetClientId}");
                    player.SetHealth(healthValue);
                }
                else {
                    Debug.LogWarning($"NetworkPlayer component not found on player {targetClientId}");
                }
            }
            else {
                Debug.LogWarning($"Player object not found for client {targetClientId}");
            }
        }
        else {
            Debug.LogWarning("Invalid health value. Please enter a valid integer.");
        }
    }

    private void SetSinglePlayerCoins() {
        if (!networkManager.IsServer || clientIds.Length == 0 || selectedClientIndex >= clientIds.Length) {
            return;
        }

        if (int.TryParse(coinsInput, out int coinsValue)) {
            ulong targetClientId = clientIds[selectedClientIndex];
            var playerObject = networkManager.SpawnManager.GetPlayerNetworkObject(targetClientId);

            if (playerObject != null) {
                NetworkPlayer player = playerObject.GetComponent<NetworkPlayer>();
                if (player != null) {
                    Debug.Log($"Setting coins to {coinsValue} for player {targetClientId}");
                    player.SetCoins(coinsValue);
                }
                else {
                    Debug.LogWarning($"NetworkPlayer component not found on player {targetClientId}");
                }
            }
            else {
                Debug.LogWarning($"Player object not found for client {targetClientId}");
            }
        }
        else {
            Debug.LogWarning("Invalid coins value. Please enter a valid integer.");
        }
    }

    private void SetAllPlayersHealth() {
        if (!networkManager.IsServer) {
            Debug.LogWarning("Only the server can set all players' health.");
            return;
        }

        if (int.TryParse(healthInput, out int healthValue)) {
            Debug.Log($"Setting all players' health to {healthValue}");
            foreach (ulong clientId in networkManager.ConnectedClientsIds) {
                var playerObject = networkManager.SpawnManager.GetPlayerNetworkObject(clientId);
                if (playerObject != null) {
                    NetworkPlayer player = playerObject.GetComponent<NetworkPlayer>();
                    if (player != null) {
                        Debug.Log($"Calling SetHealthServerRpc for player {clientId}");
                        // Call the ServerRpc directly since we're on the server
                        player.SetHealth(healthValue);
                    }
                    else {
                        Debug.LogWarning($"NetworkPlayer component not found on player {clientId}");
                    }
                }
                else {
                    Debug.LogWarning($"Player object not found for client {clientId}");
                }
            }
        }
        else {
            Debug.LogWarning("Invalid health value. Please enter a valid integer.");
        }
    }

    private void SetAllPlayersCoins() {
        if (!networkManager.IsServer) {
            Debug.LogWarning("Only the server can set all players' coins.");
            return;
        }

        if (int.TryParse(coinsInput, out int coinsValue)) {
            Debug.Log($"Setting all players' coins to {coinsValue}");
            foreach (ulong clientId in networkManager.ConnectedClientsIds) {
                var playerObject = networkManager.SpawnManager.GetPlayerNetworkObject(clientId);
                if (playerObject != null) {
                    NetworkPlayer player = playerObject.GetComponent<NetworkPlayer>();
                    if (player != null) {
                        Debug.Log($"Calling SetCoinsServerRpc for player {clientId}");
                        // Call the ServerRpc directly since we're on the server
                        player.SetCoins(coinsValue);
                    }
                    else {
                        Debug.LogWarning($"NetworkPlayer component not found on player {clientId}");
                    }
                }
                else {
                    Debug.LogWarning($"Player object not found for client {clientId}");
                }
            }
        }
        else {
            Debug.LogWarning("Invalid coins value. Please enter a valid integer.");
        }
    }
}
