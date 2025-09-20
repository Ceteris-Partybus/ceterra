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
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    
    [SyncVar(hook = nameof(OnAppearanceIndexChanged))]
    private int appearanceIndex = 0;
    
    [SyncVar(hook = nameof(OnMeshChanged))]
    private string meshAssetPath = "";
    
    [SyncVar(hook = nameof(OnMaterialsChanged))]
    private string[] materialAssetPaths = new string[0];
    
    [SyncVar(hook = nameof(OnUseSkinnedMeshChanged))]
    private bool useSkinnedMesh = false;
    
    [SyncVar(hook = nameof(OnSkinnedMeshChanged))]
    private string skinnedMeshAssetPath = "";
    
    [SyncVar(hook = nameof(OnSkinnedMaterialsChanged))]
    private string[] skinnedMaterialAssetPaths = new string[0];
    
    private void Awake()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerCollider == null) playerCollider = GetComponent<Collider>();
        if (skinnedMeshRenderer == null) skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
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
            
            // Update mesh and materials data
            var appearanceData = PlayerAppearanceController.Instance?.GetAppearanceByIndex(index);
            if (appearanceData != null)
            {
                // Store asset paths for synchronization
                meshAssetPath = GetAssetPath(appearanceData.playerMesh);
                materialAssetPaths = GetMaterialAssetPaths(appearanceData.materials);
                
                // Store skinned mesh data
                useSkinnedMesh = appearanceData.useSkinnedMesh;
                skinnedMeshAssetPath = GetAssetPath(appearanceData.skinnedMesh);
                skinnedMaterialAssetPaths = GetMaterialAssetPaths(appearanceData.skinnedMaterials);
            }
        }
        
        // Always apply appearance locally (for immediate feedback)
        ApplyAppearance(index);
    }
    
    private void OnAppearanceIndexChanged(int oldIndex, int newIndex)
    {
        ApplyAppearance(newIndex);
    }
    
    private void OnMeshChanged(string oldPath, string newPath)
    {
        if (!string.IsNullOrEmpty(newPath))
        {
            ApplyMeshFromPath(newPath);
        }
    }
    
    private void OnMaterialsChanged(string[] oldPaths, string[] newPaths)
    {
        if (newPaths != null && newPaths.Length > 0)
        {
            ApplyMaterialsFromPaths(newPaths);
        }
    }
    
    private void OnUseSkinnedMeshChanged(bool oldValue, bool newValue)
    {
        SwitchRenderingMode(newValue);
    }
    
    private void OnSkinnedMeshChanged(string oldPath, string newPath)
    {
        if (!string.IsNullOrEmpty(newPath))
        {
            ApplySkinnedMeshFromPath(newPath);
        }
    }
    
    private void OnSkinnedMaterialsChanged(string[] oldPaths, string[] newPaths)
    {
        if (newPaths != null && newPaths.Length > 0)
        {
            ApplySkinnedMaterialsFromPaths(newPaths);
        }
    }
    
    private void ApplyAppearance(int index)
    {
        if (PlayerAppearanceController.Instance == null) return;
        
        var appearanceData = PlayerAppearanceController.Instance.GetAppearanceByIndex(index);
        if (appearanceData == null) return;
        
        // Switch between regular mesh and skinned mesh
        SwitchRenderingMode(appearanceData.useSkinnedMesh);
        
        if (appearanceData.useSkinnedMesh)
        {
            // Apply skinned mesh data
            ApplySkinnedMesh(appearanceData.skinnedMesh);
            ApplySkinnedMaterials(appearanceData.skinnedMaterials);
        }
        else
        {
            // Apply regular mesh data
            ApplyMesh(appearanceData.playerMesh);
            ApplyMaterials(appearanceData.materials);
        }
        
        // Update animator controller
        if (animator != null && appearanceData.animatorController != null)
        {
            animator.runtimeAnimatorController = appearanceData.animatorController;
        }
        
        // Update collider based on mesh (optional)
        UpdateCollider(appearanceData);
    }
    
    private void SwitchRenderingMode(bool useSkinned)
    {
        if (meshRenderer != null)
            meshRenderer.enabled = !useSkinned;
        
        if (skinnedMeshRenderer != null)
            skinnedMeshRenderer.enabled = useSkinned;
    }
    
    private void ApplyMesh(Mesh mesh)
    {
        if (meshFilter != null && mesh != null)
        {
            meshFilter.mesh = mesh;
        }
    }
    
    private void ApplyMaterials(Material[] materials)
    {
        if (meshRenderer != null && materials != null && materials.Length > 0)
        {
            meshRenderer.materials = materials;
        }
    }
    
    private void ApplySkinnedMesh(Mesh mesh)
    {
        if (skinnedMeshRenderer != null && mesh != null)
        {
            skinnedMeshRenderer.sharedMesh = mesh;
        }
    }
    
    private void ApplySkinnedMaterials(Material[] materials)
    {
        if (skinnedMeshRenderer != null && materials != null && materials.Length > 0)
        {
            skinnedMeshRenderer.materials = materials;
        }
    }
    
    private void ApplyMeshFromPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return;
        
#if UNITY_EDITOR
        var mesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        ApplyMesh(mesh);
#else
        // In build, you might need to use Resources.Load or Addressables
        Debug.LogWarning("Mesh loading from path not implemented for builds");
#endif
    }
    
    private void ApplyMaterialsFromPaths(string[] assetPaths)
    {
        if (assetPaths == null || assetPaths.Length == 0) return;
        
#if UNITY_EDITOR
        Material[] materials = new Material[assetPaths.Length];
        for (int i = 0; i < assetPaths.Length; i++)
        {
            materials[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPaths[i]);
        }
        ApplyMaterials(materials);
#else
        // In build, you might need to use Resources.Load or Addressables
        Debug.LogWarning("Material loading from paths not implemented for builds");
#endif
    }
    
    private void ApplySkinnedMeshFromPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return;
        
#if UNITY_EDITOR
        var mesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        ApplySkinnedMesh(mesh);
#else
        Debug.LogWarning("Skinned mesh loading from path not implemented for builds");
#endif
    }
    
    private void ApplySkinnedMaterialsFromPaths(string[] assetPaths)
    {
        if (assetPaths == null || assetPaths.Length == 0) return;
        
#if UNITY_EDITOR
        Material[] materials = new Material[assetPaths.Length];
        for (int i = 0; i < assetPaths.Length; i++)
        {
            materials[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPaths[i]);
        }
        ApplySkinnedMaterials(materials);
#else
        Debug.LogWarning("Skinned material loading from paths not implemented for builds");
#endif
    }
    
    private void UpdateCollider(PlayerAppearanceData appearanceData)
    {
        if (playerCollider is MeshCollider meshCol)
        {
            Mesh colliderMesh = appearanceData.useSkinnedMesh ? 
                appearanceData.skinnedMesh : appearanceData.playerMesh;
            
            if (colliderMesh != null)
            {
                meshCol.sharedMesh = colliderMesh;
            }
        }
    }
    
    private string GetAssetPath(Object asset)
    {
        if (asset == null) return "";
        
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.GetAssetPath(asset);
#else
        return "";
#endif
    }
    
    private string[] GetMaterialAssetPaths(Material[] materials)
    {
        if (materials == null) return new string[0];
        
        string[] paths = new string[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            paths[i] = GetAssetPath(materials[i]);
        }
        return paths;
    }
    
    public int GetCurrentAppearanceIndex()
    {
        return appearanceIndex;
    }
}
