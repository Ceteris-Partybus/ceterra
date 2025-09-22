using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHud : NetworkBehaviour {
    [SerializeField] private UIDocument uiDocument;
    private Button characterSelectionButton;

    [Header("Character Selection")]
    [SerializeField] private Camera characterSelectionCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas characterSelectionCanvas;

    private void OnEnable() {
        this.characterSelectionButton = this.uiDocument.rootVisualElement.Q<Button>("CharacterSelectionBtn");
        this.characterSelectionButton.clicked += OnCharacterSelectionButtonClicked;
    }

    public void OnCharacterSelectionButtonClicked() {
        characterSelectionCamera.enabled = !characterSelectionCamera.enabled;
        mainCamera.enabled = !mainCamera.enabled;
        characterSelectionCanvas.enabled = !characterSelectionCanvas.enabled;
        characterSelectionButton.text = characterSelectionCamera.enabled ? "x Close" : "Character Selection";
    }
}