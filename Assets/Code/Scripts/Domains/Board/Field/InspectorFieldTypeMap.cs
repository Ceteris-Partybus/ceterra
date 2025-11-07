using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[Serializable]
public class FieldTypeMapEntry {
    public int SplineId;
    public int KnotId;
    public FieldType FieldType;

    public FieldTypeMapEntry() {
        SplineId = 0;
        KnotId = 0;
        FieldType = FieldType.NORMAL;
    }

    public FieldTypeMapEntry(int splineId, int knotId, FieldType fieldType) {
        SplineId = splineId;
        KnotId = knotId;
        FieldType = fieldType;
    }
}

public class InspectorFieldTypeMap : MonoBehaviour {
    [SerializeField]
    private List<FieldTypeMapEntry> fieldTypeMapEntries = new();

    public List<FieldTypeMapEntry> FieldTypeMapEntries {
        get => fieldTypeMapEntries;
        set => fieldTypeMapEntries = value;
    }

    public Dictionary<SplineKnotIndex, FieldType> ToDictionary() {
        var dictionary = new Dictionary<SplineKnotIndex, FieldType>();
        foreach (var entry in fieldTypeMapEntries) {
            dictionary[new SplineKnotIndex(entry.SplineId, entry.KnotId)] = entry.FieldType;
        }
        return dictionary;
    }
}