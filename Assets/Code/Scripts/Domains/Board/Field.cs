using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public abstract class Field {
    private int id;
    private int splineId;
    private FieldType type;
    private List<Field> next;
    private SplineKnotIndex splineKnotIndex;
    private Vector3 position;

    protected Field(int id, int splineId, FieldType type, SplineKnotIndex splineKnotIndex, Vector3 position) {
        this.id = id;
        this.splineId = splineId;
        this.type = type;
        this.next = new List<Field>();
        this.splineKnotIndex = splineKnotIndex;
        this.position = position;
    }

    public static Field Create(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position, FieldType type) {
        return type switch {
            FieldType.NORMAL => new NormalField(id, splineId, splineKnotIndex, position),
            FieldType.QUESTION => new QuestionField(id, splineId, splineKnotIndex, position),
            FieldType.EVENT => new EventField(id, splineId, splineKnotIndex, position),
            FieldType.CATASTROPHE => new CatastropheField(id, splineId, splineKnotIndex, position),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public abstract void Invoke(BoardPlayer player);

    public Field() {
    }

    public void AddNext(Field field) {
        next.Add(field);
        NextAdded?.Invoke();
    }

    public int Id => id;

    public int SplineId => splineId;

    public FieldType Type {
        get => type;
        set => type = value;
    }

    public IReadOnlyList<Field> Next => next.AsReadOnly();

    public SplineKnotIndex SplineKnotIndex => splineKnotIndex;

    public Vector3 Position => position;

    public override bool Equals(object obj) {
        if (obj is Field other) {
            return id == other.id;
        }
        return false;
    }

    public override int GetHashCode() {
        return id.GetHashCode();
    }

    public event Action NextAdded;
}
