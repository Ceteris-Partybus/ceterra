using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

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
    private readonly List<string> availableLanguages = new() { "English", "Deutsch" };

    [SerializeField]
    private UIDocument uIDocument;

    private void OnEnable() {
        var root = uIDocument.rootVisualElement;

        InitializeUIElements(root);
        SetupVolume();
        SetupFullscreen();
        InitializeResolutionDropdown();
        InitializeLanguageDropdown();
    }

    private void InitializeUIElements(VisualElement root) {
        settingsTemplateContainer = root.Q<TemplateContainer>("SettingsTemplateContainer");

        closeButton = settingsTemplateContainer.Q<Button>("SettingsCloseButton");
        closeButton.clicked += () => settingsTemplateContainer.RemoveFromClassList("visible");

        volumeSlider = root.Q<Slider>("VolumeSlider");
        volumeValue = root.Q<Label>("VolumeValue");

        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");

        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        languageDropdown = root.Q<DropdownField>("LanguageDropdown");
    }

    private void SetupVolume() {
        volumeSlider.RegisterValueChangedCallback(evt => UpdateVolumeValue(evt.newValue));
        UpdateVolumeValue(volumeSlider.value);
    }

    private void UpdateVolumeValue(float value) {
        volumeValue.text = Mathf.RoundToInt(value).ToString();
    }

    private void SetupFullscreen() {
        fullscreenToggle.RegisterValueChangedCallback(evt => {
            UpdateToggleGraphic(evt.newValue);
            SetFullscreen(evt.newValue);
        });

        UpdateToggleGraphic(fullscreenToggle.value);
    }

    private void UpdateToggleGraphic(bool isChecked) {
        string resourcePath = isChecked ? "UI/Other/Toggle_checked" : "UI/Other/Toggle";
        var toggleTexture = Resources.Load<Sprite>(resourcePath);
        fullscreenToggle.style.backgroundImage = new StyleBackground(toggleTexture);
    }

    private void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }

    private void InitializeResolutionDropdown() {
        resolutions = Screen.resolutions;
        resolutionOptions = new List<string>();

        var currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++) {
            var res = resolutions[i];
            string option = $"{res.width} x {res.height}";
            resolutionOptions.Add(option);

            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height) {
                currentIndex = i;
            }
        }

        resolutionDropdown.choices = resolutionOptions;
        resolutionDropdown.index = currentIndex;

        resolutionDropdown.RegisterValueChangedCallback(_ => SetResolution(resolutionDropdown.index));
    }

    private void SetResolution(int index) {
        var res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    private void InitializeLanguageDropdown() {
        languageDropdown.choices = availableLanguages;

        string savedLanguage = PlayerPrefs.GetString("selectedLanguage", "English");
        int savedIndex = availableLanguages.IndexOf(savedLanguage);

        languageDropdown.index = savedIndex >= 0 ? savedIndex : 0;

        languageDropdown.RegisterValueChangedCallback(evt => SetLanguage(evt.newValue));
        SetLanguage(languageDropdown.value); // Ensure correct locale on start
    }

    private async void SetLanguage(string language) {
        string localeCode = language switch {
            "English" => "en",
            "Deutsch" => "de",
            _ => null
        };

        if (localeCode == null) {
            Debug.LogWarning($"Unknown Language: {language}");
            return;
        }

        await LocalizationSettings.InitializationOperation.Task;

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (locale != null) {
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString("selectedLanguage", language);
            PlayerPrefs.Save();
        }
        else {
            Debug.LogWarning($"Kein Locale gefunden für Sprache: {language}");
        }
    }
}
