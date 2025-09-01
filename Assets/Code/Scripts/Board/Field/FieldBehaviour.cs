using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class FieldBehaviour : NetworkBehaviour {

    [Header("Field Data")]
    [SerializeField] private int splineId;
    [SerializeField] private FieldType type;
    [SerializeField] private List<SplineKnotIndex> next;
    [SerializeField] private SplineKnotIndex splineKnotIndex;

    private Field field;

    void OnFieldUpdate() {
        this.splineId = field.SplineId;
        this.type = field.Type;
        this.next = field.Next.Select((f) => {
            return f.SplineKnotIndex;
        }).ToList();
        this.splineKnotIndex = field.SplineKnotIndex;

        SetColor();
    }

    void OnNextAdded() {
        this.next = field.Next.Select((f) => {
            return f.SplineKnotIndex;
        }).ToList();
    }

    private void SetColor() {
        var renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = field.Type.ToColor();
        }
    }

    public Field Field {
        get => field;
        set {
            if (this.field != null) {
                this.field.NextAdded -= OnNextAdded;
            }
            this.field = value;
            if (this.field != null) {
                this.field.NextAdded += OnNextAdded;
            }
            OnFieldUpdate();
        }
    }

    void OnDestroy() {
        if (this.field != null) {
            this.field.NextAdded -= OnNextAdded;
        }
    }
}