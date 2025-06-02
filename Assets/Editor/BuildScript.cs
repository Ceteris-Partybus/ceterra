using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Dedicated Server (macOS)")]
    public static void BuildDedicatedServerMacOS()
    {
        BuildDedicatedServer(BuildTarget.StandaloneOSX, "Mac", "DedicatedServer.app");
    }

    [MenuItem("Build/Build Dedicated Server (Linux)")]
    public static void BuildDedicatedServerLinux()
    {
        BuildDedicatedServer(BuildTarget.StandaloneLinux64, "Linux", "DedicatedServer");
    }

    private static void BuildDedicatedServer(BuildTarget targetPlatform, string platformFolder, string executableName)
    {
        string buildPath = Path.Combine(Application.dataPath, $"../Build/{platformFolder}/Server");
        
        // Ensure build directory exists
        Directory.CreateDirectory(buildPath);
        
        // Get scenes and validate them
        string[] scenePaths = GetEnabledScenes();
        
        if (scenePaths.Length == 0)
        {
            Debug.LogError("No scenes found! Please add scenes to Build Settings.");
            return;
        }
        
        // Debug: Print scene paths
        Debug.Log($"Building {platformFolder} server with {scenePaths.Length} scenes:");
        foreach (string scene in scenePaths)
        {
            Debug.Log($"Scene: {scene}");
        }
        
        // Store original settings
        BuildTarget originalActiveTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup originalTargetGroup = BuildPipeline.GetBuildTargetGroup(originalActiveTarget);
        ScriptingImplementation originalScriptingBackend = PlayerSettings.GetScriptingBackend(originalTargetGroup);
        
        ManagedStrippingLevel originalStandaloneStrippingLevel = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Standalone);
        bool originalStripEngineCode = PlayerSettings.stripEngineCode;
        BuildTarget originalSelectedStandaloneTarget = EditorUserBuildSettings.selectedStandaloneTarget;

        try
        {
            // Force Mono backend and apply other necessary settings
            ForceMonoBackend(targetPlatform);
            
            // Build options for dedicated server
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenePaths;
            buildPlayerOptions.locationPathName = Path.Combine(buildPath, executableName);
            buildPlayerOptions.target = targetPlatform;
            buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
            
            // Use BuildOptions.EnableHeadlessMode for dedicated server builds
            buildPlayerOptions.options = BuildOptions.EnableHeadlessMode;
            
            // Create link.xml
            CreateComprehensiveLinkXml();
            Debug.Log("link.xml creation is enabled.");
            
            Debug.Log($"Starting {platformFolder} server build with Mono backend, no optimization, and headless mode...");
            
            // Build the server
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"{platformFolder} server build succeeded: {buildPlayerOptions.locationPathName}");
            }
            else
            {
                Debug.LogError($"{platformFolder} server build failed!");
            }
        }
        finally
        {
            // Restore original settings
            Debug.Log("Restoring original build settings...");

            // Switch back to the original active build target if it was changed
            if (EditorUserBuildSettings.activeBuildTarget != originalActiveTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(originalTargetGroup, originalActiveTarget);
            }

            // If the original target group was Standalone, restore its selected sub-target
            if (originalTargetGroup == BuildTargetGroup.Standalone) 
            {
                EditorUserBuildSettings.selectedStandaloneTarget = originalSelectedStandaloneTarget;
            }
            
            // Restore scripting backend for the original target group
            PlayerSettings.SetScriptingBackend(originalTargetGroup, originalScriptingBackend);
            
            // Restore stripping settings
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone, originalStandaloneStrippingLevel);
            PlayerSettings.stripEngineCode = originalStripEngineCode;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Original build settings restored.");
        }
    }
    
    private static void ForceMonoBackend(BuildTarget buildTarget)
    {
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;

        // Switch active build target if not already active
        if (EditorUserBuildSettings.activeBuildTarget != buildTarget || EditorUserBuildSettings.selectedStandaloneTarget != buildTarget)
        {
            Debug.Log($"Switching active build target to {buildTarget} for group {targetGroup}. Current active: {EditorUserBuildSettings.activeBuildTarget}, Current selected standalone: {EditorUserBuildSettings.selectedStandaloneTarget}");
            EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, buildTarget);
            EditorUserBuildSettings.selectedStandaloneTarget = buildTarget;
        }
        
        // Set scripting backend to Mono for Standalone group
        PlayerSettings.SetScriptingBackend(targetGroup, ScriptingImplementation.Mono2x);
        
        // Disable all stripping for Standalone group
        PlayerSettings.SetManagedStrippingLevel(targetGroup, ManagedStrippingLevel.Disabled);
        
        PlayerSettings.stripEngineCode = false;
        
        // Save settings to ensure they are applied before build
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Set scripting backend to Mono, disabled stripping for {targetGroup}. Active target: {EditorUserBuildSettings.activeBuildTarget}, Selected Standalone: {EditorUserBuildSettings.selectedStandaloneTarget}. Mono backend for Standalone: {PlayerSettings.GetScriptingBackend(targetGroup)}");
    }
    
    private static void CreateComprehensiveLinkXml()
    {
        string linkXmlPath = Path.Combine(Application.dataPath, "link.xml");
        
        // Create comprehensive link.xml with all needed assemblies
        string linkXmlContent = @"<linker>
  <!-- Core assemblies -->
  <assembly fullname=""Assembly-CSharp"" preserve=""all""/>
  <assembly fullname=""Unity.Multiplayer.Center.Common"" preserve=""all""/>
  <assembly fullname=""Unity.Collections"" preserve=""all""/>
  <assembly fullname=""Unity.ResourceManager"" preserve=""all""/>
  <assembly fullname=""Unity.Addressables"" preserve=""all""/>
  <assembly fullname=""Unity.Mathematics"" preserve=""all""/>
  <assembly fullname=""Unity.Burst"" preserve=""all""/>
  <assembly fullname=""Unity.Localization"" preserve=""all""/>
  <assembly fullname=""Unity.Profiling.Core"" preserve=""all""/>
  <assembly fullname=""Unity.InputSystem"" preserve=""all""/>

  <!-- Framework assemblies -->
  <assembly fullname=""mscorlib"" preserve=""all""/>
  <assembly fullname=""System"" preserve=""all""/>
  <assembly fullname=""System.Core"" preserve=""all""/>
  <assembly fullname=""System.Data"" preserve=""all""/>
  <assembly fullname=""System.Runtime.Serialization"" preserve=""all""/>
  <assembly fullname=""System.Xml"" preserve=""all""/>
  <assembly fullname=""System.Xml.Linq"" preserve=""all""/>

  <!-- Unity engine -->
  <assembly fullname=""UnityEngine"" preserve=""all""/>
  <assembly fullname=""UnityEngine.CoreModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.SharedInternalsModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.InputModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.InputLegacyModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.InputForUIModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.JSONSerializeModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.PhysicsModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.Physics2DModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.UnityWebRequestModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.AssetBundleModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.UI"" preserve=""all""/>
  <assembly fullname=""UnityEngine.UIModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.UIElementsModule"" preserve=""all""/>
  <assembly fullname=""UnityEngine.TextRenderingModule"" preserve=""all""/>
</linker>";
        
        File.WriteAllText(linkXmlPath, linkXmlContent);
        Debug.Log($"Created comprehensive link.xml at {linkXmlPath}");
        
        // Force asset database refresh to ensure Unity picks up the file
        AssetDatabase.Refresh();
    }
    
    private static string[] GetEnabledScenes()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        System.Collections.Generic.List<string> enabledScenes = new System.Collections.Generic.List<string>();
        
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].enabled && !string.IsNullOrEmpty(scenes[i].path))
            {
                // Ensure the scene file exists
                if (File.Exists(scenes[i].path))
                {
                    enabledScenes.Add(scenes[i].path);
                }
                else
                {
                    Debug.LogWarning($"Scene file not found: {scenes[i].path}");
                }
            }
        }
        
        return enabledScenes.ToArray();
    }
}
