using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class LocalizationManager : MonoBehaviour {
    private const string LOCALIZATION_TABLE = "ceterra";
    public static LocalizationManager Instance;

    private void Awake() {

        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string GetLocalizedText(long id) {
        return LocalizationSettings.StringDatabase
        .GetLocalizedStringAsync(LOCALIZATION_TABLE, id)
        .Result;
    }

    public string GetLocalizedParameterText(long id, int param) {
        return LocalizationSettings.StringDatabase
        .GetLocalizedStringAsync(LOCALIZATION_TABLE, id, new object[] { param })
        .Result;
    }

    public string GetLocalizedObjectsByKey(string key) {
        var localizedString = new LocalizedString(LOCALIZATION_TABLE, key);
        return localizedString.GetLocalizedString();
    }
}