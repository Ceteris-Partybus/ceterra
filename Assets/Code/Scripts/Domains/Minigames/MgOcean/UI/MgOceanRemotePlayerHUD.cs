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
        Debug.Log($"[MgOceanRemotePlayerHUD] OnEnable: remotePlayerOverview found? {remotePlayerOverview != null}");
    }

    public bool IsPlayerAdded(int playerId) {
        return playerElements.ContainsKey(playerId);
    }

    public void AddPlayer(MgOceanPlayer player) {
        Debug.Log($"[MgOceanRemotePlayerHUD] AddPlayer called for PlayerId: {player.PlayerId}");
        if (remotePlayerOverview == null) return;
        var playerElement = playerEntryTemplate.CloneTree();
        playerElement.Q<Label>("player-name").text = player.PlayerName;
        playerElement.Q<Label>("player-score").text = "0";
        remotePlayerOverview.Add(playerElement);
        playerElements[player.PlayerId] = playerElement;
    }

    public void RemovePlayer(int playerId) {
        Debug.Log($"[MgOceanRemotePlayerHUD] RemovePlayer called for PlayerId: {playerId}");
        if (playerElements.TryGetValue(playerId, out var playerElement)) {
            if (remotePlayerOverview != null) {
                remotePlayerOverview.Remove(playerElement);
            }
            playerElements.Remove(playerId);
        }
    }

    public void UpdatePlayerScore(int playerId, int score) {
        Debug.Log($"[MgOceanRemotePlayerHUD] UpdatePlayerScore called for PlayerId: {playerId} with score: {score}");
        if (playerElements.TryGetValue(playerId, out var playerElement)) {
            var scoreLabel = playerElement.Q<Label>("player-score");
            scoreLabel.text = $"{score}";
        }
    }
}
