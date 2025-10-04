using System.Collections;
using UnityEngine;

public class PlayerHud : NetworkedSingleton<PlayerHud> {
    [Header("Character Selection")]
    [SerializeField] private LobbyCameraHandler lobbyCameraHandler;
    [SerializeField] private CharacterSelectionController characterSelectionController;
    [SerializeField] private LobbyPlayerSlotController lobbyPlayerSlotController;
    protected override void Start() {
        base.Start();

        if (isServer) {
            characterSelectionController.SelectionUI.SetActive(false);
            StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            return;
        }
        lobbyPlayerSlotController.Hide();
        characterSelectionController.OnRequestBackToLobby += HideCharacterSelection;
    }

    public void ShowCharacterSelection() {
        StartCoroutine(TransitionToCharacterSelection());

        IEnumerator TransitionToCharacterSelection() {
            lobbyPlayerSlotController.Hide();
            yield return StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            characterSelectionController.ToggleCharacterSelection();
        }
    }

    private void HideCharacterSelection() {
        StartCoroutine(TransitionToLobby());

        IEnumerator TransitionToLobby() {
            characterSelectionController.ToggleCharacterSelection();
            yield return StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            lobbyPlayerSlotController.Show();
        }
    }
}