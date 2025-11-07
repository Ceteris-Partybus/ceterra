using System;
using UnityEngine.Splines;
using Mirror;

public class FieldBehaviourList {
    private readonly SyncDictionary<SplineKnotIndex, FieldBehaviour> fields = new();

    public FieldBehaviourList(SyncDictionary<SplineKnotIndex, FieldBehaviour> fields) {
        this.fields = fields;
    }

    public FieldBehaviour Find(SplineKnotIndex i) {
        if (fields.TryGetValue(i, out var field)) {
            return field;
        }

        throw new InvalidOperationException($"Field with SplineKnotIndex {i} not found.");
    }
}
