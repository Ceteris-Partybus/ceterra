using System.Collections.Generic;
using UnityEngine;

public interface IPrepScreen {
    string GetTitle();
    string GetDescription();
    string GetControls();
    Sprite GetScreenshot();
    List<SceneConditionalPlayer> GetPlayers();
    void StartGame();
}