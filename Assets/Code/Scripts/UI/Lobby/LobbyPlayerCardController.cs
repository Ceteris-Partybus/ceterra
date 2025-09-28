using System;
using UnityEngine;

public class LobbyPlayerCardController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private LobbyPlayerCardUI lobbyPlayerCardUI;
    public Action OnRequestBackToCharacterSelection;

    public void Initialize(LobbyPlayer lobbyPlayer) {
        if (string.IsNullOrWhiteSpace(lobbyPlayer.PlayerName)) {
            lobbyPlayerCardUI.playerDisplayName.text = "New player";
            return;
        }
        lobbyPlayerCardUI.playerDisplayName.text = lobbyPlayer.PlayerName;
        lobbyPlayerCardUI.characterSelectionBtn.clicked += () => OnRequestBackToCharacterSelection?.Invoke();
        lobbyPlayerCardUI.readyBtn.clicked += () => lobbyPlayer.CmdChangeReadyState(!lobbyPlayer.readyToBegin);
    }
}