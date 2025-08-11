using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using Mirror;

public class DockerServerBuilder : EditorWindow
{
    [MenuItem("Tools/Docker Server Builder")]
    public static void ShowWindow()
    {
        GetWindow<DockerServerBuilder>("Docker Server Builder");
    }

    void OnGUI() {
        GUILayout.Label("Unity Linux Server Docker Builder", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Check Docker status
        bool dockerRunning = IsDockerRunning();
        if (!dockerRunning) {
            EditorGUILayout.HelpBox("Docker Desktop is not running! Please start Docker Desktop first.", MessageType.Error);
        }

        GUILayout.Label("This will:");
        GUILayout.Label("-Build Unity Linux Server");
        GUILayout.Label("-Copy to Docker container");
        GUILayout.Label("-Start server on port 7777 (UDP/TCP)");
        GUILayout.Space(10);

        GUI.enabled = dockerRunning;
        if (GUILayout.Button("(RE)-Build & Deploy Linux Server", GUILayout.Height(30))) {
            BuildAndDeployServer();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        if (GUILayout.Button("Check Docker Status", GUILayout.Height(20))) {
            CheckDockerStatus();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Stop Docker Container", GUILayout.Height(25))) {
            StopDockerContainer();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Start Docker Container", GUILayout.Height(20))) {
            StartDockerContainer();
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Restart Docker Container", GUILayout.Height(20))) {
            RestartDockerContainer();
        }
        
    }
    
    void BuildAndDeployServer()
    {
        EditorUtility.DisplayProgressBar("Docker Server Builder", "Building Linux Server...", 0.1f);
        
        // Build Linux server
        BuildLinuxServer();
        
        EditorUtility.DisplayProgressBar("Docker Server Builder", "Building Docker image...", 0.5f);
        
        // Build and run Docker container
        BuildDockerImage();
        
        EditorUtility.DisplayProgressBar("Docker Server Builder", "Starting container...", 0.8f);
        
        // Start container
        StartDockerContainer();
        
        EditorUtility.ClearProgressBar();
        
        EditorUtility.DisplayDialog("Success", 
            "Linux server built and deployed!\n\n" +
            $"Server running on {System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0]}:7777\n" +
            "Make sure to:\n" +
            "1. Set KCP Transport settings:\n" +
            "   - Port: 7777\n" +
            "   - NoDelay: true\n" +
            "   - Congestion Window: false\n" +
            "   - Timeout: 30000\n" +
            "2. Check both TCP and UDP ports are open\n" +
            "3. Try connecting with different addresses:\n" +
            "   - Your IP\n" +
            "   - localhost\n" +
            "   - 127.0.0.1", "OK");
    }
    
    void BuildLinuxServer()
    {
        string buildsRoot = Path.Combine(Application.dataPath, "..", "Builds");
        string buildPath = Path.Combine(buildsRoot, "LinuxServer");
        
        // Clear existing build
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, true);
        }
        Directory.CreateDirectory(buildPath);

        // Build options with improved settings
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetScenesFromBuildSettings(),
            locationPathName = Path.Combine(buildPath, "Server"),
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.CleanBuildCache | BuildOptions.StrictMode
        };

