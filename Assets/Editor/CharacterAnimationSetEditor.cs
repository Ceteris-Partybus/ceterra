using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(CharacterAnimationSet))]
public class CharacterAnimationSetEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var animationSet = (CharacterAnimationSet)target;

        if (GUILayout.Button("Auto-Fill From FBX")) {
            AutoFill(animationSet);
        }
    }

    public static void AutoFill(CharacterAnimationSet animationSet) {
        if (animationSet.fbxAsset == null) {
            Debug.LogWarning("No FBX assigned!");
            return;
        }

        var fbxPath = AssetDatabase.GetAssetPath(animationSet.fbxAsset);
        if (string.IsNullOrEmpty(fbxPath)) {
            Debug.LogWarning("Could not find FBX path!");
            return;
        }

        var clips = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
            .OfType<AnimationClip>()
            .Where(clip => !clip.name.ToLower().Contains("preview"))
            .ToArray();
        if (clips == null || clips.Length == 0) {
            Debug.LogWarning("No clips found in FBX!");
            return;
        }

        Undo.RecordObject(animationSet, "Auto-Fill Character Animation Clips");
        animationSet.clips.Clear();
        animationSet.clips = clips.Select(clip => new CharacterAnimationSet.ClipEntry {
            name = CleanClipName(clip.name),
            clip = clip
        }).ToList();

        EditorUtility.SetDirty(animationSet);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Editor] Filled {animationSet.clips.Count} clips for {animationSet.name}");
    }

    private static string CleanClipName(string name) {
        return name.Split('|').Last().ToLower().Trim();
    }
}
