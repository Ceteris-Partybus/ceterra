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

    [SyncVar]
    [SerializeField] private float normalizedSplinePosition;

    public event Action OnFieldInvocationComplete;

    public void Initialize(int id, int splineId, FieldType type, SplineKnotIndex splineKnotIndex, Vector3 position, float normalizedSplinePosition) {
        this.fieldId = id;
        this.splineId = splineId;
        this.type = type;
        this.splineKnotIndex = splineKnotIndex;
        this.position = position;
        this.normalizedSplinePosition = normalizedSplinePosition;
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
        bool completed = false;
        Action completionHandler = () => completed = true;
        OnFieldInvocationComplete += completionHandler;
        OnFieldInvoked(player);
        yield return new WaitUntil(() => completed);

        OnFieldInvocationComplete -= completionHandler;
    }

    protected abstract void OnFieldInvoked(BoardPlayer player);

    protected void CompleteFieldInvocation() {
        OnFieldInvocationComplete?.Invoke();
    }

    public void Hide() {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    public void Show() {
        GetComponent<Renderer>().enabled = true;
    }

    public int FieldId => fieldId;
    public int SplineId => splineId;
    public FieldType Type => type;
    public IReadOnlyList<FieldBehaviour> Next => nextFields.AsReadOnly();
    public SplineKnotIndex SplineKnotIndex => splineKnotIndex;
    public Vector3 Position => position;

    public float NormalizedSplinePosition => normalizedSplinePosition;

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