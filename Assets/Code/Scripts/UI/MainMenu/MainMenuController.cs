using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class MainMenuController : MonoBehaviour {
    private VisualElement root;
    private Button playButton;
    private Button settingsButton;
    private Button exitButton;
    private TemplateContainer settingsContainer;

    [SerializeField]
    private UIDocument uIDocument;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource uiAudioSource;  
    [SerializeField] private AudioClip buttonClickSound;

    private void OnEnable() {
        root = uIDocument.rootVisualElement;

        InitializeUIElements();
        StartCoroutine(ApplyInitialAudioValuesNextFrame());
    }

    private void InitializeUIElements() {
        playButton = root.Q<Button>("PlayButton");
        playButton.clicked += () => 
        {
            Audiomanager.Instance?.PlayClickSound();
            SceneManager.LoadScene("PlayMenu");   
        };

        settingsButton = root.Q<Button>("SettingsButton");
        settingsButton.clicked += () => 
        {
            Audiomanager.Instance?.PlayClickSound();
            ShowSettings();
        };

        exitButton = root.Q<Button>("ExitButton");
        exitButton.clicked += () => 
        {
            Audiomanager.Instance?.PlayClickSound();
            Application.Quit();
        };

        settingsContainer = root.Q<TemplateContainer>("SettingsTemplateContainer");
    }

    private void ShowSettings() {
        settingsContainer.AddToClassList("visible");
    }
    private IEnumerator ApplyInitialAudioValuesNextFrame()
    {
        yield return null; 

        float master = PlayerPrefs.GetFloat("MasterVolume", 100f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 100f);

        SetMixerVolume(master, "SoundVol");
        SetMixerVolume(music, "MusicVol");
    }

    private void SetMixerVolume(float value, string param)
    {
        if (audioMixer == null) return;

        float normalized = Mathf.Clamp01(value / 100f);
        float dB = (normalized <= 0.0001f) ? -80f : Mathf.Log10(normalized) * 20f;
        audioMixer.SetFloat(param, dB);
    }
}