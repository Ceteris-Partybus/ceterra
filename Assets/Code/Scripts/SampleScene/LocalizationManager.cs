using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizationManager : MonoBehaviour {

    void Start() {
        this.InvokeRepeating(nameof(SwitchLanguage), 0, 5);
    }

    private void SwitchLanguage() {
        var currentLocale = LocalizationSettings.SelectedLocale;
        int currentIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(currentLocale);
        int nextIndex = (currentIndex + 1) % LocalizationSettings.AvailableLocales.Locales.Count;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[nextIndex];
        Debug.Log($"Switched to {LocalizationSettings.SelectedLocale.LocaleName}");
    }
}
