using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour {
    private UIDocument document;
    private VisualElement root;
    private Button playButton;
    private Button settingsButton;
    private Button exitButton;
    private TemplateContainer settingsTemplateContainer;

    private void OnEnable() {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        playButton = root.Q<Button>("PlayButton");
        settingsButton = root.Q<Button>("SettingsButton");
        exitButton = root.Q<Button>("ExitButton");
        settingsTemplateContainer = root.Q<TemplateContainer>("SettingsTemplateContainer");

        playButton.clicked += OnPlayButtonClicked;
        settingsButton.clicked += OnSettingsButtonClicked;
        exitButton.clicked += OnExitButtonClicked;
    }

    private void OnSettingsButtonClicked() {
        settingsTemplateContainer.style.display = DisplayStyle.Flex;
        settingsTemplateContainer.AddToClassList("visible");
    }

    private void OnPlayButtonClicked() {
        SceneManager.LoadScene("PlayMenu");
    }

    private void OnExitButtonClicked() {
        Application.Quit();
    }
}