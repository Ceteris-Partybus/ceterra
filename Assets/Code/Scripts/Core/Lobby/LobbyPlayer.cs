using Mirror;
using System.Linq;
using UnityEngine;

public class LobbyPlayer : NetworkRoomPlayer {
    public int Id => index;

    private string playerName;
    public string PlayerName => playerName;

    [SyncVar(hook = nameof(OnSelectedCharacterChanged))]
    private int selectedCharacterIndex = -1;
    public int SelectedCharacterIndex => selectedCharacterIndex;
    private GameObject currentCharacterInstance;

    [SyncVar(hook = nameof(OnSelectedDiceChanged))]
    private int selectedDiceIndex = -1;
    public int SelectedDiceIndex => selectedDiceIndex;
    private GameObject currentDiceInstance;
    private GameObject CharacterModel => GameManager.Singleton.GetCharacter(selectedCharacterIndex);
    private GameObject DiceModel => GameManager.Singleton.GetDice(selectedDiceIndex);

    public override void Start() {
        base.Start();

        if (string.IsNullOrEmpty(playerName)) {
            playerName = $"Player[{index}]";
        }
    }

    public override void OnClientEnterRoom() {
        gameObject.transform.position = GameManager.Singleton.GetStartPosition().position;
    }

    public void Hide() {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    [Command]
    public void CmdSetCharacterSelection(int characterIndex, int diceIndex) {
        var characterHasChanged = characterIndex != selectedCharacterIndex;
        if (characterHasChanged) {
            selectedCharacterIndex = characterIndex;
            ChangeSelectedCharacter();
        }
        if (diceIndex != selectedDiceIndex || characterHasChanged) {
            selectedDiceIndex = diceIndex;
            ChangeSelectedDice();
        }
    }

    public void ChangeSelectedCharacter() {
        if (currentCharacterInstance != null) { Destroy(currentCharacterInstance); }
        currentCharacterInstance = Instantiate(CharacterModel, transform);
        currentCharacterInstance.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    public void OnSelectedCharacterChanged(int _old, int _new) {
        ChangeSelectedCharacter();
    }

    public void ChangeSelectedDice() {
        if (currentDiceInstance != null) { Destroy(currentDiceInstance); }
        var dicePosition = currentCharacterInstance.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.CompareTag("DicePosition"));
        currentDiceInstance = Instantiate(DiceModel, dicePosition);
    }

    public void OnSelectedDiceChanged(int _old, int _new) {
        ChangeSelectedDice();
    }
}
