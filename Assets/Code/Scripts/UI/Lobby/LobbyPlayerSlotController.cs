using UnityEngine;

public class LobbyPlayerSlotController {
    private LobbyPlayerSlotUI[] lobbyPlayerSlotUIs = new LobbyPlayerSlotUI[4];

    public LobbyPlayerSlotController() {
        for (var i = 0; i < lobbyPlayerSlotUIs.Length; i++) {
            lobbyPlayerSlotUIs[i] = new LobbyPlayerSlotUI();
        }
    }
}