using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Globalization;
using System;

public class SettingsController : MonoBehaviour {

    private TemplateContainer settingsTemplateContainer;
    private Button closeButton;
    private Slider volumeSlider;
    private Label volumeValue;
    private Toggle fullscreenToggle;
    private DropdownField resolutionDropdown;
    private DropdownField languageDropdown;
    private Resolution[] resolutions;
    private List<string> resolutionOptions;
    private List<string> availableLanguages = new List<string> {
        "English",
        "Deutsch",
    };

    void OnEnable() {
        var root = GetComponent<UIDocument>().rootVisualElement;

        settingsTemplateContainer = root.Q<TemplateContainer>("SettingsTemplateContainer");
        closeButton = settingsTemplateContainer.Q<Button>("SettingsCloseButton");
        closeButton.clicked += OnCloseButtonClicked;

        volumeSlider = root.Q<Slider>("VolumeSlider");
        volumeValue = root.Q<Label>("VolumeValue");
        volumeSlider.RegisterValueChangedCallback(evt => {
            UpdateVolumeValue(evt.newValue);
        });
        UpdateVolumeValue(volumeSlider.value);

        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        fullscreenToggle.RegisterValueChangedCallback(evt => {
            UpdateRadioButtonStyle(evt.newValue);
            SetFullscreen(evt.newValue);
        });
        UpdateRadioButtonStyle(fullscreenToggle.value);

        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        InitializeResolutionDropdown();

        languageDropdown = root.Q<DropdownField>("LanguageDropdown");
        InitializeLanguageDropdown();
    }

    private void OnCloseButtonClicked() {
        settingsTemplateContainer.RemoveFromClassList("visible");
    }

    private void InitializeLanguageDropdown() {
        languageDropdown.choices = availableLanguages;
        int currentLanguageIndex = 0;

        languageDropdown.index = currentLanguageIndex;

        languageDropdown.RegisterValueChangedCallback(evt => {
            SetLanguage(evt.newValue);
        });
    }

    private void SetLanguage(string language) {
        Debug.Log($"Changing language to: {language} and it need to be implemented");
    }

    private void UpdateVolumeValue(float value) {
        volumeValue.text = Mathf.RoundToInt(value).ToString();
    }

    private void UpdateRadioButtonStyle(bool isChecked) {
        if (isChecked) {
            var checkedToggleTexture = Resources.Load<Sprite>("UI/Other/Toggle_checked");
            fullscreenToggle.style.backgroundImage = new StyleBackground(checkedToggleTexture);
        }
        else {
            var uncheckedToggleTexture = Resources.Load<Sprite>("UI/Other/Toggle");
            fullscreenToggle.style.backgroundImage = new StyleBackground(uncheckedToggleTexture);
        }
    }

    private void InitializeResolutionDropdown() {
        resolutions = Screen.resolutions;
        resolutionOptions = new List<string>();

        var currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++) {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            resolutionOptions.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height) {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.choices = resolutionOptions;

        resolutionDropdown.index = currentResolutionIndex;

        resolutionDropdown.RegisterValueChangedCallback(evt => {
            SetResolution(resolutionDropdown.index);
        });
    }

    private void SetResolution(int resolutionIndex) {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    private void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }
}