using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Field {
    private int id;
    private Lane lane;
    private FieldType type;
    private List<Field> nextFields;
    private Vector3 position;
    private SplineKnotIndex splineKnotIndex;

    internal Field(int id, Lane lane, FieldType type, SplineKnotIndex splineKnotIndex, Vector3 position) {
        this.id = id;
        this.lane = lane;
        this.type = type;
        this.splineKnotIndex = splineKnotIndex;
        this.position = position;
        this.nextFields = new List<Field>();
    }

    public int Id {
        get {
            return id;
        }
    }

    public Lane Lane {
        get {
            return lane;
        }
    }

    public FieldType Type {
        get {
            return type;
        }
    }

    public bool IsIntersection {
        get {
            return nextFields.Count > 1;
        }
    }

    public IReadOnlyList<Field> NextFields {
        get {
            return nextFields.AsReadOnly();
        }
    }

    public SplineKnotIndex SplineKnotIndex {
        get {
            return splineKnotIndex;
        }
    }

    public Vector3 Position {
        get {
            return position;
        }
    }

    public void AddNextField(Field nextField) {
        if (nextField != null && !nextFields.Contains(nextField)) {
            nextFields.Add(nextField);
        }
    }

    public static FieldBuilder Builder() {
        return new FieldBuilder();
    }

    public class FieldBuilder {
        private int id;
        private Lane lane;
        private FieldType type;
        private Vector3 position;
        private SplineKnotIndex splineKnotIndex;

        public FieldBuilder WithId(int id) {
            this.id = id;
            return this;
        }

        public FieldBuilder WithLane(Lane lane) {
            this.lane = lane;
            return this;
        }

        public FieldBuilder WithType(FieldType type) {
            this.type = type;
            return this;
        }

        public FieldBuilder WithPosition(Vector3 position) {
            this.position = position;
            return this;
        }

        public FieldBuilder WithSplineKnotIndex(SplineKnotIndex splineKnotIndex) {
            this.splineKnotIndex = splineKnotIndex;
            return this;
        }

        public Field Build() {
            return new Field(id, lane, type, splineKnotIndex, position);
        }
    }
}