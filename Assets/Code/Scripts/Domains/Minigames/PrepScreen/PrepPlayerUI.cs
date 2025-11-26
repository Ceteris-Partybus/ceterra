using UnityEngine.UIElements;

public class PrepPlayerUI {
    private Button readyButton;
    private Label readyStatusDisplayLabel;
    private SceneConditionalPlayer player;
    public bool isPlayerReady => player.IsReady;

    public PrepPlayerUI(VisualElement playerPrepSlot, SceneConditionalPlayer player) {
        playerPrepSlot.parent.parent.style.display = DisplayStyle.Flex;
        playerPrepSlot.Q<Label>("PlayerName").text = player.PlayerName;
        this.readyButton = playerPrepSlot.Q<Button>("readyButton");
        if (player.isLocalPlayer) {
            this.readyButton.SetEnabled(true);
            this.readyButton.clicked += () => player.CmdChangeReadyState();
        }
        else {
            this.readyButton.style.display = DisplayStyle.None;
        }
        this.readyStatusDisplayLabel = playerPrepSlot.Q<Label>("ready-status-display-label");
        this.player = player;
    }

    public void UpdateMe() {
        readyButton.EnableInClassList("not-ready", !isPlayerReady);
        var isReadyText = LocalizationManager.Instance.GetLocalizedText(61453348007936000);
        var isNotReadyText = LocalizationManager.Instance.GetLocalizedText(61453405545398272);
        readyStatusDisplayLabel.text = isPlayerReady ? isReadyText : isNotReadyText;
        readyStatusDisplayLabel.parent.EnableInClassList("ready", isPlayerReady);
        readyStatusDisplayLabel.parent.style.display = !player.isLocalPlayer ? DisplayStyle.Flex : DisplayStyle.None;
        var clickToReadyText = LocalizationManager.Instance.GetLocalizedText(61453992487911424);
        var clickToUnreadyText = LocalizationManager.Instance.GetLocalizedText(61453481038675968);
        readyButton.text = isPlayerReady ? clickToUnreadyText : clickToReadyText;
    }
}