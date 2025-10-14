using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class LocalizationManager : NetworkedSingleton<LocalizationManager> {
    protected override bool ShouldPersistAcrossScenes => true;
    private const string LOCALIZATION_TABLE = "ceterra";

    // public static List<T> LoadLocalizedText(TextAsset jsonFile, string key) {
    //     var allText = JsonConvert.DeserializeObject<Dictionary<string, List<T>>>(jsonFile.text);
    //     if (allText.ContainsKey(key)) {
    //         return allText[key];
    //     }
    //     else if (allText.ContainsKey("de")) {
    //         Debug.Log($"Wähle Default \"de\", da Key {key} nicht gefunden wurde.");
    //         return allText["de"];
    //     }
    //     else {
    //         Debug.Log("Kein entsprechender Key gefunden");
    //         return null;
    //     }
    // }
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