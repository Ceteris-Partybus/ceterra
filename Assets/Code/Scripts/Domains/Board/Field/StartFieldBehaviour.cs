using Mirror;
using System.Collections;
using UnityEngine;

public class StartFieldBehaviour : FieldBehaviour {
    protected override void OnPlayerCross(BoardPlayer player) {
        base.OnPlayerCross(player);
        GameManager.Singleton.IncrementPlayersPassedStart();
    }
}
