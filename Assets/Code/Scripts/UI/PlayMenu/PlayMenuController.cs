using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayMenuController : MonoBehaviour {
    private Button backButton;
    private Button joinLobbyButton;
    private Button refreshButton;

    private TextField[] otpDigits;
    private DropdownField serverDropdown;

    [SerializeField]
    private UIDocument uIDocument;
    [SerializeField] private GameObject mainMenuObject;

    private const int CONNECTION_TIMEOUT = 5;

    private void Awake() {
        if (InviteCodeValidator.Instance == null) {
            var go = new GameObject("InviteCodeValidator");
            go.AddComponent<InviteCodeValidator>();
        }
    }

    private void OnEnable() {
        if (mainMenuObject.activeSelf) {
            gameObject.SetActive(false);
            return;
        }

        var root = uIDocument.rootVisualElement;
        InitializeUIElements(root);

        if (!SceneManager.GetSceneByName("BackgroundView").isLoaded) {
            StartCoroutine(LoadBackgroundSmoothly(root));
        }
    }

    private IEnumerator LoadBackgroundSmoothly(VisualElement root) {
        var fader = new VisualElement();
        fader.style.backgroundColor = Color.black;
        fader.style.position = Position.Absolute;
        fader.style.top = 0;
        fader.style.bottom = 0;
        fader.style.left = 0;
        fader.style.right = 0;
        root.Insert(0, fader);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BackgroundView", LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f) {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone) {
            yield return null;
        }

        float duration = 1.0f;
        float timer = 0f;

        while (timer < duration) {
            timer += Time.deltaTime;
            fader.style.opacity = Mathf.Lerp(1, 0, timer / duration);
            yield return null;
        }

        fader.RemoveFromHierarchy();
    }

    private void OnDisable() {
        if (InviteCodeValidator.Instance != null) {
            InviteCodeValidator.Instance.OnServerListUpdated -= UpdateServerDropdown;
        }
    }

    private void InitializeUIElements(VisualElement root) {
        backButton = root.Q<Button>("BackButton");
        backButton.clicked += () => {
            ReturnToMainMenu();
            Audiomanager.Instance?.PlayClickSound();
        };

        joinLobbyButton = root.Q<Button>("JoinLobbyButton");
        joinLobbyButton.clicked += () => {
            Audiomanager.Instance?.PlayClickSound();
            OnJoinLobbyClicked();
        };
        joinLobbyButton.SetEnabled(false);

        otpDigits = new TextField[4];
        for (int i = 0; i < 4; i++) {
            otpDigits[i] = root.Q<TextField>($"Digit{i + 1}");
            int index = i;
            otpDigits[i].RegisterCallback<ChangeEvent<string>>(evt => OnDigitChanged(evt, index));
            otpDigits[i].RegisterCallback<KeyDownEvent>(evt => OnDigitKeyDown(evt, index));
        }

        serverDropdown = root.Q<DropdownField>("ServerDropdown");

        refreshButton = root.Q<Button>("RefreshButton");
        refreshButton.clicked += () => {
            Audiomanager.Instance?.PlayClickSound();
            InviteCodeValidator.Instance?.RefreshServers();
        };

        UpdateServerDropdown();
        if (InviteCodeValidator.Instance != null) {
            InviteCodeValidator.Instance.OnServerListUpdated += UpdateServerDropdown;
        }

        root.RegisterCallback<KeyDownEvent>(OnRootKeyDown);
    }

    private void UpdateServerDropdown() {
        if (InviteCodeValidator.Instance == null) {
            return;
        }

        var servers = InviteCodeValidator.Instance.AvailableServers;
        var choices = new List<string>();
        foreach (var server in servers) {
            string latencyText = server.Latency >= 0 ? $"{server.Latency}ms" : "N/A";
            choices.Add($"{server.Key} ({latencyText})");
        }
        serverDropdown.choices = choices;

        if (choices.Count > 0) {
            // Try to keep selected value if valid, else select first
            if (string.IsNullOrEmpty(serverDropdown.value) || !choices.Contains(serverDropdown.value)) {
                serverDropdown.index = 0;
            }
        }
    }

    private void OnDigitChanged(ChangeEvent<string> evt, int index) {
        string newValue = evt.newValue;

        // Enforce single digit
        if (newValue.Length > 1) {
            otpDigits[index].value = newValue.Substring(newValue.Length - 1);
            return; // Value change will trigger again
        }

        if (!string.IsNullOrEmpty(newValue)) {
            if (index < 3) {
                otpDigits[index + 1].Focus();
            }
            else {
                otpDigits[index].Blur();
            }
        }

        CheckJoinButtonState();
    }

    private void OnDigitKeyDown(KeyDownEvent evt, int index) {
        if (evt.keyCode == KeyCode.Backspace) {
            if (string.IsNullOrEmpty(otpDigits[index].value) && index > 0) {
                otpDigits[index - 1].Focus();
            }
        }
    }

    private void OnRootKeyDown(KeyDownEvent evt) {
        // Paste handling
        if (evt.ctrlKey && evt.keyCode == KeyCode.V) {
            string clipboard = GUIUtility.systemCopyBuffer;
            if (!string.IsNullOrEmpty(clipboard)) {
                string digits = new string(clipboard.Where(char.IsDigit).ToArray());
                if (digits.Length >= 4) {
                    for (int i = 0; i < 4; i++) {
                        otpDigits[i].value = digits[i].ToString();
                    }
                    otpDigits[3].Focus();
                    CheckJoinButtonState();
                }
            }
            return;
        }

        // Number entry when not focused on a text field
        if (!(evt.target is TextField)) {
            string charStr = evt.character.ToString();
            if (string.IsNullOrEmpty(charStr) || !char.IsDigit(evt.character)) {
                // Fallback to keycodes if character is not set (some Unity versions)
                if (evt.keyCode >= KeyCode.Alpha0 && evt.keyCode <= KeyCode.Alpha9) {
                    charStr = ((int)evt.keyCode - (int)KeyCode.Alpha0).ToString();
                }
                else if (evt.keyCode >= KeyCode.Keypad0 && evt.keyCode <= KeyCode.Keypad9) {
                    charStr = ((int)evt.keyCode - (int)KeyCode.Keypad0).ToString();
                }
                else {
                    return;
                }
            }

            // Find first empty slot
            for (int i = 0; i < 4; i++) {
                if (string.IsNullOrEmpty(otpDigits[i].value)) {
                    otpDigits[i].value = charStr;
                    otpDigits[i].Focus(); // Focus it so next type goes to next
                    // OnDigitChanged will handle focus next
                    break;
                }
            }
        }
    }

    private void CheckJoinButtonState() {
        bool allFilled = true;
        foreach (var field in otpDigits) {
            if (string.IsNullOrEmpty(field.value)) {
                allFilled = false;
                break;
            }
        }
        joinLobbyButton.SetEnabled(allFilled);
    }

    private void OnJoinLobbyClicked() {
        string code = "";
        foreach (var field in otpDigits) {
            code += field.value;
        }

        if (code.Length != 4) {
            return;
        }

        if (string.IsNullOrEmpty(serverDropdown.value)) {
            Debug.LogError("No server selected");
            return;
        }

        string selectedOption = serverDropdown.value;
        string serverKey = selectedOption.Split(' ')[0];

        joinLobbyButton.SetEnabled(false);

        InviteCodeValidator.Instance.ValidateCode(code, serverKey, OnValidateSuccess, OnValidateError);
    }

    private void OnValidateSuccess(InviteCodeValidator.CodeResponse response) {
        Debug.Log($"✓ Valid code! Connect to {response.domain}:{response.port}");
        JoinServer(response.domain, response.port);
    }

    private void OnValidateError(string error) {
        Debug.LogWarning($"✗ Validation failed: {error}");
        joinLobbyButton.SetEnabled(true);
        // Ideally show error in UI
    }

    private void JoinServer(string ipAddress, int port) {
        NetworkManager.singleton.networkAddress = ipAddress;

        if (Transport.active is PortTransport portTransport) {
            portTransport.Port = (ushort)port;
        }

        NetworkManager.singleton.StartClient();
        StartCoroutine(HandleConnectionTimeout());
    }

    private IEnumerator HandleConnectionTimeout() {
        float elapsedTime = 0f;

        while (elapsedTime < CONNECTION_TIMEOUT) {
            if (NetworkClient.isConnected) {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        NetworkManager.singleton.StopClient();
        joinLobbyButton.SetEnabled(true);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ReturnToMainMenu();
        }
    }

    private void ReturnToMainMenu() {
        mainMenuObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
