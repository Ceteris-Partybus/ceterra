using Mirror;
using UnityEngine;

public abstract class Player : NetworkBehaviour {
    [Header("Player")]
    [SerializeField]
    [SyncVar]
    protected int id;
    public int Id {
        get => id;
        set => id = value;
    }

    [SerializeField]
    [SyncVar]
    protected string playerName;
    public string PlayerName {
        get => playerName;
        set => playerName = value;
    }
}