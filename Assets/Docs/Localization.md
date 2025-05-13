# Localization

## Lokalisierung im UI Builder

1. Öffne dein Visual Tree Asset im UI Builder.
2. Gehe zu `Attributes` im Inspektor und wähle das Attribut, das du lokalisiert haben möchtest.
3. Rechtsklicke in das Textfeld und wähle `Add Binding`.
4. Wähle als `Type` den `UnityEngine.Localization.Localized String`.

### Lokalisierung bereits vorhanden
5. Existiert die Übersetzung bereits, wähle sie über `Select Entry...` aus
6. 

### Neue Lokalisierung erstellen
5. Klicke auf `Add New Table Entry` und wähle die zu deiner Szene passende Collection.

#### Neue Collection erstellen
5.1 Klicke auf `Open Table Editor` und klickt auf `New Table Collection`. \
5.2 Wähle die zu unterstützenden Sprachen aus. Benötigst du noch weitere Sprachen, als oben dargestellt, füge diese über den `Locale Generator` hinzu. \
5.3 Gebe der Collection einen passenden Namen wie `<SzenenName>StringCollection`.

6. Gebe dem Eintrag als Namen die englische Übersetzung des Textes.
7. Trage alle Übersetzungen in die Felder ein und klicke auf `Add Binding`.

## Lokalisierung via Code setzen
```csharp
private void SetLocale(int localeIndex) {
  LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
}
```

Der Index der Sprache (`localeIndex`) ist entweder codeseitig herauszufinden oder kann der Reihenfolge der Sprachen in den `LocalizationSettings` (befindet sich im `Settings` Ordner) entnommen werden.