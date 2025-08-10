using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkRoomPlayer {

    private int id = -1;
    public int Id => id;

    private string playerName;
    public string PlayerName => playerName;

    // TODO: Start wird nur einmal aufgerufen, das hiden in ne neue mehtode die vom GameManager aufgerufen wird
    public override void Start() {
        base.Start();

        if (id == -1) {
            id = (int)netId;
        }
        if (string.IsNullOrEmpty(playerName)) {
            playerName = $"Player[{id}]";
        }

        if (NetworkManager.networkSceneName != GameManager.singleton.RoomScene) {
            gameObject.SetActive(false);
        }
    }

    public override void OnClientEnterRoom() {
        gameObject.transform.position = GameManager.singleton.GetStartPosition().position;
    }
}
