using System;
using UnityEngine.Splines;
using System.Collections.Generic;

public class FieldList {
    private readonly Dictionary<SplineKnotIndex, Field> cache = new();

    private Field head;

    public FieldList() { }

    public Field Head {
        get => head;
        set => head = value;
    }

    public Field Find(SplineKnotIndex splineKnotIndex) {
        if (cache.Count == 0) {
            InitializeCache();
        }

        if (cache.TryGetValue(splineKnotIndex, out Field field)) {
            return field;
        }

        throw new InvalidOperationException($"Field with SplineKnotIndex {splineKnotIndex} not found in cache.");
    }

    private void InitializeCache() {
        var head = BoardContext.Instance?.FieldList?.Head;
        if (head == null) {
            throw new InvalidOperationException("BoardContext or FieldList is not initialized yet.");
        }

        var queue = new Queue<Field>();
        var visited = new HashSet<Field>();

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