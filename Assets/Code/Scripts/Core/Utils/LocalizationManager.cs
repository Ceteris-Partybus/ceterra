using UnityEngine.Localization.Settings;

public class LocalizationManager : Singleton<LocalizationManager> {
    private const string LOCALIZATION_TABLE = "ceterra";
    protected override bool ShouldPersistAcrossScenes => true;

    public string GetCurrentLocaleCode() {
        return LocalizationSettings.SelectedLocale.Identifier.Code;
    }

    public string GetLocalizedText(long id) {
        return LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE, id);
    }

    public string GetLocalizedText(long id, params object[] parameters) {
        return LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE, id, parameters);
    }

    public string GetLocalizedText(string key) {
        return LocalizationSettings.StringDatabase.GetLocalizedString(LOCALIZATION_TABLE, key);
    }
}