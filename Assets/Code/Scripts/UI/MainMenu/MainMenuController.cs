using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour {
    private VisualElement root;
    private Button playButton;
    private Button settingsButton;
    private Button exitButton;
    private TemplateContainer settingsContainer;

    private void OnEnable() {
        root = GetComponent<UIDocument>().rootVisualElement;

        InitializeUIElements();
    }

    private void InitializeUIElements() {
        playButton = root.Q<Button>("PlayButton");
        playButton.clicked += () => SceneManager.LoadScene("PlayMenu");

        settingsButton = root.Q<Button>("SettingsButton");
        settingsButton.clicked += ShowSettings;

        exitButton = root.Q<Button>("ExitButton");
        exitButton.clicked += Application.Quit;

        settingsContainer = root.Q<TemplateContainer>("SettingsTemplateContainer");
    }

    private void ShowSettings() {
        settingsContainer.AddToClassList("visible");
    }
}