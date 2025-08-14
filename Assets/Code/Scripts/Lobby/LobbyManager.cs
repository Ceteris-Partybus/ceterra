using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyManager : NetworkRoomManager {
    public static new LobbyManager singleton => NetworkManager.singleton as LobbyManager;

    public EnvironmentDisplay environmentDisplay;

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
                foreach (NetworkRoomPlayer roomPlayer in roomSlots) {
                    LobbyPlayer lobbyPlayer = roomPlayer as LobbyPlayer;
                    if (lobbyPlayer != null) {
                        lobbyPlayer.SetHidden(true);
                    }
                }
                StartCoroutine(ReplacePlayersWithMinigamePlayers(sceneName));
            }
        }
    }

    private System.Collections.IEnumerator ReplacePlayersWithMinigamePlayers(string sceneName) {
        yield return null;

        GameObject minigamePlayerPrefab = MinigameManager.Instance?.GetPlayerPrefabForScene(sceneName);
        if (minigamePlayerPrefab == null) {
            yield break;
        }

        foreach (NetworkRoomPlayer roomPlayer in roomSlots) {
            if (roomPlayer == null) {
                continue;
            }

            LobbyPlayer lobbyPlayer = roomPlayer as LobbyPlayer;
            if (lobbyPlayer == null) {
                continue;
            }

            Transform startPos = GetStartPosition();
            GameObject newMinigamePlayer = startPos != null
                ? Instantiate(minigamePlayerPrefab, startPos.position, startPos.rotation)
                : Instantiate(minigamePlayerPrefab, Vector3.zero, Quaternion.identity);

            newMinigamePlayer.name = $"{minigamePlayerPrefab.name} [id={lobbyPlayer.index}]";

            if (newMinigamePlayer.TryGetComponent<MinigameOnePlayer>(out MinigameOnePlayer minigameOnePlayer)) {
                minigameOnePlayer.Id = lobbyPlayer.index;
                minigameOnePlayer.PlayerName = $"Player {lobbyPlayer.index}";
            }
            else {
                Debug.LogWarning($"Unknown minigame player type on object: {newMinigamePlayer.name}");
                Destroy(newMinigamePlayer);
                continue;
            }

            NetworkServer.Spawn(newMinigamePlayer, lobbyPlayer.connectionToClient);

            NetworkServer.ReplacePlayerForConnection(lobbyPlayer.connectionToClient, newMinigamePlayer, ReplacePlayerOptions.KeepAuthority);
        }
    }

    public override void OnRoomClientSceneChanged() {
        if (NetworkServer.active) {
            return;
        }

        if (networkSceneName != GameplayScene) {
        }

        MinigameData minigameData = MinigameManager.Instance?.GetMinigameData(networkSceneName);
        if (minigameData != null) {
            Debug.Log($"Client scene changed to minigame: {networkSceneName}");
        }
    }

    // TODO: Is this called, when returning to board from a minigame?
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer) {
        LobbyPlayer lobbyPlayer = roomPlayer.GetComponent<LobbyPlayer>();
        Debug.Log($"OnRoomServerSceneLoadedForPlayer: Scene={networkSceneName}, RoomPlayer={roomPlayer?.name}, GamePlayer={gamePlayer?.name}");

        BoardPlayer boardPlayer = gamePlayer.GetComponent<BoardPlayer>();
        if (boardPlayer != null) {
            boardPlayer.Id = lobbyPlayer.index;
            boardPlayer.PlayerName = "Player " + lobbyPlayer.index;
            Debug.Log($"Initialized BoardPlayer: {boardPlayer.PlayerName}");
        }

        lobbyPlayer.SetHidden(true);
        return true;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        if (MinigameManager.Instance.IsMinigameScene(networkSceneName)) {
            return;
        }

        base.OnServerAddPlayer(conn);
    }

    public override void OnServerReady(NetworkConnectionToClient conn) {
        if (MinigameManager.Instance.IsMinigameScene(networkSceneName)) {
            NetworkServer.SetClientReady(conn);
            return;
        }

        base.OnServerReady(conn);
    }
}