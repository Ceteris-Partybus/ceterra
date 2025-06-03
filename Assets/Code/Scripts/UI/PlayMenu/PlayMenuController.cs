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
    private const string EXAMPLE_PLAY_SCENE = "InGameTestScene";

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

        if (NetworkClient.isConnected && !hasClientConnected) {
            hasClientConnected = true;
            OnClientConnected();
        }

        if (!NetworkClient.isConnected && hasClientConnected) {
            hasClientConnected = false;
            OnClientDisconnected();
        }
    }

    private bool hasClientConnected = false;

    private void OnClientConnected() {
        Debug.Log("Client connected to server! Loading game scene...");
        LoadScene(EXAMPLE_PLAY_SCENE);
    }

    private void OnClientDisconnected() {
        Debug.Log("Client disconnected from server.");
    }

    private void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}