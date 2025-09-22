using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkRoomPlayer {

    private int id = -1;
    public int Id => id;

    private string playerName;
    public string PlayerName => playerName;

    [SyncVar(hook = nameof(OnChangeCurrentActivePlayerModel))]
    private int currentActivePlayerModel = 0;
    public int CurrentActivePlayerModel => currentActivePlayerModel;
    private GameObject PlayerModel => GetComponentInChildren<CharacterSelection>().SelectablePrefabs[currentActivePlayerModel];

    private void ChangeCurrentActivePlayerModel(int modelIndex) {
        PlayerModel.SetActive(false);
        currentActivePlayerModel = modelIndex;
        PlayerModel.SetActive(true);
        SetReferences();
    }

    private void OnChangeCurrentActivePlayerModel(int _, int newValue) {
        ChangeCurrentActivePlayerModel(newValue);
    }

    private void SetReferences() {
        var references = PlayerModel.GetComponent<CharacterReferences>();
        gameObject.GetComponent<MeshRenderer>().material = references.material;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshCollider>().sharedMesh = references.mesh;
    }

    public override void Start() {
        base.Start();
        SetReferences();

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

    public void Hide() {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }
}
