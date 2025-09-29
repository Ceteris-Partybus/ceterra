using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyPlayerSlotController : MonoBehaviour {
    [SerializeField] private UIDocument lobbyUI;
    private LobbyPlayerSlotUI[] lobbyPlayerSlotUIs = new LobbyPlayerSlotUI[4];

    public void Start() {
        var root = lobbyUI.rootVisualElement;
        for (var i = 0; i < lobbyPlayerSlotUIs.Length; i++) {
            lobbyPlayerSlotUIs[i] = new LobbyPlayerSlotUI(root.Q<VisualElement>($"lobby-player-slot-{i}"));
        }
    }

    public void OnLobbyPlayerAdded(LobbyPlayer lobbyPlayer, Action showCharacterSelection) {
        lobbyPlayerSlotUIs[lobbyPlayer.index].AssignTo(lobbyPlayer, showCharacterSelection);
    }

    public void OnLobbyPlayerRemoved(LobbyPlayer lobbyPlayer) {
        lobbyPlayerSlotUIs[lobbyPlayer.index].Clear();
    }

    public void Show() {
        lobbyUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        lobbyUI.rootVisualElement.style.display = DisplayStyle.None;
    }
}