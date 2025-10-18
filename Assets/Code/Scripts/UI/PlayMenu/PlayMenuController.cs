using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Mirror;
using System;
using System.Collections;
using System.Text;

public class PlayMenuController : MonoBehaviour {
    private Button backButton;
    private Button joinLobbyButton;
    private Button validateCodeButton;

    [SerializeField]
    private UIDocument uIDocument;

    private const string MAIN_MENU_SCENE = "MainMenu";
    
    // HTTP API endpoint for code validation
    private const string API_URL = "https://api.okolyt.com/check-code";

    private TextField ipAddressField;
    private TextField portField;
    private TextField digitCodeField;

    private string originalValidateButtonText;

    [Serializable]
    private class CodeRequest {
        public string code;
    }

    [Serializable]
    private class CodeResponse {
        public bool success;
        public string message;
        public string domain;
        public int port;
    }

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
        joinLobbyButton.clicked += () => {
            Audiomanager.Instance?.PlayClickSound();
            OnJoinLobbyClicked();
        };

        validateCodeButton = root.Q<Button>("ValidateCodeButton");
        validateCodeButton.AddToClassList("validate-code");
        originalValidateButtonText = validateCodeButton.text;
        validateCodeButton.clicked += () => {
            Audiomanager.Instance?.PlayClickSound();
            OnValidateCodeClicked();
        };

        ipAddressField = root.Q<TextField>("IPAddressField");
        portField = root.Q<TextField>("PortField");
        digitCodeField = root.Q<TextField>("DigitCodeField");

        if (digitCodeField == null) {
            Debug.LogError("DigitCodeField not found in UI!");
        } else {
            digitCodeField.focusable = true;
            Debug.Log("DigitCodeField initialized successfully");
        }
    }

    private void OnValidateCodeClicked() {
        string code = digitCodeField.value;

        if (string.IsNullOrEmpty(code)) {
            Debug.LogError("Digit code is empty.");
            return;
        }

        if (code.Length != 4 || !int.TryParse(code, out _)) {
            Debug.LogError("Code must be exactly 4 digits.");
            return;
        }

        SetValidateButtonLoading(true);
        StartCoroutine(CheckCodeCoroutine(code));
    }

    private IEnumerator CheckCodeCoroutine(string code) {
        Debug.Log($"Validating code: {code}");

        validateCodeButton.text = "VALIDATING...";

        // JSON payload
        CodeRequest request = new CodeRequest { code = code };
        string jsonPayload = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        // POST request
        using (UnityWebRequest webRequest = UnityWebRequest.Post(API_URL, jsonPayload, "application/json")) {
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success) {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"Response: {responseText}");

                try {
                    // example response: {"success":true,"message":"Code is valid","domain":"example.de","port":7777}
                    CodeResponse response = JsonUtility.FromJson<CodeResponse>(responseText);

                    if (response != null && response.success) {
                        Debug.Log($"Valid code! Connect to {response.domain}:{response.port}");
                        
                        validateCodeButton.text = "CONNECTING...";
                        
                        ipAddressField.value = response.domain;
                        portField.value = response.port.ToString();
                        
                        JoinServer(response.domain, response.port);
                    } else {
                        Debug.LogWarning($"✗ {response?.message ?? "Code is not valid"}");
                        SetValidateButtonLoading(false);
                    }
                } catch (Exception e) {
                    Debug.LogError($"Failed to parse response: {e.Message}. Response was: {responseText}");
                    SetValidateButtonLoading(false);
                }
            } else {
                Debug.LogError($"Failed to validate code: {webRequest.error} (Status: {webRequest.responseCode})");
                SetValidateButtonLoading(false);
            }
        }
    }

    private void SetValidateButtonLoading(bool isLoading) {
        if (isLoading) {
            validateCodeButton.AddToClassList("loading");
            validateCodeButton.SetEnabled(false);
        } else {
            validateCodeButton.RemoveFromClassList("loading");
            validateCodeButton.SetEnabled(true);
            validateCodeButton.text = originalValidateButtonText;
        }
    }

    private void OnJoinLobbyClicked() {
        string ipAddress = ipAddressField.value;
        string port = portField.value;

        if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(port)) {
            Debug.LogError("IP Address or Port is empty.");
            return;
        }

        if (!ushort.TryParse(port, out ushort parsedPort)) {
            Debug.LogError("Invalid port number");
            return;
        }

        JoinServer(ipAddress, parsedPort);
    }

    private void JoinServer(string ipAddress, int port) {
        Debug.Log($"Connecting to server at {ipAddress}:{port}");

        NetworkManager.singleton.networkAddress = ipAddress;

        if (Transport.active is PortTransport portTransport) {
            portTransport.Port = (ushort)port;
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