using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public abstract class FieldBehaviour : NetworkBehaviour {

    [Header("Field Data")]
    [SyncVar]
    [SerializeField] private FieldType type;

    [SerializeField] private readonly SyncList<FieldBehaviour> nextFields = new();

    [SyncVar]
    [SerializeField] private SplineKnotIndex splineKnotIndex;

    [SyncVar]
    [SerializeField] private float normalizedSplinePosition;

    public event Action OnFieldInvocationComplete;

    public FieldBehaviour Initialize(FieldType type, SplineKnotIndex splineKnotIndex, float normalizedSplinePosition) {
        this.type = type;
        this.splineKnotIndex = splineKnotIndex;
        this.normalizedSplinePosition = normalizedSplinePosition;
        return this;
    }

    public void AddNext(FieldBehaviour field) {
        nextFields.Add(field);
    }

    [Server]
    public IEnumerator InvokeFieldAsync(BoardPlayer player) {
        bool completed = false;
        Action completionHandler = () => completed = true;
        OnFieldInvocationComplete += completionHandler;
        OnFieldInvoked(player);

        yield return new WaitUntil(() => completed && player.IsAnimationFinished);

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
    public FieldType Type => type;
    public SyncList<FieldBehaviour> Next => nextFields;
    public SplineKnotIndex SplineKnotIndex => splineKnotIndex;
    public Vector3 Position => transform.position;

    public float NormalizedSplinePosition => normalizedSplinePosition;

    public override void OnStartClient() {
        base.OnStartClient();
        transform.SetParent(FieldInstantiate.Instance.SplineContainerTransform, false);
    }

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