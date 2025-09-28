using UnityEngine;
using UnityEngine.UIElements;

public class LobbyPlayerCardUI : MonoBehaviour {
    [SerializeField] private UIDocument lobbyPlayerCardUI;
    public Label playerDisplayName;
    public Label playerPing;
    public Button characterSelectionBtn;
    public Button readyBtn;

    void OnEnable() {
        var root = lobbyPlayerCardUI.rootVisualElement;
        playerDisplayName = root.Q<Label>("player-card__display-name");
        playerPing = root.Q<Label>("player-card__ping");
        characterSelectionBtn = root.Q<Button>("player-card__character-selection-btn");
        readyBtn = root.Q<Button>("player-card__ready-btn");
    }
}