        UnityEditor.Build.Reporting.BuildReport report = null;
        try
        {
            SetGameManagerHeadlessStartMode(HeadlessStartOptions.AutoStartServer, setKcpPortIfPresent: true);
            report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
        finally
        {
            SetGameManagerHeadlessStartMode(HeadlessStartOptions.DoNothing, setKcpPortIfPresent: false);
        }
        
        if (report == null)
        {
            throw new System.Exception("Linux build did not produce a report. Check the console for details.");
        }
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log($"Linux server build completed successfully! Build written to {buildPlayerOptions.locationPathName}");
        }
        else if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            // Log detailed build errors
            foreach (var step in report.steps)
            {
                foreach (var message in step.messages)
                {
                    if (message.type == LogType.Error)
                    {
                        UnityEngine.Debug.LogError($"Build step {step.name} failed: {message.content}");
                    }
                }
            }
            throw new System.Exception($"Linux build failed! Check the console for detailed error messages.");
        }
    }

    void AddServerAutoStartToBootstrap()
    {
        SetGameManagerHeadlessStartMode(HeadlessStartOptions.AutoStartServer, setKcpPortIfPresent: true);
    }

    void SetGameManagerHeadlessStartMode(HeadlessStartOptions mode, bool setKcpPortIfPresent)
    {
        string bootstrapPath = "Assets/Level/Scenes/Bootstrap.unity";
        
        if (!File.Exists(bootstrapPath))
        {
            UnityEngine.Debug.LogWarning("Bootstrap scene not found, skipping headlessStartMode setup");
            return;
        }

        UnityEngine.Debug.Log($"Setting GameManager headlessStartMode to {mode}...");
        
        // Open the Bootstrap scene
        var originalScene = EditorSceneManager.GetActiveScene();
        var bootstrapScene = EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single);
        
        try
        {
            // Find GameManager in the scene
            var gameManagers = FindObjectsByType<GameManager>(FindObjectsSortMode.None);
            if (gameManagers.Length == 0)
            {
                UnityEngine.Debug.LogWarning("No GameManager found in Bootstrap scene");
                return;
            }

            var gameManager = gameManagers[0];
            
            // Set headless start mode
            gameManager.headlessStartMode = mode;

            // Optionally ensure KCP port is correct when enabling server auto start
            if (setKcpPortIfPresent)
            {
                var kcpTransport = gameManager.transport as kcp2k.KcpTransport;
                if (kcpTransport != null)
                {
                    kcpTransport.Port = 7777;
                    UnityEngine.Debug.Log("DockerServerBuilder: Set KCP transport port to 7777");
                }
            }
            
            UnityEngine.Debug.Log($"DockerServerBuilder: Set GameManager headlessStartMode to {gameManager.headlessStartMode}");

            // Save the scene
            EditorSceneManager.SaveScene(bootstrapScene);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to set headlessStartMode in Bootstrap scene: {e.Message}");
        }
        finally
        {
            // Restore original scene if it was different
            if (originalScene.IsValid() && originalScene != bootstrapScene)
            {
                EditorSceneManager.OpenScene(originalScene.path, OpenSceneMode.Single);
            }
        }
    }

    
    bool IsDockerRunning()
    {
        try
        {
            RunCommand("docker", "version", true);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    void CheckDockerStatus()
    {
        try
        {
            RunCommand("docker", "version");
            EditorUtility.DisplayDialog("Docker Status", "Docker is running successfully!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Docker Status", 
                "Docker is not running or not installed!\n\n" +
                "Please:\n" +
                "1. Install Docker Desktop\n" +
                "2. Start Docker Desktop\n" +
                "3. Wait for it to fully start\n\n" +
                "Error: " + e.Message, "OK");
        }
    }
    
    string[] GetScenesFromBuildSettings()
    {
        // For Mirror networking, we need multiple scenes
        List<string> scenesToInclude = new List<string>();
        
        // Essential scenes for Mirror networking
        string[] requiredScenes = {
            "Assets/Level/Scenes/Bootstrap.unity",  // Main networking scene
            "Assets/Level/Scenes/Board.unity",      // Game scene (if exists)
            "Assets/Level/Scenes/Lobby.unity",      // Lobby scene (if exists)
            "Assets/Level/Scenes/PlayMenu.unity",   // Menu scene that clients connect through
            "Assets/Level/Scenes/MainMenu.unity",    // Settings scene (if exists)
            "Assets/Level/Scenes/MinigameOne.unity"    // Settings scene (if exists)
        };
        
        foreach (string scenePath in requiredScenes)
        {
            if (File.Exists(scenePath))
            {
                scenesToInclude.Add(scenePath);
                UnityEngine.Debug.Log($"Including scene in server build: {scenePath}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Scene not found, skipping: {scenePath}");
            }
        }
        
        if (scenesToInclude.Count == 0)
        {
            UnityEngine.Debug.LogError("No valid scenes found for server build!");
            throw new System.Exception("No valid scenes found");
        }
        
        UnityEngine.Debug.Log($"Server build will include {scenesToInclude.Count} scenes");
        return scenesToInclude.ToArray();
    }
    
    void BuildDockerImage()
    {
        try 
        {
            UnityEngine.Debug.Log("Starting Docker build process...");
            RunCommand("docker-compose", "build --no-cache");
            UnityEngine.Debug.Log("Docker build completed successfully");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Docker build failed: {e.Message}");
            throw;
        }
    }
    
    void StartDockerContainer()
    {
        // Use docker-compose to start the container in detached mode
        RunCommand("docker-compose", "up -d");
    }
    
    void StopDockerContainer()
    {
        RunCommand("docker-compose", "down");
        EditorUtility.DisplayDialog("Stopped", "Docker containers stopped and removed.", "OK");
    }

    void RestartDockerContainer()
    {
        // Restart the container
        RunCommand("docker-compose", "restart");
        EditorUtility.DisplayDialog("Restarted", "Docker containers restarted successfully.", "OK");
    }

    
    string RunCommandWithOutput(string command, string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        using (Process process = Process.Start(startInfo))
        {
            bool finished = process.WaitForExit(15000); // 15 second timeout
            
            if (!finished)
            {
                process.Kill();
                throw new System.Exception($"Command timed out: {command} {arguments}");
            }
            
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            
            if (process.ExitCode != 0)
            {
                if (error.Contains("No such container") || error.Contains("is not running"))
                {
                    throw new System.Exception("Container is not running. Please start the server first.");
                }
                
                throw new System.Exception($"Command failed: {command} {arguments}\nError: {error}");
            }
            
            return output;
        }
    }
    
    void RunCommand(string command, string arguments, bool ignoreErrors = false)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(Application.dataPath, "..")
        };
        
        using (Process process = Process.Start(startInfo))
        {
            // Set timeout for Docker commands
            bool finished = process.WaitForExit(300000); // 5 minute timeout
            
            if (!finished)
            {
                process.Kill();
                throw new System.Exception($"Command timed out: {command} {arguments}");
            }
            
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            
            if (process.ExitCode != 0 && !ignoreErrors)
            {
                // Special handling for Docker connection errors
                if (error.Contains("dockerDesktopLinuxEngine") || error.Contains("cannot connect") || error.Contains("pipe"))
                {
                    throw new System.Exception("Docker Desktop is not running! Please start Docker Desktop and try again.");
                }
                
                throw new System.Exception($"Command failed: {command} {arguments}\nError: {error}");
            }
            
            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log($"Command output: {output}");
            }
        }
    }
}