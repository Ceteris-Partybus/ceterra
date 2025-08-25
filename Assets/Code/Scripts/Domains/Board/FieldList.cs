using System;
using UnityEngine.Splines;
using System.Collections.Generic;

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

        var queue = new Queue<Field>();
        var visited = new HashSet<Field>();

        queue.Enqueue(head);
        visited.Add(head);

        while (queue.Count > 0) {
            var current = queue.Dequeue();
            if (current.SplineKnotIndex.Equals(splineKnotIndex)) {
                return current;
            }

            foreach (var next in current.Next) {
                if (!visited.Contains(next)) {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
        throw new Exception($"Field with SplineKnotIndex {splineKnotIndex} not found.");
    }
}