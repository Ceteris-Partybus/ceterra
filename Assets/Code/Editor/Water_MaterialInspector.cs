using UnityEditor;
using UnityEngine;

// Minimal fallback custom inspector for the shader that declares:
// CustomEditor = Water_MaterialInspector
// Must be in an Editor folder and inherit from ShaderGUI or MaterialEditor.
public class Water_MaterialInspector : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // Optional header so you can see the custom inspector is active
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Water Material (fallback inspector)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Draw default shader properties so material remains editable
        materialEditor.PropertiesDefaultGUI(properties);

        // Optionally add help / fallback info
        EditorGUILayout.HelpBox("This is a fallback inspector. Replace with the asset's proper inspector or update the shader's CustomEditor to the correct fully-qualified class name.", MessageType.Info);
    }
}
