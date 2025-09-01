using System;
using UnityEngine.Splines;
using System.Collections.Generic;

// List to manage FieldBehaviour objects
public class FieldBehaviourList {
    private FieldBehaviour head;

    public FieldBehaviourList() {
    }

    public FieldBehaviour Head {
        get => head;
        set => head = value;
    }

    public FieldBehaviour Find(SplineKnotIndex splineKnotIndex) {
        if (head == null) {
            throw new Exception("FieldBehaviourList is empty.");
        }

        var queue = new Queue<FieldBehaviour>();
        var visited = new HashSet<FieldBehaviour>();

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
        throw new Exception($"FieldBehaviour with SplineKnotIndex {splineKnotIndex} not found.");
    }
}
