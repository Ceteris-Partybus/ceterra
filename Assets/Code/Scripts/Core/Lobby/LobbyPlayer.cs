using Mirror;
using System.Linq;
using UnityEngine;

public class LobbyPlayer : NetworkRoomPlayer {
    public int Id => index;
    [SyncVar] private string playerName;
    public string PlayerName => playerName;
    private int selectedCharacterIndex = -1;
    public int SelectedCharacterIndex => selectedCharacterIndex;
    private GameObject currentCharacterInstance;
    public GameObject CurrentCharacterInstance => currentCharacterInstance;
    private int selectedDiceIndex = -1;
    public int SelectedDiceIndex => selectedDiceIndex;
    private GameObject currentDiceInstance;
    public GameObject CurrentDiceInstance => currentDiceInstance;

    private GameObject CharacterModel => GameManager.Singleton.GetCharacter(selectedCharacterIndex);
    private GameObject DiceModel => GameManager.Singleton.GetDice(selectedDiceIndex);

    public override void OnClientEnterRoom() {
        gameObject.transform.position = GameManager.Singleton.GetStartPosition().position;
    }

    [Command]
    public void CmdSetCharacterSelection(int characterIndex, int diceIndex, string playerName) {
        var characterHasChanged = characterIndex != selectedCharacterIndex;
        if (characterHasChanged) {
            ChangeSelectedCharacter(characterIndex);
            RpcChangeSelectedCharacter(characterIndex);
        }
        if (diceIndex != selectedDiceIndex || characterHasChanged) {
            ChangeSelectedDice(diceIndex);
            RpcChangeSelectedDice(diceIndex);
        }
        this.playerName = playerName;
    }

    public void ChangeSelectedCharacter(int characterIndex) {
        if (currentCharacterInstance != null) { Destroy(currentCharacterInstance); }
        selectedCharacterIndex = characterIndex;
        currentCharacterInstance = Instantiate(CharacterModel, transform);
        currentCharacterInstance.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    [ClientRpc]
    public void RpcChangeSelectedCharacter(int characterIndex) {
        ChangeSelectedCharacter(characterIndex);
    }

    public void ChangeSelectedDice(int diceIndex) {
        if (currentDiceInstance != null) { Destroy(currentDiceInstance); }
        selectedDiceIndex = diceIndex;
        var dicePosition = currentCharacterInstance.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.CompareTag("DicePosition"));
        currentDiceInstance = Instantiate(DiceModel, dicePosition);
    }

    [ClientRpc]
    public void RpcChangeSelectedDice(int diceIndex) {
        ChangeSelectedDice(diceIndex);
    }
}
