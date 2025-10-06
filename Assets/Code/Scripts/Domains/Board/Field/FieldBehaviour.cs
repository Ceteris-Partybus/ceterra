using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public abstract class FieldBehaviour : NetworkBehaviour {
    [Header("Field Data")]
    [SerializeField] protected readonly SyncList<FieldBehaviour> nextFields = new();
    public SyncList<FieldBehaviour> Next => nextFields;
    [SyncVar]
    [SerializeField] private SplineKnotIndex splineKnotIndex;
    public SplineKnotIndex SplineKnotIndex => splineKnotIndex;
    [SyncVar]
    [SerializeField] private float normalizedSplinePosition;
    public float NormalizedSplinePosition => normalizedSplinePosition;
    [SyncVar]
    [SerializeField] private bool skipStepCount;
    public bool SkipStepCount => skipStepCount;
    [SerializeField] private bool pausesMovement;
    public bool PausesMovement => pausesMovement;
    public Vector3 Position => transform.position;
    public event Action OnFieldInvocationComplete;

    public FieldBehaviour Initialize(SplineKnotIndex splineKnotIndex, float normalizedSplinePosition) {
        this.splineKnotIndex = splineKnotIndex;
        this.normalizedSplinePosition = normalizedSplinePosition;
        return this;
    }

    public void AddNext(FieldBehaviour field) {
        nextFields.Add(field);
    }

    [Server]
    public IEnumerator InvokeOnPlayerLand(BoardPlayer player) {
        bool completed = false;
        Action completionHandler = () => completed = true;
        OnFieldInvocationComplete += completionHandler;
        OnPlayerLand(player);

        yield return new WaitUntil(() => completed);

        OnFieldInvocationComplete -= completionHandler;
    }

    [Server]
    public IEnumerator InvokeOnPlayerCross(BoardPlayer player) {
        bool completed = false;
        Action completionHandler = () => completed = true;
        OnFieldInvocationComplete += completionHandler;
        OnPlayerCross(player);

        yield return new WaitUntil(() => completed);

        OnFieldInvocationComplete -= completionHandler;
    }

    [Server]
    protected virtual void OnPlayerLand(BoardPlayer player) {
        CompleteFieldInvocation();
    }

    [Server]
    protected virtual void OnPlayerCross(BoardPlayer player) {
        CompleteFieldInvocation();
    }

    protected void CompleteFieldInvocation() {
        OnFieldInvocationComplete?.Invoke();
    }

    public void Hide() {
        GetComponent<Renderer>().enabled = false;
    }

    public void Show() {
        GetComponent<Renderer>().enabled = true;
    }

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

    public virtual FieldBehaviour GetNextTargetField() {
        return nextFields.First();
    }
}