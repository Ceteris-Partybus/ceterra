using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkRoomPlayer
{
    [Header("Player Appearance")]
    [SyncVar(hook = nameof(OnSelectedAppearanceChanged))]
    [SerializeField] private int selectedAppearanceIndex = 0;

    [SerializeField] private PlayerAppearanceLinker appearanceLinker;

    public int SelectedAppearanceIndex => selectedAppearanceIndex;
    public string PlayerName { get; set; } = "";
    public int Id => index;

    private void Awake()
    {
        if (appearanceLinker == null)
            appearanceLinker = GetComponent<PlayerAppearanceLinker>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Apply initial appearance after a short delay to ensure components are ready
        StartCoroutine(ApplyInitialAppearance());
    }

    private System.Collections.IEnumerator ApplyInitialAppearance()
    {
        yield return new WaitForEndOfFrame();
        if (appearanceLinker != null)
        {
            appearanceLinker.SetAppearance(selectedAppearanceIndex);
        }
    }

    [Command]
    public void CmdChangeAppearance(int newAppearanceIndex)
    {
        if (PlayerAppearanceController.Instance == null) return;

        var appearances = PlayerAppearanceController.Instance.AvailableAppearances;
        if (newAppearanceIndex >= 0 && newAppearanceIndex < appearances.Count)
        {
            selectedAppearanceIndex = newAppearanceIndex;
        }
    }

    private void OnSelectedAppearanceChanged(int oldIndex, int newIndex)
    {
        // Update appearance when synced from server
        if (appearanceLinker != null)
        {
            appearanceLinker.SetAppearance(newIndex);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public override void OnGUI()
    {
        if (!showRoomGUI) return;

        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room && room.showRoomGUI && Utils.IsSceneActive(room.RoomScene))
        {
            DrawPlayerInfo();
            DrawAppearanceSelection();
            DrawReadyButton();
        }
    }

    private void DrawPlayerInfo()
    {
        GUILayout.BeginArea(new Rect(20f + (index * 200), 200f, 190f, 200f));

        GUILayout.Label($"Player [{index + 1}]");

        if (readyToBegin)
            GUILayout.Label("Ready");
        else
            GUILayout.Label("Not Ready");

        GUILayout.EndArea();
    }

    private void DrawAppearanceSelection()
    {
        if (!isLocalPlayer) return;

        if (PlayerAppearanceController.Instance == null) return;

        var appearances = PlayerAppearanceController.Instance.AvailableAppearances;
        if (appearances.Count == 0) return;

        GUILayout.BeginArea(new Rect(20f + (index * 200), 250f, 190f, 120f));

        GUILayout.Label("Character:");

        // Simple appearance selection buttons
        for (int i = 0; i < appearances.Count; i++)
        {
            var appearance = appearances[i];
            bool isSelected = i == selectedAppearanceIndex;

            string buttonText = isSelected ? $"► {appearance.name}" : appearance.name;

            if (GUILayout.Button(buttonText))
            {
                CmdChangeAppearance(i);
            }
        }

        GUILayout.EndArea();
    }

    private void DrawReadyButton()
    {
        if (!NetworkClient.active || !isLocalPlayer) return;

        GUILayout.BeginArea(new Rect(20f + (index * 200), 380f, 190f, 30f));

        if (readyToBegin)
        {
            if (GUILayout.Button("Cancel"))
                CmdChangeReadyState(false);
        }
        else
        {
            if (GUILayout.Button("Ready"))
                CmdChangeReadyState(true);
        }

        GUILayout.EndArea();
    }
}
