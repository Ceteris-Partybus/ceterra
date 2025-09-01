using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public abstract class FieldBehaviour : NetworkBehaviour {

    [Header("Field Data")]
    [SyncVar]
    [SerializeField] private int fieldId;

    [SyncVar]
    [SerializeField] private int splineId;

    [SyncVar]
    [SerializeField] private FieldType type;

    [SerializeField] private List<FieldBehaviour> nextFields = new List<FieldBehaviour>();

    [SyncVar]
    [SerializeField] private SplineKnotIndex splineKnotIndex;

    [SyncVar]
    [SerializeField] private Vector3 position;

    // Event to signal when field invocation is complete
    public event Action OnFieldInvocationComplete;

    public void Initialize(int id, int splineId, FieldType type, SplineKnotIndex splineKnotIndex, Vector3 position) {
        this.fieldId = id;
        this.splineId = splineId;
        this.type = type;
        this.splineKnotIndex = splineKnotIndex;
        this.position = position;
        SetColor();
    }

    public void AddNext(FieldBehaviour field) {
        nextFields.Add(field);
    }

    private void SetColor() {
        var renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = type.ToColor();
        }
    }

    [Server]
    public IEnumerator InvokeFieldAsync(BoardPlayer player) {
        // Start the field invocation
        OnFieldInvoked(player);

        // Wait until the field signals completion
        bool completed = false;
        Action completionHandler = () => completed = true;

        OnFieldInvocationComplete += completionHandler;

        // Wait for completion
        yield return new WaitUntil(() => completed);

        // Clean up
        OnFieldInvocationComplete -= completionHandler;
    }

    protected abstract void OnFieldInvoked(BoardPlayer player);

    // Call this method when the field action is complete
    protected void CompleteFieldInvocation() {
        OnFieldInvocationComplete?.Invoke();
    }

    // Properties
    public int FieldId => fieldId;
    public int SplineId => splineId;
    public FieldType Type => type;
    public IReadOnlyList<FieldBehaviour> Next => nextFields.AsReadOnly();
    public SplineKnotIndex SplineKnotIndex => splineKnotIndex;
    public Vector3 Position => position;

    public override bool Equals(object obj) {
        if (obj is FieldBehaviour other) {
            return splineKnotIndex.Equals(other.splineKnotIndex);
        }
        return false;
    }

    public override int GetHashCode() {
        return splineKnotIndex.GetHashCode();
    }
}