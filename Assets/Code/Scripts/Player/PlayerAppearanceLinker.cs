using UnityEngine;
using Mirror;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PlayerAppearanceLinker : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider playerCollider;
    
    [SyncVar(hook = nameof(OnAppearanceIndexChanged))]
    private int appearanceIndex = 0;
    
    private void Awake()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerCollider == null) playerCollider = GetComponent<Collider>();
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        // Apply appearance when client starts
        ApplyAppearance(appearanceIndex);
    }
    
    // Remove [Server] attribute to allow both server and client calls
    public void SetAppearance(int index)
    {
        if (NetworkServer.active)
        {
            // If we're on server, update the SyncVar
            appearanceIndex = index;
        }
        
        // Always apply appearance locally (for immediate feedback)
        ApplyAppearance(index);
    }
    
    private void OnAppearanceIndexChanged(int oldIndex, int newIndex)
    {
        ApplyAppearance(newIndex);
    }
    
    private void ApplyAppearance(int index)
    {
        if (PlayerAppearanceController.Instance == null) return;
        
        var appearanceData = PlayerAppearanceController.Instance.GetAppearanceByIndex(index);
        if (appearanceData == null) return;
        
        // Update mesh
        if (meshFilter != null && appearanceData.playerMesh != null)
        {
            meshFilter.mesh = appearanceData.playerMesh;
        }
        
        // Update materials
        if (meshRenderer != null && appearanceData.materials != null && appearanceData.materials.Length > 0)
        {
            meshRenderer.materials = appearanceData.materials;
        }
        
        // Update animator controller
        if (animator != null && appearanceData.animatorController != null)
        {
            animator.runtimeAnimatorController = appearanceData.animatorController;
        }
        
        // Update collider based on mesh (optional)
        if (playerCollider is MeshCollider meshCol && appearanceData.playerMesh != null)
        {
            meshCol.sharedMesh = appearanceData.playerMesh;
        }
    }
    
    public int GetCurrentAppearanceIndex()
    {
        return appearanceIndex;
    }
}
