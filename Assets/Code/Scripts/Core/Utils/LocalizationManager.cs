using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class LocalizationManager : Singleton<LocalizationManager> {
    private const string LOCALIZATION_TABLE = "ceterra";
    protected override bool ShouldPersistAcrossScenes => true;

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