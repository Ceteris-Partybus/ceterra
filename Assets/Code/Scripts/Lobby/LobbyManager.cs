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
            Debug.Log($"OnRoomServerSceneChanged: {sceneName}");
            BoardContext.Instance?.StartPlayerTurn();
        }
    }

    public override void OnRoomClientSceneChanged() {
        if (NetworkServer.active) {
            return;
        }

        if (networkSceneName == GameplayScene) {
            Debug.Log($"OnRoomClientSceneChanged: {networkSceneName}");
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer) {
        LobbyPlayer lobbyPlayer = roomPlayer.GetComponent<LobbyPlayer>();
        BoardPlayer boardPlayer = gamePlayer.GetComponent<BoardPlayer>();

        boardPlayer.Id = lobbyPlayer.index;
        boardPlayer.PlayerName = "Player " + lobbyPlayer.index;

        lobbyPlayer.SetHidden(true);

        return true;
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