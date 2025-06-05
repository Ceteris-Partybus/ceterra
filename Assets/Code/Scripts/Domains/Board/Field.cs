using System;
using System.Collections.Generic;
using UnityEngine.Splines;

public class Field {
    private int id;
    private int splineId;
    private FieldType type;
    private List<Field> next;
    private SplineKnotIndex splineKnotIndex;

    public Field(int id, int splineId, FieldType type, SplineKnotIndex splineKnotIndex) {
        this.id = id;
        this.splineId = splineId;
        this.type = type;
        this.next = new List<Field>();
        this.splineKnotIndex = splineKnotIndex;
    }

    public void AddNext(Field field) {
        this.next.Add(field);
        NextAdded?.Invoke();
    }

    public int Id {
        get {
            return this.id;
        }
    }

    public int SplineId {
        get {
            return this.splineId;
        }
    }

    public FieldType Type {
        get {
            return this.type;
        }
        set {
            this.type = value;
        }
    }

    public IReadOnlyList<Field> Next {
        get {
            return this.next.AsReadOnly();
        }
    }

    public SplineKnotIndex SplineKnotIndex {
        get {
            return this.splineKnotIndex;
        }
    }

    public event Action NextAdded;
}