using UnityEngine.Localization.Settings;

public class LocalizationManager : Singleton<LocalizationManager> {
    private const string LOCALIZATION_TABLE = "ceterra";
    protected override bool ShouldPersistAcrossScenes => true;

    public string GetLocalizedText(long id) {
        return LocalizationSettings.StringDatabase
        .GetLocalizedStringAsync(LOCALIZATION_TABLE, id)
        .Result;
    }

    public string GetLocalizedText(string key) {
        return LocalizationSettings.StringDatabase
        .GetLocalizedStringAsync(LOCALIZATION_TABLE, key)
        .Result;
    }

    public string GetLocalizedParameterText(long id, int param) {
        return LocalizationSettings.StringDatabase
        .GetLocalizedStringAsync(LOCALIZATION_TABLE, id, new object[] { param })
        .Result;
    }
}