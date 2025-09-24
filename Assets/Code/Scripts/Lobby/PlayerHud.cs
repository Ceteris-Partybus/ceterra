using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHud : NetworkBehaviour {
    [SerializeField] private UIDocument uiDocument;
    private Button characterSelectionButton;

    [Header("Character Selection")]
    [SerializeField] private LobbyCameraHandler lobbyCameraHandler;
    [SerializeField] private CharacterSelectionController characterSelectionController;

    void Start() {
        if (isServer) {
            characterSelectionController.gameObject.SetActive(false);
            StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            return;
        }
        this.characterSelectionButton = this.uiDocument.rootVisualElement.Q<Button>("CharacterSelectionBtn");
        this.characterSelectionButton.clicked += OnCharacterSelectionButtonClicked;
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
            characterSelectionButton.SetEnabled(false);
            yield return StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            characterSelectionButton.SetEnabled(true);
            characterSelectionController.ToggleCharacterSelection();
            characterSelectionButton.text = "x Close";
        }
    }

    private void HideCharacterSelection() {
        StartCoroutine(TransitionToLobby());

        IEnumerator TransitionToLobby() {
            characterSelectionButton.SetEnabled(false);
            characterSelectionController.ToggleCharacterSelection();
            yield return StartCoroutine(lobbyCameraHandler.ToggleCharacterSelection());
            characterSelectionButton.SetEnabled(true);
            characterSelectionButton.text = "Close Character Selection";
        }
    }
}