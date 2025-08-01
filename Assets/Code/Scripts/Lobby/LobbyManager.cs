using Mirror;
using UnityEngine;

public class LobbyManager : NetworkRoomManager {
    public static new LobbyManager singleton {
        get {
            return NetworkManager.singleton as LobbyManager;
        }
    }

    public override void OnRoomServerSceneChanged(string sceneName) {
        if (sceneName == GameplayScene) {
            Debug.Log($"OnRoomServerSceneChanged: {sceneName}");
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
        lobbyPlayer.SetHidden(true);
        Debug.Log($"OnRoomServerSceneLoadedForPlayer: {conn.address} - {gamePlayer.name}");

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