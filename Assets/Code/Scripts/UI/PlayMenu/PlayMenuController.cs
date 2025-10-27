using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.Localization;

public class PlayMenuController : MonoBehaviour {
    private Button backButton;
    private Button joinLobbyButton;
    private Button validateCodeButton;

    [SerializeField]
    private UIDocument uIDocument;

    private const string MAIN_MENU_SCENE = "MainMenu";
    private const long ORIGINAL_BUTTON_TEXT_ID = 60011435313287168;
    private const long CONNECTION_TEXT_ID = 60011225140908032;
    private const long VALIDATION_TEXT_ID= 60010223864074240;

    private TextField ipAddressField;
    private TextField portField;
    private TextField digitCodeField;

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

        SetValidateButtonLoading(true, VALIDATION_TEXT_ID); 
        StartCoroutine(InviteCodeValidator.ValidateCode(code, OnValidateSuccess, OnValidateError));
    }

    private void OnValidateSuccess(InviteCodeValidator.CodeResponse response) {
        Debug.Log($"✓ Valid code! Connect to {response.domain}:{response.port}");
        
        SetValidateButtonLoading(true, CONNECTION_TEXT_ID);
        
        ipAddressField.value = response.domain;
        portField.value = response.port.ToString();
        
        JoinServer(response.domain, response.port);
    }

    private void OnValidateError(string error) {
        Debug.LogWarning($"✗ Validation failed: {error}");
        SetValidateButtonLoading(false);
    }

    private void SetValidateButtonLoading(bool isLoading, long customTextId = ORIGINAL_BUTTON_TEXT_ID) {
        validateCodeButton.EnableInClassList("loading", isLoading);
        validateCodeButton.SetEnabled(!isLoading);
        var localizedString = validateCodeButton.GetBinding("text") as LocalizedString;
        localizedString.SetReference("ceterra", customTextId);
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