using System;
using UnityEngine.Splines;
using System.Collections.Generic;

// List to manage FieldBehaviour objects
public class FieldBehaviourList {
    private readonly Dictionary<SplineKnotIndex, FieldBehaviour> cache = new();
    private FieldBehaviour head;

    public FieldBehaviourList() { }

    public FieldBehaviour Head {
        get => head;
        set => head = value;
    }

    public FieldBehaviour Find(SplineKnotIndex splineKnotIndex) {
        if (cache.Count == 0) {
            InitializeCache();
        }

        if (cache.TryGetValue(splineKnotIndex, out FieldBehaviour field)) {
            return field;
        }

        throw new InvalidOperationException($"Field with SplineKnotIndex {splineKnotIndex} not found in cache.");
    }

    private void InitializeCache() {
        var head = BoardContext.Instance?.FieldBehaviourList?.Head;
        if (head == null) {
            throw new InvalidOperationException("BoardContext or FieldList is not initialized yet.");
        }

        var queue = new Queue<FieldBehaviour>();
        var visited = new HashSet<FieldBehaviour>();

        queue.Enqueue(head);
        visited.Add(head);

        while (queue.Count > 0) {
            var current = queue.Dequeue();
            cache[current.SplineKnotIndex] = current;

            foreach (var next in current.Next) {
                if (!visited.Contains(next)) {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
    }
}
