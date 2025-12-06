using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class MgMemoryController : NetworkedSingleton<MgMemoryController> {
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Canvas canvas;

    private Label scoreLabel;
    private Label currentPlayerLabel;

    private VisualElement factPopup;
    private Label factTitle;
    private Label factDescription;
    private VisualElement factImage;
    private Label progressText;

    protected override void Start() {
        base.Start();

        var root = uiDocument.rootVisualElement;

        scoreLabel = root.Q<Label>("score-label");
        currentPlayerLabel = root.Q<Label>("current-player-label");
        factPopup = root.Q<VisualElement>("fact-popup");
        factTitle = root.Q<Label>("fact-title");
        factDescription = root.Q<Label>("fact-description");
        factImage = root.Q<VisualElement>("fact-image");
        progressText = root.Q<Label>("progress-text");
    }

    public void UpdatePlayerScore(int score) {
        scoreLabel.text = score.ToString();
    }

    public void UpdateCurrentPlayer(string playerName) {
        currentPlayerLabel.text = playerName;
    }

    [Server]
    public void ShowFactPopup(MemoryFactData factData, float duration) {
        RpcShowFactPopup(factData, duration);
    }

    [ClientRpc]
    private void RpcShowFactPopup(MemoryFactData factData, float duration) {
        Audiomanager.Instance?.PlaySuccessSound();

        canvas.sortingOrder = 0;
        factTitle.text = factData.title;
        this.factDescription.text = factData.description;

        var sprite = Resources.Load<Sprite>(factData.imagePath);
        factImage.style.backgroundImage = new StyleBackground(sprite);

        SetElementDisplay(factPopup, true);

        StartCoroutine(HideFactPopupAfterDelay(duration));
        StartCoroutine(UpdateFactPopupCountdown(duration));
    }

    private IEnumerator UpdateFactPopupCountdown(float duration) {
        var elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            var remainingSeconds = Mathf.CeilToInt(duration - elapsed);
            progressText.text = remainingSeconds.ToString();
            yield return null;
        }

        progressText.text = "0";
    }

    private IEnumerator HideFactPopupAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        SetElementDisplay(factPopup, false);

        canvas.sortingOrder = 1;
    }

    private void SetElementDisplay(VisualElement element, bool visible) {
        if (element != null) {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}