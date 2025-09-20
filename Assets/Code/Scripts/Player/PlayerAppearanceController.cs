using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerAppearanceData
{
    public string name;
    public Mesh playerMesh;
    public Material[] materials;
    public RuntimeAnimatorController animatorController;
    public Sprite icon; // For GUI display
    
    [Header("Skinned Mesh Support")]
    public bool useSkinnedMesh;
    public SkinnedMeshRenderer skinnedMeshPrefab; // Reference to a prefab with SkinnedMeshRenderer
    public Mesh skinnedMesh;
    public Material[] skinnedMaterials;
}

[CreateAssetMenu(fileName = "PlayerAppearanceController", menuName = "Game/Player Appearance Controller")]
public class PlayerAppearanceController : ScriptableObject
{
    private static PlayerAppearanceController _instance;
    
    public static PlayerAppearanceController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<PlayerAppearanceController>("PlayerAppearanceController");
                if (_instance == null)
                {
                    Debug.LogError("PlayerAppearanceController not found in Resources folder!");
                }
            }
            return _instance;
        }
    }

    [SerializeField]
    private List<PlayerAppearanceData> availableAppearances = new List<PlayerAppearanceData>();
    
    public List<PlayerAppearanceData> AvailableAppearances => availableAppearances;
    
    public PlayerAppearanceData GetAppearanceByIndex(int index)
    {
        if (index >= 0 && index < availableAppearances.Count)
            return availableAppearances[index];
        return availableAppearances.Count > 0 ? availableAppearances[0] : null;
    }
    
    public int GetAppearanceIndex(string appearanceName)
    {
        for (int i = 0; i < availableAppearances.Count; i++)
        {
            if (availableAppearances[i].name == appearanceName)
                return i;
        }
        return 0;
    }
}
