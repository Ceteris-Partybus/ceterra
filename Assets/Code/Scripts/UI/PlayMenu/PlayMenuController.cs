using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Mirror;
using UnityEngine.Networking;

public class PlayMenuController : MonoBehaviour {
    private Button backButton;
    private Button joinLobbyButton;

    [SerializeField]
    private UIDocument uIDocument;

    private const string MAIN_MENU_SCENE = "MainMenu";

    private TextField ipAddressField;
    private TextField portField;
    private TextField lobbyCodeField;
    private Button joinByCodeButton;
    
    private const string API_URL = "http://localhost:3000/api";

    private void OnEnable() {
        var root = uIDocument.rootVisualElement;
        InitializeUIElements(root);
    }

    private void InitializeUIElements(VisualElement root) {
        backButton = root.Q<Button>("BackButton");
        backButton.clicked += () => LoadScene(MAIN_MENU_SCENE);

        joinLobbyButton = root.Q<Button>("JoinLobbyButton");
        joinLobbyButton.clicked += OnJoinLobbyClicked;

        ipAddressField = root.Q<TextField>("IPAddressField");
        portField = root.Q<TextField>("PortField");

        lobbyCodeField = root.Q<TextField>("LobbyCodeField");
        joinByCodeButton = root.Q<Button>("JoinByCodeButton");
        joinByCodeButton.clicked += OnJoinByCodeClicked;
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

    private void OnJoinByCodeClicked() {
        string code = lobbyCodeField.value;
        if (string.IsNullOrEmpty(code)) {
            Debug.LogError("Lobby Code is empty.");
            return;
        }

        StartCoroutine(ValidateAndJoinLobby(code));
    }

    private IEnumerator ValidateAndJoinLobby(string code) {
        Debug.Log($"Validating invite code: {code}");
        string url = $"{API_URL}/lobby/{code}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                string errorMsg = $"Failed to validate invite code: {request.error}";
                
                if (request.responseCode == 404) {
                    errorMsg = "Invalid or expired invite code. Please check the code and try again.";
                } else if (request.responseCode == 0) {
                    errorMsg = "Cannot connect to lobby API. Make sure the server is running.";
                }
                
                Debug.LogError(errorMsg);
                // TODO: Show error message in UI
                yield break;
            }

            // Parse JSON response
            string json = request.downloadHandler.text;
            Debug.Log($"API Response: {json}");
            
            LobbyInfo lobbyInfo = JsonUtility.FromJson<LobbyInfo>(json);

            if (lobbyInfo == null || string.IsNullOrEmpty(lobbyInfo.ip)) {
                Debug.LogError("Invalid response from server");
                yield break;
            }

            Debug.Log($"Connecting via code '{code}' to {lobbyInfo.ip}:{lobbyInfo.port}");

            // Connect to the server
            NetworkManager.singleton.networkAddress = lobbyInfo.ip;
            if (Transport.active is PortTransport portTransport) {
                if (ushort.TryParse(lobbyInfo.port, out ushort parsedPort)) {
                    portTransport.Port = parsedPort;
                    Debug.Log($"Set transport port to {parsedPort}");
                } else {
                    Debug.LogError("Invalid port from API");
                    yield break;
                }
            }
            
            Debug.Log("Starting Mirror client...");
            NetworkManager.singleton.StartClient();
        }
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

[System.Serializable]
public class LobbyInfo {
    public string ip;
    public string port;
}