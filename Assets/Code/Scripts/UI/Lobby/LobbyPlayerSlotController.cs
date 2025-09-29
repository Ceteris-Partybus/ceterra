using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyPlayerSlotController : MonoBehaviour {
    [SerializeField] private UIDocument lobbyUI;
    private LobbyPlayerSlotUI[] lobbyPlayerSlotUIs = new LobbyPlayerSlotUI[4];
    private bool isHidden = false;

    public void Start() {
        var root = lobbyUI.rootVisualElement;
        for (var i = 0; i < lobbyPlayerSlotUIs.Length; i++) {
            lobbyPlayerSlotUIs[i] = new LobbyPlayerSlotUI(root.Q<VisualElement>($"lobby-player-slot-{i}"));
        }
    }

    public void Show() {
        isHidden = false;
        lobbyUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        isHidden = true;
        lobbyUI.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void OnGUI() {
        if (isHidden) { return; }

        var toClear = new List<int> { 0, 1, 2, 3 };
        foreach (var lobbyPlayer in GameManager.Singleton.roomSlots.Select(player => player.GetComponent<LobbyPlayer>())) {
            lobbyPlayerSlotUIs[lobbyPlayer.index].AssignTo(lobbyPlayer);
            toClear.Remove(lobbyPlayer.index);
        }

        foreach (var indexToClear in toClear) {
            lobbyPlayerSlotUIs[indexToClear].Clear();
        }
    }
}