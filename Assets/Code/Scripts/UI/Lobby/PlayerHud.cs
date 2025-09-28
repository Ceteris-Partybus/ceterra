using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHud : NetworkBehaviour {
    [SerializeField] private UIDocument lobbyUI;

    [Header("Character Selection")]
    [SerializeField] private LobbyCameraHandler lobbyCameraHandler;
    [SerializeField] private CharacterSelectionController characterSelectionController;

    void Start() {
        if (isServer) {
            characterSelectionController.gameObject.SetActive(false);
            StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            return;
        }
        characterSelectionController.OnRequestBackToLobby += HideCharacterSelection;
    }

    private void OnCharacterSelectionButtonClicked() {
        if (lobbyCameraHandler.IsShowingLobby) {
            ShowCharacterSelection();
            return;
        }
        HideCharacterSelection();
    }

    private void ShowCharacterSelection() {
        StartCoroutine(TransitionToCharacterSelection());

        IEnumerator TransitionToCharacterSelection() {
            //characterSelectionButton.SetEnabled(false);
            yield return StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            // characterSelectionButton.SetEnabled(true);
            characterSelectionController.ToggleCharacterSelection();
            // characterSelectionButton.text = "x Close";
        }
    }

    private void HideCharacterSelection() {
        StartCoroutine(TransitionToLobby());

        IEnumerator TransitionToLobby() {
            //   characterSelectionButton.SetEnabled(false);
            characterSelectionController.ToggleCharacterSelection();
            yield return StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            // characterSelectionButton.SetEnabled(true);
            // characterSelectionButton.text = "Close Character Selection";
        }
    }
}