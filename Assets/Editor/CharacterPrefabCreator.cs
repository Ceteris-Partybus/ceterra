using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CharacterPrefabCreator : EditorWindow {
    public GameObject[] fbxAssets;
    public RuntimeAnimatorController baseController;
    public string prefabSavePath = "Assets/Level/Prefabs/Player/Prefabs/";
    public string animationSetSavePath = "Assets/Level/Prefabs/Player/AnimationSets/";

    [MenuItem("Tools/Character Prefab Creator")]
    static void OpenWindow() {
        GetWindow<CharacterPrefabCreator>("Character Prefab Creator");
    }

    void OnGUI() {
        GUILayout.Label("Create Character Prefab", EditorStyles.boldLabel);

        SerializedObject so = new SerializedObject(this);
        SerializedProperty fbxsProp = so.FindProperty("fbxAssets");
        EditorGUILayout.PropertyField(fbxsProp, new GUIContent("FBX Assets"), true);
        so.ApplyModifiedProperties();

        baseController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Base Controller", baseController, typeof(RuntimeAnimatorController), false);
        prefabSavePath = EditorGUILayout.TextField("Prefab Save Path", prefabSavePath);
        animationSetSavePath = EditorGUILayout.TextField("Animation Set Save Path", animationSetSavePath);

        if (GUILayout.Button("Create Prefab")) {
            if (fbxAssets == null || fbxAssets.Length == 0 || baseController == null) {
                Debug.LogWarning("Please assign FBX and Base Controller.");
                return;
            }
            EnsureFolderExists(prefabSavePath);
            EnsureFolderExists(animationSetSavePath);

            fbxAssets.Where(fbx => fbx != null).ToList().ForEach(fbx => CreateCharacterPrefab(fbx));

            AssetDatabase.Refresh();
        }
    }

    void CreateCharacterPrefab(GameObject fbx) {
        var instance = PrefabUtility.InstantiatePrefab(fbx) as GameObject;
        instance.AddComponent<Animator>();

        var animationSet = CreateInstance<CharacterAnimationSet>();
        animationSet.fbxAsset = fbx;
        CharacterAnimationSetEditor.AutoFill(animationSet);

        AssetDatabase.CreateAsset(animationSet, animationSetSavePath + fbx.name + ".asset");
        AssetDatabase.SaveAssets();

        var loader = instance.AddComponent<CharacterAnimatorLoader>();
        loader.baseController = baseController;
        loader.animationSet = animationSet;

        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabSavePath + fbx.name + ".prefab");
        AddPrefabToGameManager(savedPrefab);
        DestroyImmediate(instance);

        Debug.Log($"[Editor] Created and saved prefab and animation set for {fbx.name}");
    }

    private static void EnsureFolderExists(string folderPath) {
        if (AssetDatabase.IsValidFolder(folderPath)) { return; }

        var parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
        var newFolderName = Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent)) {
            EnsureFolderExists(parent);
        }

        AssetDatabase.CreateFolder(parent, newFolderName);
        Debug.Log($"Created folder: {folderPath}");
    }

    private static void AddPrefabToGameManager(GameObject prefab) {
        if (EditorSceneManager.GetActiveScene().name != "Bootstrap") {
            Debug.Log($"Cannot add prefab to GameManager.selectableCharacters unless Bootstrap scene is open.");
            return;
        }

        var gameManager = FindAnyObjectByType<GameManager>();
        var tmp = new List<GameObject>(gameManager.SelectableCharacters?.Where(x => x != null) ?? new GameObject[0]);
        if (!tmp.Contains(prefab)) {
            Undo.RecordObject(gameManager, "Add Selectable Character");
            tmp.Add(prefab);
            gameManager.SelectableCharacters = tmp.ToArray();

            EditorUtility.SetDirty(gameManager);
            Debug.Log($"[Editor] Added {prefab.name} to GameManager.selectableCharacters");
        }
    }
}