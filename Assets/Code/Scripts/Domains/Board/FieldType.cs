using UnityEngine;

public enum FieldType {
    NORMAL,
    QUESTION,
    EVENT,
    CATASTROPHE
}

public static class FieldTypeExtensions {
    public static Color ToColor(this FieldType fieldType) {
        return fieldType switch {
            FieldType.NORMAL => Color.white,
            FieldType.QUESTION => Color.yellow,
            FieldType.EVENT => Color.blue,
            FieldType.CATASTROPHE => Color.red,
            _ => throw new System.ArgumentOutOfRangeException(nameof(fieldType), fieldType, null)
        };
    }
}