using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkRoomPlayer {
    private int id = -1;
    public int Id => id;

    private string playerName;
    public string PlayerName => playerName;

    [SyncVar]
    public int selectedMeshIndex = 0;

    public override void Start() {
        base.Start();

        if (id == -1) {
            id = (int)netId;
        }
        if (string.IsNullOrEmpty(playerName)) {
            playerName = $"Player[{id}]";
        }
    }

    public override void OnClientEnterRoom() {
        gameObject.transform.position = GameManager.singleton.GetStartPosition().position;
    }

    public void SetSelectedMeshIndex(int index) {
        if (isLocalPlayer)
        {
            CmdSetSelectedMeshIndex(index);
        }
    }

    [Command]
    private void CmdSetSelectedMeshIndex(int index)
    {
        selectedMeshIndex = index;
    }

    public void Hide() {
        GetComponent<Renderer>().enabled = false;
    }
}
