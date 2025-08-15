using UnityEngine;
using UnityEngine.UIElements;

public class LocalPlayerHUD : NetworkedSingleton<LocalPlayerHUD> {
    [SerializeField]
    private UIDocument uiDocument;

    private Label scoreLabel;

    protected override void Start() {
        base.Start();
        var root = uiDocument.rootVisualElement;
        scoreLabel = root.Q<Label>("local-player-score");
    }

    public void UpdateScore(uint score) {
        scoreLabel.text = $"{score}";
    }
}