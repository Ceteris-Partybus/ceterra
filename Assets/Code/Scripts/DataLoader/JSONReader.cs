using UnityEngine;
using System.Collections.Generic;
using System;

public static class JsonReader {
    [Serializable]
    private class Wrapper<T> {
        public T[] items;
    }

    public static List<T> LoadJsonArrayFromResources<T>(string filePathInResources) {
        var jsonFile = Resources.Load<TextAsset>(filePathInResources);

        if (jsonFile == null) {
            Debug.LogError($"[JsonReader] Datei nicht gefunden im Resources-Ordner: {filePathInResources}.json");
            return new List<T>();
        }

        var jsonString = jsonFile.text;
        var wrappedJson = $"{{\"items\":{jsonString}}}";

        var wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);

        if (wrapper == null || wrapper.items == null) {
            Debug.LogError($"[JsonReader] Fehler beim Parsen der JSON-Datei: {filePathInResources}.json. Überprüfe die JSON-Struktur und die Klasse '{typeof(T).Name}'.");
            var singleObject = JsonUtility.FromJson<T>(jsonString);
            if (singleObject != null) {
                Debug.LogWarning($"[JsonReader] JSON wurde als einzelnes Objekt vom Typ '{typeof(T).Name}' geparst, nicht als Array. Wenn ein Array erwartet wurde, überprüfe die JSON-Struktur oder verwende LoadJsonObjectFromResources.");
                return new List<T> { singleObject };
            }
            return new List<T>();
        }

        return new List<T>(wrapper.items);
    }

    public static T LoadJsonObjectFromResources<T>(string filePathInResources) {
        var jsonFile = Resources.Load<TextAsset>(filePathInResources);

        if (jsonFile == null) {
            Debug.LogError($"[JsonReader] Datei nicht gefunden im Resources-Ordner: {filePathInResources}.json");
            return default;
        }

        var jsonString = jsonFile.text;
        var result = JsonUtility.FromJson<T>(jsonString);

        if (result == null) {
            Debug.LogError($"[JsonReader] Fehler beim Parsen des JSON-Objekts aus: {filePathInResources}.json. Überprüfe die JSON-Struktur und die Klasse '{typeof(T).Name}'.");
        }
        return result;
    }
}