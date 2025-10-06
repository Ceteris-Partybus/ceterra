using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using Mirror;
using UnityEditor;

public class PlayMenuController : MonoBehaviour {
    private Button backButton;
    private Button joinLobbyButton;

    [SerializeField]
    private UIDocument uIDocument;

    private const string MAIN_MENU_SCENE = "MainMenu";

    private TextField ipAddressField;
    private TextField portField;

    private void OnEnable() {
        var root = uIDocument.rootVisualElement;
        InitializeUIElements(root);
    }

    private void InitializeUIElements(VisualElement root) {
        backButton = root.Q<Button>("BackButton");
        backButton.clicked += () => {
            LoadScene(MAIN_MENU_SCENE);
            Audiomanager.Instance?.PlayClickSound();
        };

        joinLobbyButton = root.Q<Button>("JoinLobbyButton");
        joinLobbyButton.clicked += () =>
        {
            Audiomanager.Instance?.PlayClickSound();
            OnJoinLobbyClicked();                   
        };

        ipAddressField = root.Q<TextField>("IPAddressField");
        portField = root.Q<TextField>("PortField");
    }

    private void OnJoinLobbyClicked() {
        string ipAddress = ipAddressField.value;
        string port = portField.value;

        if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(port)) {
            Debug.LogError("IP Address or Port is empty.");
            return;
        }

        Debug.Log($"Connecting to server at {ipAddress}:{port}");

        NetworkManager.singleton.networkAddress = ipAddress;

        if (Transport.active is PortTransport portTransport) {
            if (ushort.TryParse(port, out ushort parsedPort)) {
                portTransport.Port = parsedPort;
            }
            else {
                Debug.LogError("Invalid port number");
                return;
            }
        }

        // Scene change is done indirectly by the NetworkManager when a client connects
        // Scene will change to the Scene defined in NetworkManager's "Online Scene"
        NetworkManager.singleton.StartClient();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            LoadScene(MAIN_MENU_SCENE);
        }
    }

    private void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}