using Mirror;
using UnityEngine;

public class MgGarbageBin : NetworkBehaviour {
    [SerializeField]
    private MgGarbageTrashType acceptedTrashType;
    public MgGarbageTrashType AcceptedTrashType => acceptedTrashType;
}