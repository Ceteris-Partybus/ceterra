using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using Mirror;

public class PlayMenuController : MonoBehaviour {
    private Button backButton;
    private Button joinLobbyButton;
    private TextField ipAddressField;
    private TextField portField;

    [SerializeField]
    private UIDocument uIDocument;

    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string EXAMPLE_PLAY_SCENE = "MirrorRoomOnlineLobby";

    private void OnEnable() {
        var root = uIDocument.rootVisualElement;
        InitializeUIElements(root);
    }

    private void InitializeUIElements(VisualElement root) {
        backButton = root.Q<Button>("BackButton");
        joinLobbyButton = root.Q<Button>("JoinLobbyButton");
        ipAddressField = root.Q<TextField>("IPAddressField");
        portField = root.Q<TextField>("PortField");

        backButton.clicked += () => LoadScene(MAIN_MENU_SCENE);

        joinLobbyButton.clicked += () => {
            string ip = ipAddressField.value.Trim();
            string portText = portField.value.Trim();

            if (ushort.TryParse(portText, out ushort port)) {
                StartClientConnection(ip, port);
            }
            else {
                Debug.LogError($"Invalid port: {portText}");
            }
        };
    }

    private void StartClientConnection(string ip, ushort port) {
        NetworkManager manager = NetworkManager.singleton;
        if (manager == null) {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        // Stop any existing client connection first
        if (NetworkClient.active) {
            manager.StopClient();
        }

        manager.networkAddress = ip;

        if (Transport.active is PortTransport portTransport) {
            portTransport.Port = port;
        }
        else {
            Debug.LogWarning("Transport does not support port override.");
        }

        Debug.Log($"Attempting to connect to {ip}:{port}");
        manager.StartClient();
    }

    private void Start() {
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            LoadScene(MAIN_MENU_SCENE);
        }

        // Only check connection state if we're not already transitioning
        if (!isTransitioning) {
            if (NetworkClient.isConnected && !hasClientConnected) {
                hasClientConnected = true;
                OnClientConnected();
            }

            if (!NetworkClient.isConnected && hasClientConnected) {
                hasClientConnected = false;
                OnClientDisconnected();
            }
        }
    }

    private bool hasClientConnected = false;
    private bool isTransitioning = false;

    private void OnClientConnected() {
        Debug.Log("Client connected to server! Waiting for ready state...");

        // Don't immediately load scene - wait for NetworkClient.Ready
        if (NetworkClient.ready) {
            LoadGameScene();
        }
        else {
            // Wait a frame for the connection to fully establish
            StartCoroutine(WaitForReadyAndLoadScene());
        }
    }

    private System.Collections.IEnumerator WaitForReadyAndLoadScene() {
        // Wait for the client to be ready
        float timeout = 5f;
        float elapsed = 0f;

        while (!NetworkClient.ready && elapsed < timeout) {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (NetworkClient.ready) {
            LoadGameScene();
        }
        else {
            Debug.LogError("Client failed to become ready within timeout period");
            NetworkManager.singleton.StopClient();
        }
    }

    private void LoadGameScene() {
        if (!isTransitioning && NetworkClient.isConnected) {
            isTransitioning = true;
            Debug.Log("Loading game scene...");
            LoadScene(EXAMPLE_PLAY_SCENE);
        }
    }

    private void OnClientDisconnected() {
        Debug.Log("Client disconnected from server.");
        isTransitioning = false;
    }

    private void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}