using System;
using UnityEngine.Splines;

public class FieldList {
    private Field head;

    public FieldList() {
    }

    public Field Head {
        get => head;
        set => head = value;
    }

    public Field Find(SplineKnotIndex splineKnotIndex) {
        if (head == null) {
            throw new Exception("FieldList is empty.");
        }

        Field current = head;
        while (current != null) {
            if (current.SplineKnotIndex.Equals(splineKnotIndex)) {
                return current;
            }
            current = current.Next.Count > 0 ? current.Next[0] : null; // Assuming a single next for simplicity
        }

        throw new Exception($"Field with SplineKnotIndex {splineKnotIndex} not found.");
    }
}