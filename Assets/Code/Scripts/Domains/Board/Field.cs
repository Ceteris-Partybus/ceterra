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
    private Transform transform;

    protected Field(int id, int splineId, FieldType type, SplineKnotIndex splineKnotIndex, Vector3 position, Transform transform) {
        this.id = id;
        this.splineId = splineId;
        this.type = type;
        this.next = new List<Field>();
        this.splineKnotIndex = splineKnotIndex;
        this.position = position;
        this.transform = transform;
    }

    public static Field Create(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position, FieldType type, Transform transform) {
        return type switch {
            FieldType.NORMAL => new NormalField(id, splineId, splineKnotIndex, position, transform),
            FieldType.QUESTION => new QuestionField(id, splineId, splineKnotIndex, position, transform),
            FieldType.EVENT => new EventField(id, splineId, splineKnotIndex, position, transform),
            FieldType.CATASTROPHE => new CatastropheField(id, splineId, splineKnotIndex, position, transform),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public abstract void Invoke(BoardPlayer player);

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

    public Transform Transform => transform;

    public override bool Equals(object obj) {
        if (obj is Field other) {
            return splineKnotIndex.Equals(other.splineKnotIndex);
        }
        return false;
    }

    public override int GetHashCode() {
        return splineKnotIndex.GetHashCode();
    }

    public event Action NextAdded;
}
