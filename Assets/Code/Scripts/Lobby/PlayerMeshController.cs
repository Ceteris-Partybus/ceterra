using Mirror;
using UnityEngine;

public class PlayerMeshController : NetworkBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Mesh[] availableMeshes;

    [SyncVar(hook = nameof(OnMeshChanged))]
    private int currentMeshIndex = 0;

    [SyncVar] private bool isLocked = false;

    public int MeshCount => availableMeshes != null ? availableMeshes.Length : 0;
    public int CurrentMeshIndex => currentMeshIndex;

    public override void OnStartClient()
    {
        OnMeshChanged(0, currentMeshIndex);
    }


    [Command]
    public void CmdChangeMesh(int newIndex)
    {
        if (newIndex < 0 || newIndex >= MeshCount) return;
        if (isLocked) return; // prevent switching when locked
        currentMeshIndex = newIndex;
    }

    [Server]
    public void SetMeshIndex(int index)
    {
        if (index < 0 || index >= MeshCount) return;
        currentMeshIndex = index;
    }

    [Command]
    public void CmdLockCharacter(int index)
    {
        if (index < 0 || index >= MeshCount) return;
        currentMeshIndex = index;
        isLocked = true;
    }

    [Command]
    public void CmdUnlockCharacter()
    {
        isLocked = false;
    }

    private void OnMeshChanged(int oldIndex, int newIndex)
    {
        if (meshFilter != null && MeshCount > 0)
            meshFilter.mesh = availableMeshes[newIndex];
    }
}
