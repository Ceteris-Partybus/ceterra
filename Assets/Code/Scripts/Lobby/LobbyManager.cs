using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyManager : NetworkRoomManager {
    public static new LobbyManager singleton {
        get {
            return NetworkManager.singleton as LobbyManager;
        }
    }

    public List<int> GetPlayerIds() {
        return roomSlots.Select(slot => {
            return slot.index;
        }).ToList();
    }

    public override void OnRoomServerSceneChanged(string sceneName) {
        if (sceneName == GameplayScene) {
            BoardContext.Instance?.StartPlayerTurn();
        }
        else {
            MinigameData minigameData = MinigameManager.Instance?.GetMinigameData(sceneName);
            if (minigameData != null) {
                Debug.Log($"OnRoomServerSceneChanged: Minigame scene {sceneName} loaded");
                Debug.Log($"[DEBUG] RoomSlots count: {roomSlots.Count}");

                foreach (NetworkRoomPlayer roomPlayer in roomSlots) {
                    LobbyPlayer lobbyPlayer = roomPlayer as LobbyPlayer;
                    if (lobbyPlayer != null) {
                        Debug.Log($"[DEBUG] Hiding lobby player {lobbyPlayer.index} for minigame scene");
                        lobbyPlayer.SetHidden(true);
                    }
                    else {
                        Debug.Log($"[DEBUG] RoomPlayer is not a LobbyPlayer or is null");
                    }
                }
                // Initialize minigame-specific context here if needed
                // The minigame context will be initialized when the scene loads

                // Replace all existing players with minigame player instances
                StartCoroutine(ReplacePlayersWithMinigamePlayers(sceneName));
            }
        }
    }

    private System.Collections.IEnumerator ReplacePlayersWithMinigamePlayers(string sceneName) {
        // Wait a frame to ensure the scene is fully loaded
        yield return null;

        Debug.Log($"[DEBUG] ReplacePlayersWithMinigamePlayers started for scene: {sceneName}");

        GameObject minigamePlayerPrefab = MinigameManager.Instance?.GetPlayerPrefabForScene(sceneName);
        if (minigamePlayerPrefab == null) {
            Debug.LogError($"No minigame player prefab found for scene: {sceneName}");
            yield break;
        }

        Debug.Log($"[DEBUG] Found minigame player prefab: {minigamePlayerPrefab.name}");

        // Find all room players and replace them with minigame players
        foreach (NetworkRoomPlayer roomPlayer in roomSlots) {
            if (roomPlayer == null) {
                Debug.Log($"[DEBUG] Skipping null room player");
                continue;
            }

            LobbyPlayer lobbyPlayer = roomPlayer as LobbyPlayer;
            if (lobbyPlayer == null) {
                Debug.Log($"[DEBUG] RoomPlayer is not a LobbyPlayer");
                continue;
            }

            Debug.Log($"[DEBUG] Processing lobby player {lobbyPlayer.index}, connection: {lobbyPlayer.connectionToClient?.connectionId}");

            // Create a new minigame player instance
            Transform startPos = GetStartPosition();
            GameObject newMinigamePlayer = startPos != null
                ? Instantiate(minigamePlayerPrefab, startPos.position, startPos.rotation)
                : Instantiate(minigamePlayerPrefab, Vector3.zero, Quaternion.identity);

            // Set a useful name for debugging
            newMinigamePlayer.name = $"{minigamePlayerPrefab.name} [connId={lobbyPlayer.connectionToClient.connectionId}]";
            Debug.Log($"[DEBUG] Created minigame player: {newMinigamePlayer.name} at position {newMinigamePlayer.transform.position}");

            // Initialize based on minigame type
            if (newMinigamePlayer.TryGetComponent<MinigameOnePlayer>(out MinigameOnePlayer minigameOnePlayer)) {
                Debug.Log($"[DEBUG] Initializing MinigameOnePlayer for player {lobbyPlayer.index}");
                // Initialize with basic data instead of BoardPlayer for now
                minigameOnePlayer.Id = lobbyPlayer.index;
                minigameOnePlayer.PlayerName = $"Player {lobbyPlayer.index}";
                Debug.Log($"Successfully initialized MinigameOnePlayer: {minigameOnePlayer.PlayerName}");
            }
            else {
                Debug.LogWarning($"Unknown minigame player type on object: {newMinigamePlayer.name}");
                Destroy(newMinigamePlayer);
                continue;
            }

            // Spawn the minigame player on the network first
            Debug.Log($"[DEBUG] Spawning minigame player on network");
            NetworkServer.Spawn(newMinigamePlayer, lobbyPlayer.connectionToClient);

            // Replace the connection with the new minigame player
            Debug.Log($"[DEBUG] Replacing connection for player {lobbyPlayer.index}");
            bool success = NetworkServer.ReplacePlayerForConnection(lobbyPlayer.connectionToClient, newMinigamePlayer, ReplacePlayerOptions.KeepAuthority);
            Debug.Log($"Replace player result: {success} for player {lobbyPlayer.index}");
        }

        Debug.Log($"[DEBUG] ReplacePlayersWithMinigamePlayers completed");
    }

    public override void OnRoomClientSceneChanged() {
        if (NetworkServer.active) {
            return;
        }

        if (networkSceneName != GameplayScene) {
        }
        // Check if this is a minigame scene
        MinigameData minigameData = MinigameManager.Instance?.GetMinigameData(networkSceneName);
        if (minigameData != null) {
            Debug.Log($"OnRoomClientSceneChanged: Minigame scene {networkSceneName} loaded on client");
            // Any client-specific minigame initialization can go here
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer) {
        LobbyPlayer lobbyPlayer = roomPlayer.GetComponent<LobbyPlayer>();
        Debug.Log($"OnRoomServerSceneLoadedForPlayer: Scene={networkSceneName}, RoomPlayer={roomPlayer?.name}, GamePlayer={gamePlayer?.name}");

        // For minigame scenes, the player replacement is handled in OnRoomServerSceneChanged
        // So we just return true to allow the normal flow
        if (networkSceneName != GameplayScene) {
            Debug.Log($"Player loaded for minigame scene: {networkSceneName}");
            return true;
        }

        // Default behavior for GameplayScene (board scene)
        BoardPlayer boardPlayer = gamePlayer.GetComponent<BoardPlayer>();
        if (boardPlayer != null) {
            boardPlayer.Id = lobbyPlayer.index;
            boardPlayer.PlayerName = "Player " + lobbyPlayer.index;
            Debug.Log($"Initialized BoardPlayer: {boardPlayer.PlayerName}");
        }

        lobbyPlayer.SetHidden(true);
        return true;
    }

    private BoardPlayer GetBoardPlayerForLobbyPlayer(LobbyPlayer lobbyPlayer) {
        // Find the corresponding BoardPlayer in the current scene
        BoardPlayer[] boardPlayers = FindObjectsByType<BoardPlayer>(FindObjectsSortMode.None);
        return boardPlayers.FirstOrDefault(bp => bp.Id == lobbyPlayer.index);
    }

    public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn) {
        Debug.Log($"OnRoomServerAddPlayer: {conn.address}");

        base.OnRoomServerAddPlayer(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        // For minigame scenes, we need to handle player addition differently
        if (networkSceneName != RoomScene && networkSceneName != GameplayScene) {
            Debug.Log($"Client requesting to add player in minigame scene: {networkSceneName}");

            // In minigame scenes, we don't add new players - existing players should already be there
            // This might be a reconnection attempt. Let the client reconnect to their existing player.
            // We can add logic here if needed for handling reconnections to minigames

            // Debug.LogWarning($"Player addition blocked in minigame scene. Scene: {networkSceneName}");
            return;
        }

        // Use the base NetworkRoomManager behavior for Room and Gameplay scenes
        base.OnServerAddPlayer(conn);
    }

    public override void OnServerReady(NetworkConnectionToClient conn) {
        Debug.Log($"OnServerReady called for scene: {networkSceneName}");

        // For minigame scenes, we handle readiness differently
        if (networkSceneName != RoomScene && networkSceneName != GameplayScene) {
            Debug.Log($"Client ready in minigame scene: {networkSceneName}");
            // Call the base NetworkManager.OnServerReady instead of NetworkRoomManager.OnServerReady
            // to avoid the SceneLoadedForPlayer call which expects room players
            NetworkServer.SetClientReady(conn);
            return;
        }

        // Use the base NetworkRoomManager behavior for Room and Gameplay scenes
        base.OnServerReady(conn);
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer) {
        Debug.Log($"OnRoomServerCreateGamePlayer: {conn.address} for scene: {networkSceneName}");

        // Default behavior for all scenes - let the base class handle it
        return null;
    }

    public override void OnRoomStopClient() {
        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer() {
        base.OnRoomStopServer();
    }

#if !UNITY_SERVER
    public override void OnGUI() {
        base.OnGUI();
    }
#endif
}