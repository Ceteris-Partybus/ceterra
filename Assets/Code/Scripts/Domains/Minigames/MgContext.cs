using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
        prep.Initialize(this);
        base.Start();
    }

    public List<SceneConditionalPlayer> GetPlayers() {
        return FindObjectsByType<P>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Select(p => (SceneConditionalPlayer)p).ToList();
    }
}