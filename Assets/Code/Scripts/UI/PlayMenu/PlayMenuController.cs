using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;

public class PlayMenuController : MonoBehaviour {
    private Button backButton;
    private Button joinLobbyButton;

    [SerializeField]
    private UIDocument uIDocument;

    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string EXAMPLE_PLAY_SCENE = "ExamplePlayScene";

    private void OnEnable() {
        var root = uIDocument.rootVisualElement;
        InitializeUIElements(root);
    }

    private void InitializeUIElements(VisualElement root) {
        backButton = root.Q<Button>("BackButton");
        backButton.clicked += () => LoadScene(MAIN_MENU_SCENE);

        joinLobbyButton = root.Q<Button>("JoinLobbyButton");
        joinLobbyButton.clicked += () => LoadScene(EXAMPLE_PLAY_SCENE);
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