using System;
using UnityEngine.Splines;
using System.Collections.Generic;

public class FieldBehaviourList {
    private readonly Dictionary<SplineKnotIndex, FieldBehaviour> fields = new();

    public FieldBehaviourList(Dictionary<SplineKnotIndex, FieldBehaviour> fields) {
        this.fields = fields;
    }

    public FieldBehaviour Find(SplineKnotIndex i) {
        if (fields.TryGetValue(i, out var field)) {
            return field;
        }

        throw new InvalidOperationException($"Field with SplineKnotIndex {i} not found.");
    }
}
