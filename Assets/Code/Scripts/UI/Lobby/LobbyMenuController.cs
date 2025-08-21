using UnityEngine;
using UnityEngine.UIElements;
using Mirror;


public class LobbyMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerMeshController player;

    private Button switchButton;
    private VisualElement displayCharacter;
    private Label characterTitle;
    private Toggle takeToggle;

    // Helper property for local player ownership check
    private bool IsLocalPlayerOwned => player != null && player.isOwned;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        switchButton     = root.Q<Button>("Switch_Character");
        displayCharacter = root.Q<VisualElement>("Display_Character");
        characterTitle   = root.Q<Label>("CharacterTitle");
        takeToggle       = root.Q<Toggle>("Take");

        if (switchButton != null)
            switchButton.clicked += OnSwitchCharacter;
        if (takeToggle != null)
            takeToggle.RegisterValueChangedCallback(OnTakeChanged);
    }

    void OnDisable()
    {
        if (switchButton != null)
            switchButton.clicked -= OnSwitchCharacter;
        if (takeToggle != null)
            takeToggle.UnregisterValueChangedCallback(OnTakeChanged);
    }

    private void OnSwitchCharacter()
    {
        if (!IsLocalPlayerOwned) return;

        int nextIndex = player.CurrentMeshIndex + 1;
        if (nextIndex >= player.MeshCount)
            nextIndex = 0;

        player.CmdChangeMesh(nextIndex);

        // Set mesh index on LobbyPlayer for transfer to game scene
        var lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
        if (lobbyPlayer != null)
        {
            lobbyPlayer.SetSelectedMeshIndex(nextIndex);
        }

    }

    private void OnTakeChanged(ChangeEvent<bool> evt)
    {
        if (!IsLocalPlayerOwned) return;

        int currentIndex = player.CurrentMeshIndex;
        uint playerNetId = player.netId;

        var lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
        if (lobbyPlayer != null)
        {
            lobbyPlayer.SetSelectedMeshIndex(currentIndex);
        }

        if (evt.newValue)
        {
            Debug.Log($"Player {playerNetId} locked in character #{currentIndex}");
            player.CmdLockCharacter(currentIndex);
        }
        else
        {
            Debug.Log($"Player {playerNetId} unlocked character choice");
            player.CmdUnlockCharacter();
        }
    }

    void Update()
    {
        // Bind to local player when it spawns
        if (player == null && NetworkClient.localPlayer != null)
        {
            player = NetworkClient.localPlayer.GetComponent<PlayerMeshController>();
        }

        // Keep character title in sync with authoritative value from PlayerMeshController
        if (player != null && characterTitle != null)
        {
            characterTitle.text = $"Character Preview #{player.CurrentMeshIndex + 1}";
        }
    }
}
