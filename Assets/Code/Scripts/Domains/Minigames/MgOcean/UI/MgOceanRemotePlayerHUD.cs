using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MgOceanRemotePlayerHUD : NetworkedSingleton<MgOceanRemotePlayerHUD> {
    [SerializeField]
    private UIDocument uiDocument;

    private VisualElement remotePlayerOverview;

    private Dictionary<int, VisualElement> playerElements = new Dictionary<int, VisualElement>();
    [SerializeField]
    private VisualTreeAsset playerEntryTemplate;

    void OnEnable() {
        var root = uiDocument.rootVisualElement;
        remotePlayerOverview = root.Q<VisualElement>("remote-player-overview");
    }

    public bool IsPlayerAdded(int playerId) {
        return playerElements.ContainsKey(playerId);
    }

    public void AddPlayer(MgOceanPlayer player) {
        var playerElement = playerEntryTemplate.CloneTree();
        playerElement.Q<Label>("player-name").text = player.PlayerName;
        playerElement.Q<Label>("player-score").text = "0";
        remotePlayerOverview.Add(playerElement);
        playerElements[player.PlayerId] = playerElement;
    }

    public void RemovePlayer(int playerId) {
        if (playerElements.TryGetValue(playerId, out var playerElement)) {
            remotePlayerOverview.Remove(playerElement);
            playerElements.Remove(playerId);
        }
    }

    public void UpdatePlayerScore(int playerId, uint score) {
        if (playerElements.TryGetValue(playerId, out var playerElement)) {
            var scoreLabel = playerElement.Q<Label>("player-score");
            scoreLabel.text = $"{score}";
        }
    }
}
