# Localization

## Lokalisierung im UI Builder

1. Öffne dein Visual Tree Asset im UI Builder.
2. Gehe zu `Attributes` im Inspektor und wähle das Attribut, das du lokalisiert haben möchtest.
3. Rechtsklicke in das Textfeld und wähle `Add Binding`.
4. Wähle als `Type` den `UnityEngine.Localization.Localized String`.

### Lokalisierung bereits vorhanden
5. Existiert die Übersetzung bereits, wähle sie über `Select Entry...` aus

### Neue Lokalisierung erstellen
5. Klicke auf `Add New Table Entry` und wähle die `Translations` Collection aus.

6. Gebe dem Eintrag als Namen die englische Übersetzung des Textes mit der zugehörigen Szene als Präfix, also z.B. `SampleScene.Hello`.
7. Trage alle Übersetzungen in die Felder ein und klicke auf `Add Binding`.

## Lokalisierung via Code setzen
```csharp
private void SetLocale(int localeIndex) {
  LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
}
```

Der Index der Sprache (`localeIndex`) ist entweder codeseitig herauszufinden oder kann der Reihenfolge der Sprachen in den `LocalizationSettings` (befindet sich im `Settings` Ordner) entnommen werden.

## Lokalisierung exportieren

Öffne die `Translations` Collection im Projekt Explorer, klicke auf `Open in Table Editor` und wähle `Export` -> `CSV` und überschriebe die `Translations.csv` im gleichen Ordner.