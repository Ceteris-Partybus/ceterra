using System.Collections;
using UnityEngine;

public class MinigameOneContext : NetworkedSingleton<MinigameOneContext> {

    protected override void Start() {
        base.Start();
        StartCoroutine(EndMinigameAfterDelay());
    }

    private IEnumerator EndMinigameAfterDelay() {
        yield return new WaitForSeconds(30f);
        GameManager.singleton.EndMinigame();
    }
}