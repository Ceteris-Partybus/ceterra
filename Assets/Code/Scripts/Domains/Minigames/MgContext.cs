using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IPrepScreen {
    string GetTitle();
    string GetDescription();
    string GetControls();
    Sprite GetScreenshot();
    List<SceneConditionalPlayer> GetPlayers();
}

public abstract class MgContext<T, P> : NetworkedSingleton<T>, IPrepScreen
    where T : NetworkedSingleton<T>
    where P : SceneConditionalPlayer {

    [SerializeField] private string title;
    [SerializeField] private string description;
    [SerializeField] private string controls;
    [SerializeField] private Sprite screenshot;
    public string GetTitle() => this.title;
    public string GetDescription() => this.description;
    public string GetControls() => this.controls;
    public Sprite GetScreenshot() => this.screenshot;

    protected override void Start() {
        var prep = FindAnyObjectByType<PrepScreenUI>();
        prep.Initialize(this as IPrepScreen);
        base.Start();
    }

    public List<SceneConditionalPlayer> GetPlayers() {
        return FindObjectsByType<P>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Select(p => (SceneConditionalPlayer)p).ToList();
    }
}