using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CharacterPrefabCreator : EditorWindow {
    public GameObject[] fbxAssets;
    public RuntimeAnimatorController baseController;
    public string prefabSavePath = "Assets/Level/Prefabs/Player/SelectableCharacters/";
    public float scale = .6f;

    [MenuItem("Tools/Character Prefab Creator")]
    static void OpenWindow() {
        GetWindow<CharacterPrefabCreator>("Character Prefab Creator");
    }

    void OnGUI() {
        GUILayout.Label("Create Character Prefab", EditorStyles.boldLabel);

        var so = new SerializedObject(this);
        var fbxsProp = so.FindProperty("fbxAssets");
        EditorGUILayout.PropertyField(fbxsProp, new GUIContent("FBX Assets"), true);
        so.ApplyModifiedProperties();

        baseController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Base Controller", baseController, typeof(RuntimeAnimatorController), false);
        prefabSavePath = EditorGUILayout.TextField("Prefab Save Path", prefabSavePath);
        scale = EditorGUILayout.FloatField("Scale", scale);

        if (GUILayout.Button("Create Prefab")) {
            if (fbxAssets == null || fbxAssets.Length == 0 || baseController == null) {
                Debug.LogWarning("[Editor] Please assign FBX and Base Controller.");
                return;
            }
            EnsureFolderExists(prefabSavePath);

            fbxAssets.Where(fbx => fbx != null).ToList().ForEach(fbx => CreateCharacterPrefab(fbx));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    void CreateCharacterPrefab(GameObject fbx) {
        var fbxPath = AssetDatabase.GetAssetPath(fbx);
        ConfigureAnimationSettings(fbxPath);

        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        var mesh = assets.OfType<Mesh>().FirstOrDefault();

        var instance = PrefabUtility.InstantiatePrefab(fbx) as GameObject;
        instance.transform.localScale = Vector3.one * scale;

        var collider = instance.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        var loader = instance.AddComponent<CharacterAnimatorLoader>();
        loader.baseController = baseController;

        AutoFillClips(loader, assets);

        var character = instance.AddComponent<Character>();
        character.CharacterName = fbx.name;

        var prefabPath = prefabSavePath + fbx.name + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        AddPrefabToGameManager(savedPrefab);
        DestroyImmediate(instance);

        Debug.Log($"[Editor] Created a prefab for {fbx.name}");
    }

    private static void ConfigureAnimationSettings(string fbxPath) {
        var modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        var animations = modelImporter.defaultClipAnimations;
        foreach (var clip in animations) {
            var cleanName = CleanClipName(clip.name);
            if (cleanName == "run" || cleanName == "idle") {
                clip.loopTime = true;
                clip.loopPose = true;
            }
        }

        modelImporter.clipAnimations = animations;
        modelImporter.SaveAndReimport();
    }

    private static void EnsureFolderExists(string folderPath) {
        if (AssetDatabase.IsValidFolder(folderPath)) { return; }

        var parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
        var newFolderName = Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent)) {
            EnsureFolderExists(parent);
        }

        AssetDatabase.CreateFolder(parent, newFolderName);
        Debug.Log($"[Editor] Created folder: {folderPath}");
    }

    private static void AddPrefabToGameManager(GameObject prefab) {
        if (EditorSceneManager.GetActiveScene().name != "Bootstrap") {
            Debug.Log($"[Editor] Cannot add prefab to GameManager.selectableCharacters unless Bootstrap scene is open.");
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

    private static void AutoFillClips(CharacterAnimatorLoader loader, Object[] assets) {
        var clips = assets
            .OfType<AnimationClip>()
            .Where(clip => !clip.name.ToLower().Contains("preview"))
            .ToArray();

        if (clips == null || clips.Length == 0) {
            Debug.LogWarning("[Editor] No clips found in FBX!");
            return;
        }

        Undo.RecordObject(loader, "Auto-Fill Character Animation Clips");
        loader.animationClips = clips.Select(clip => {
            var cleanName = CleanClipName(clip.name);
            Debug.Log($"[Editor] Auto-filled clip: {cleanName}, name before cleaning: {clip.name}");
            return new CharacterAnimatorLoader.ClipEntry(cleanName, clip);
        }).ToList();

        EditorUtility.SetDirty(loader);
    }

    private static string CleanClipName(string name) {
        return name.Split('|').Last().ToLower().Trim();
    }
}