using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MgContext<T, P> : NetworkedSingleton<T>, IPrepScreen
    where T : NetworkedSingleton<T>
    where P : SceneConditionalPlayer {

    [SerializeField] private long title;
    [SerializeField] private long description;
    [SerializeField] private long controls;
    [SerializeField] private Sprite screenshot;
    public string GetTitle() => LocalizationManager.Instance.GetLocalizedText(this.title);
    public string GetDescription() => LocalizationManager.Instance.GetLocalizedText(this.description);
    public string GetControls() => LocalizationManager.Instance.GetLocalizedText(this.controls);
    public Sprite GetScreenshot() => this.screenshot;
    public abstract void OnStartGame();
    public virtual void StartGame() {
        if (isClient) {
            SkyboxManager.Instance.OnMinigameStarted();
        }
        this.OnStartGame();
    }

    protected override void Start() {
        var prep = FindAnyObjectByType<PrepScreenUI>();
        prep.Initialize(this);
    }

    public List<SceneConditionalPlayer> GetPlayers() {
        return FindObjectsByType<P>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Select(p => (SceneConditionalPlayer)p).ToList();
    }
}