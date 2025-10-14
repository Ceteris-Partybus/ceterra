using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using UnityEngine.Audio;
using System.Collections;

public class SettingsController : NetworkedSingleton<SettingsController> {
    protected override bool ShouldPersistAcrossScenes => true;
    private VisualElement settingsTemplateContainer;
    private Button closeButton;
    private Slider soundSlider;
    private Label soundValue;
    private Slider musicSlider;
    private Label musicValue;

    private Toggle fullscreenToggle;
    private DropdownField resolutionDropdown;
    private DropdownField languageDropdown;
    private Resolution[] resolutions;
    private List<string> resolutionOptions;
    private readonly List<string> availableLanguages = new() { "English", "Deutsch" };
    private const string soundVolumeParam = "SoundVol";
    private const string musicVolumeParam = "MusicVol";

    [SerializeField]
    private UIDocument uIDocument;
    [SerializeField] private AudioMixer audioMixer;
    private bool isInputAllowed = true;

    private void OnEnable() {
        var root = uIDocument.rootVisualElement;

        InitializeUIElements(root);
        SetupSound();
        SetupMusic();
        SetupFullscreen();
        InitializeResolutionDropdown();
        InitializeLanguageDropdown();
    }

    private void InitializeUIElements(VisualElement root) {
        settingsTemplateContainer = root.Q<VisualElement>("Overlay");

        closeButton = settingsTemplateContainer.Q<Button>("SettingsCloseButton");
        closeButton.clicked += () => {
            Audiomanager.Instance?.PlayClickSound();
            settingsTemplateContainer.RemoveFromClassList("visible");
        };

        soundSlider = root.Q<Slider>("SoundSlider");
        soundValue = root.Q<Label>("SoundValue");

        musicSlider = root.Q<Slider>("MusicSlider");
        musicValue = root.Q<Label>("MusicValue");

        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");

        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        languageDropdown = root.Q<DropdownField>("LanguageDropdown");
    }

    private void SetupSound() {
        soundSlider.RegisterValueChangedCallback(evt => {
            UpdateAudioValue(evt.newValue, soundValue, soundVolumeParam);
            PlayerPrefs.SetFloat("SoundVolume", evt.newValue);
            PlayerPrefs.Save();
        });

        soundSlider.RegisterCallback<MouseUpEvent>(evt => Audiomanager.Instance?.PlayClickSound());
        soundSlider.RegisterCallback<ClickEvent>(evt => Audiomanager.Instance?.PlayClickSound());

        var savedVolume = PlayerPrefs.GetFloat("SoundVolume", 100f);
        soundSlider.value = savedVolume;
        UpdateAudioValue(savedVolume, soundValue, soundVolumeParam);
    }

    private void SetupMusic() {
        musicSlider.RegisterValueChangedCallback(evt => {
            UpdateAudioValue(evt.newValue, musicValue, musicVolumeParam);
            PlayerPrefs.SetFloat("MusicVolume", evt.newValue);
            PlayerPrefs.Save();
        });

        musicSlider.RegisterCallback<MouseUpEvent>(evt => Audiomanager.Instance?.PlayClickSound());
        musicSlider.RegisterCallback<ClickEvent>(evt => Audiomanager.Instance?.PlayClickSound());

        var savedMusic = PlayerPrefs.GetFloat("MusicVolume", 100f);
        musicSlider.value = savedMusic;
        UpdateAudioValue(savedMusic, musicValue, musicVolumeParam);
    }

    private void UpdateAudioValue(float value, Label label, string mixerParam) {
        if (audioMixer == null) { return; }

        var normalized = Mathf.Clamp01(value / 100f);
        var dB = (normalized <= .0001f) ? -80f : Mathf.Log10(normalized) * 20f;
        audioMixer.SetFloat(mixerParam, dB);

        if (label != null) {
            label.text = Mathf.RoundToInt(value) + "%";
        }
    }

    private void SetupFullscreen() {
        fullscreenToggle.RegisterValueChangedCallback(evt => {
            UpdateToggleGraphic(evt.newValue);
            SetFullscreen(evt.newValue);
            Audiomanager.Instance?.PlayClickSound();
        });

        UpdateToggleGraphic(fullscreenToggle.value);
    }

    private void UpdateToggleGraphic(bool isChecked) {
        var resourcePath = isChecked ? "UI/Other/Toggle_checked" : "UI/Other/Toggle";
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

        for (var i = 0; i < resolutions.Length; i++) {
            var res = resolutions[i];
            var option = $"{res.width} x {res.height}";
            resolutionOptions.Add(option);

            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height) {
                currentIndex = i;
            }
        }

        resolutionDropdown.choices = resolutionOptions;
        resolutionDropdown.index = currentIndex;
        resolutionDropdown.RegisterCallback<MouseDownEvent>(evt => Audiomanager.Instance?.PlayClickSound());

        resolutionDropdown.RegisterValueChangedCallback(_ => {
            Audiomanager.Instance?.PlayClickSound();
            SetResolution(resolutionDropdown.index);
        });
    }

    private void SetResolution(int index) {
        var res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    private void InitializeLanguageDropdown() {
        languageDropdown.choices = availableLanguages;

        var savedLanguage = PlayerPrefs.GetString("selectedLanguage", "English");
        var savedIndex = availableLanguages.IndexOf(savedLanguage);

        languageDropdown.index = savedIndex >= 0 ? savedIndex : 0;
        languageDropdown.RegisterCallback<MouseDownEvent>(evt => Audiomanager.Instance?.PlayClickSound());

        languageDropdown.RegisterValueChangedCallback(evt => {
            Audiomanager.Instance?.PlayClickSound();
            SetLanguage(evt.newValue);
        });
        SetLanguage(languageDropdown.value);
    }

    private async void SetLanguage(string language) {
        var localeCode = language switch {
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

    void Update() {
        if (ModalManager.Instance?.ModalStack.Count > 0) {
            return;
        }

        if (Input.GetKey(KeyCode.Escape)) {
            OpenSettingsPanel();
        }
    }

    public void OpenSettingsPanel() {
        if (isInputAllowed) {
            isInputAllowed = false;
            StartCoroutine(WaitForSettingsPanelToFinishTransition());

            IEnumerator WaitForSettingsPanelToFinishTransition() {
                settingsTemplateContainer.ToggleInClassList("visible");
                yield return new WaitForSeconds(.5f);
                isInputAllowed = true;
            }
        }
    }
}
