using DG.Tweening;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public abstract class FieldBehaviour : NetworkBehaviour {
    [Header("Field Data")]
    [SerializeField] private readonly SyncList<FieldBehaviour> nextFields = new();
    public SyncList<FieldBehaviour> Next => nextFields;

    [SyncVar]
    [SerializeField] private SplineKnotIndex splineKnotIndex;
    public SplineKnotIndex SplineKnotIndex => splineKnotIndex;
    [SyncVar][SerializeField] private float normalizedSplinePosition;
    public float NormalizedSplinePosition => normalizedSplinePosition;
    [SerializeField] public bool isEditorField;
    public bool IsEditorField => isEditorField;
    [SyncVar]
    [SerializeField] private bool skipStepCount;
    public bool SkipStepCount => skipStepCount;
    [SerializeField] private bool pausesMovement;
    public bool PausesMovement => pausesMovement;
    public Vector3 Position => transform.position;
    public event Action OnFieldInvocationComplete;

    public abstract FieldType GetFieldType();

    public FieldBehaviour Initialize(SplineKnotIndex splineKnotIndex, float normalizedSplinePosition) {
        this.splineKnotIndex = splineKnotIndex;
        this.normalizedSplinePosition = normalizedSplinePosition;
        this.isEditorField = false;
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

        AdjustPlayerPositions();

        yield return new WaitUntil(() => completed);

        OnFieldInvocationComplete -= completionHandler;
    }

    [Server]
    public void AdjustPlayerPositions() {
        var boardPlayers = BoardContext.Instance.GetAllPlayers();
        var playersOnField = new List<BoardPlayer>();

        foreach (var boardPlayer in boardPlayers) {
            if (boardPlayer.SplineKnotIndex == splineKnotIndex) {
                playersOnField.Add(boardPlayer);
            }
        }

        var playersStaying = playersOnField.Where(p => !p.IsMoving).ToList();
        if (playersOnField.Count <= 1 || playersStaying.Count == 0) {
            return;
        }

        var fieldBounds = GetComponent<Renderer>().bounds;
        var fieldCenter = transform.position;
        var fieldWidth = fieldBounds.size.x;
        var fieldHeight = fieldBounds.size.z;

        List<Vector3> targetPositions = CalculatePlayerPositions(fieldCenter, fieldWidth, fieldHeight, playersStaying.Count);

        IEnumerator WaitForHandlersAndAnimate() {
            yield return new WaitUntil(() => playersStaying.All(p => p.VisualHandler != null));

            for (int i = 0; i < playersStaying.Count; i++) {
                var player = playersStaying[i];
                player.RpcTriggerAnimation(AnimationType.RUN);
                player.transform.DOMove(targetPositions[i], 0.5f).SetEase(Ease.InOutQuad).OnComplete(() => {
                    player.RpcTriggerAnimation(AnimationType.IDLE);
                });
            }
        }

        StartCoroutine(WaitForHandlersAndAnimate());
    }

    private List<Vector3> CalculatePlayerPositions(Vector3 fieldCenter, float fieldWidth, float fieldHeight, int playerCount) {
        List<Vector3> positions = new List<Vector3>();
        float halfWidth = fieldWidth / 2f;
        float halfHeight = fieldHeight / 2f;

        if (playerCount == 1) {
            positions.Add(fieldCenter);
        }
        else if (playerCount == 2) {
            positions.Add(fieldCenter + new Vector3(-halfWidth / 2f, 0, halfHeight / 2f));
            positions.Add(fieldCenter + new Vector3(halfWidth / 2f, 0, -halfHeight / 2f));
        }
        else if (playerCount == 3) {
            float radius = Mathf.Max(fieldWidth, fieldHeight) / 2f;
            float angle1 = 90f * Mathf.Deg2Rad;
            positions.Add(fieldCenter + new Vector3(Mathf.Cos(angle1) * radius * 0.6f, 0, Mathf.Sin(angle1) * radius * 0.6f));
            float angle2 = (90f + 240f) * Mathf.Deg2Rad;
            positions.Add(fieldCenter + new Vector3(Mathf.Cos(angle2) * radius * 0.6f, 0, Mathf.Sin(angle2) * radius * 0.6f));
            float angle3 = (90f + 120f) * Mathf.Deg2Rad;
            positions.Add(fieldCenter + new Vector3(Mathf.Cos(angle3) * radius * 0.6f, 0, Mathf.Sin(angle3) * radius * 0.6f));
        }
        else if (playerCount == 4) {
            float quarterWidth = fieldWidth / 2f;
            float quarterHeight = fieldHeight / 2f;
            positions.Add(fieldCenter + new Vector3(-quarterWidth / 2f, 0, quarterHeight / 2f));  // Top-left
            positions.Add(fieldCenter + new Vector3(quarterWidth / 2f, 0, quarterHeight / 2f));   // Top-right
            positions.Add(fieldCenter + new Vector3(-quarterWidth / 2f, 0, -quarterHeight / 2f)); // Bottom-left
            positions.Add(fieldCenter + new Vector3(quarterWidth / 2f, 0, -quarterHeight / 2f));  // Bottom-right
        }

        return positions;
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
        StartCoroutine(DelayedOnStartClient());
    }

    private IEnumerator DelayedOnStartClient() {
        yield return new WaitUntil(() => BoardContext.Instance != null && BoardContext.Instance.FieldBehaviourList != null);
        if (normalizedSplinePosition != -1) {
            transform.SetParent(FieldInstantiate.Instance.SplineContainerTransform, false);
        }
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