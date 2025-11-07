using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public interface IPrepScreen {
    string GetTitle();
    string GetDescription();
    string GetControls();
    Sprite GetScreenshot();
    List<SceneConditionalPlayer> GetPlayers();
    void StartGame();
}