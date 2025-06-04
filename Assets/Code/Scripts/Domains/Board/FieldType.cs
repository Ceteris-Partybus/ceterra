using System;
using UnityEngine;

public enum FieldType {
    NORMAL,
    START,
    QUESTION,
    EVENT,
    CATASTROPHE
}
public static class FieldTypeExtensions {
    public static Color ToColor(this FieldType fieldType) {
        return fieldType switch {
            FieldType.NORMAL => Color.yellow,
            FieldType.START => Color.white,
            FieldType.QUESTION => Color.green,
            FieldType.EVENT => Color.blue,
            FieldType.CATASTROPHE => Color.red,
            _ => throw new NotImplementedException(),
        };
    }
}