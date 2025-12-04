using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MgOcean3DRemotePlayerHUD : NetworkedSingleton<MgOcean3DRemotePlayerHUD> {
    [SerializeField]
    private UIDocument uiDocument; // Should have LocalPlayerHUD.uxml assigned (contains remote-player-overview)

    private VisualElement remotePlayerOverview;

    private Dictionary<int, VisualElement> playerElements = new Dictionary<int, VisualElement>();
    [SerializeField]
    private VisualTreeAsset playerEntryTemplate;

    void OnEnable() {
        if (uiDocument == null) {
            Debug.LogError("[MgOcean3DRemotePlayerHUD] UIDocument not assigned!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        remotePlayerOverview = root.Q<VisualElement>("remote-player-overview");
        
        if (remotePlayerOverview == null) {
            Debug.LogError("[MgOcean3DRemotePlayerHUD] Could not find 'remote-player-overview' element in UI document!");
        }
    }

    public bool IsPlayerAdded(int playerId) {
        return playerElements.ContainsKey(playerId);
    }

    public void AddPlayer(MgOceanPlayer3D player) {
        if (remotePlayerOverview == null) {
            Debug.LogWarning("[MgOcean3DRemotePlayerHUD] remotePlayerOverview not initialized yet!");
            return;
        }
        
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

    public void UpdatePlayerScore(int playerId, int score) {
        if (playerElements.TryGetValue(playerId, out var playerElement)) {
            var scoreLabel = playerElement.Q<Label>("player-score");
            scoreLabel.text = $"{score}";
        }
    }
}