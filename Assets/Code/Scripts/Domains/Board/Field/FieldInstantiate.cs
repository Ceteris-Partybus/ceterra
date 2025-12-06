using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class FieldInstantiate : NetworkedSingleton<FieldInstantiate> {
    protected override bool ShouldPersistAcrossScenes => true;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject normalFieldPrefab;
    public Material[] NormalFieldMaterial => normalFieldPrefab.GetComponent<Renderer>().sharedMaterials;
    [SerializeField] private GameObject questionFieldPrefab;
    [SerializeField] private GameObject eventFieldPrefab;
    [SerializeField] private GameObject catastropheFieldPrefab;
    [SerializeField] private GameObject junctionFieldPrefab;
    [SerializeField] private GameObject ledgeFieldPrefab;
    [SerializeField] private GameObject startFieldPrefab;
    private readonly SyncDictionary<SplineKnotIndex, FieldBehaviour> fields = new();
    public Transform SplineContainerTransform => splineContainer.transform;

    protected override void Start() {
        if (isClient) {
            SetFieldBehaviourList();
            AlignKnotsWithFields();
        }
        base.Start();
    }

    private void AlignKnotsWithFields() {
        foreach (var field in fields.Values) {
            var spline = splineContainer.Splines[field.SplineKnotIndex.Spline];
            var knotId = field.SplineKnotIndex.Knot;
            var knot = spline.Knots.ElementAt(knotId);
            knot.Position = field.transform.position;
            spline.SetKnot(knotId, knot);
        }
    }

    public void ClearEditorFields() {
        if (!IsInitialized) { return; }
        foreach (var fieldToDestroy in FindAllEditorFields()) {
            Destroy(fieldToDestroy.gameObject);
        }
    }

    [ClientRpc]
    public void RpcClearEditorFields() {
        ClearEditorFields();
    }

    public override void OnStartServer() {
        var initialFields = FindAllEditorFields();
        var splines = splineContainer.Splines;
        foreach (var (splineId, spline) in splines.Select((s, i) => (i, s))) {
            var isClosed = spline.Closed;
            var knots = spline.Knots;
            var knotStart = isClosed ? 0 : 1;
            var knotEnd = isClosed ? knots.Count() : knots.Count() - 1;

            var first = MatchKnotAndCreateField(initialFields, splineId, knotStart, knots.ElementAt(knotStart));
            var previous = first;
            foreach (var knotId in Enumerable.Range(knotStart, knotEnd - knotStart)) {
                var current = MatchKnotAndCreateField(initialFields, splineId, knotId, knots.ElementAt(knotId));
                if (current != null) {
                    previous.AddNext(current);
                    previous = current;
                }
            }
            if (isClosed) {
                previous.AddNext(first);
            }
        }
        LinkBranches();

        SetFieldBehaviourList();
    }

    private FieldBehaviour MatchKnotAndCreateField(List<FieldBehaviour> initialFields, int splineId, int knotId, BezierKnot knot) {
        var worldKnotPosition = splineContainer.transform.TransformPoint(knot.Position);
        if (HasMatchingKnot(worldKnotPosition, initialFields, out var matchedField)) {
            var current = CreateField(splineId, knotId, matchedField);
            fields.Add(current.SplineKnotIndex, current);
            Destroy(matchedField.gameObject);
            return current;
        }
        return null;
    }

    private bool HasMatchingKnot(Vector3 position, List<FieldBehaviour> initialFields, out FieldBehaviour matchedField) {
        for (var i = 0; i < initialFields.Count; i++) {
            var field = initialFields[i];
            var meshFilter = field.GetComponent<MeshFilter>();
            var fieldRadius = meshFilter.mesh.bounds.extents.x;
            if (Vector3.Distance(field.Position, position) <= fieldRadius) {
                matchedField = field;
                initialFields.RemoveAt(i);
                return true;
            }
        }
        matchedField = null;
        return false;
    }

    private List<FieldBehaviour> FindAllEditorFields() {
        return FindObjectsByType<FieldBehaviour>(FindObjectsSortMode.None).Where(f => f.IsEditorField).ToList();
    }

    private void SetFieldBehaviourList() {
        BoardContext.Instance.FieldBehaviourList = new FieldBehaviourList(fields);
    }

    private FieldBehaviour CreateField(int splineId, int knotId, FieldBehaviour field) {
        var splineKnotIndex = new SplineKnotIndex(splineId, knotId);
        var spline = splineContainer.Splines.ElementAt(splineId);
        var fieldInstance = Instantiate(GetPrefabByType(field.GetFieldType()), field.Position, Quaternion.identity);
        fieldInstance.transform.SetParent(splineContainer.transform);

        var knot = spline.Knots.ElementAt(knotId);
        knot.Position = splineContainer.transform.InverseTransformPoint(fieldInstance.transform.position);
        spline.SetKnot(knotId, spline.ElementAt(knotId));

        var normalizedPosition = spline.ConvertIndexUnit(knotId, PathIndexUnit.Knot, PathIndexUnit.Normalized);
        NetworkServer.Spawn(fieldInstance);
        var instance = fieldInstance.GetComponent<FieldBehaviour>().Initialize(splineKnotIndex, normalizedPosition);

        if (instance is CatastropheFieldBehaviour catastropheField) {
            catastropheField.CatastropheType = ((CatastropheFieldBehaviour)field).CatastropheType;
        }
        else if (instance is LedgeFieldBehaviour ledgeField) {
            ledgeField.jumpPower = ((LedgeFieldBehaviour)field).jumpPower;
        }

        return instance;
    }

    private GameObject GetPrefabByType(FieldType type) {
        return type switch {
            FieldType.NORMAL => normalFieldPrefab,
            FieldType.QUESTION => questionFieldPrefab,
            FieldType.EVENT => eventFieldPrefab,
            FieldType.CATASTROPHE => catastropheFieldPrefab,
            FieldType.JUNCTION => junctionFieldPrefab,
            FieldType.LEDGE => ledgeFieldPrefab,
            FieldType.START => startFieldPrefab,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private void LinkBranches() {
        var linkCollection = splineContainer.KnotLinkCollection;
        foreach (var (i, currentField) in fields) {
            var knotLinks = linkCollection.GetKnotLinks(i);
            if (knotLinks == null) { continue; }

            foreach (var link in knotLinks.Where(link => link != i)) {
                if (link.Knot == 0) {
                    fields.TryGetValue(new SplineKnotIndex(link.Spline, link.Knot + 1), out var nextField);
                    currentField.AddNext(nextField);
                    continue;
                }
                fields.TryGetValue(new SplineKnotIndex(link.Spline, link.Knot - 1), out var previousField);
                previousField.AddNext(currentField);
            }
        }
    }

    [Server]
    public void ReplaceField(FieldBehaviour oldField, FieldType newType) {
        var fieldInstance = Instantiate(GetPrefabByType(newType), oldField.transform.localPosition, Quaternion.identity);
        fieldInstance.transform.SetParent(oldField.transform.parent, false);

        NetworkServer.Spawn(fieldInstance);

        var newFieldInstance = fieldInstance.GetComponent<FieldBehaviour>().Initialize(oldField.SplineKnotIndex, oldField.NormalizedSplinePosition);
        fields[oldField.SplineKnotIndex] = newFieldInstance;

        foreach (var field in fields.Values.Where(f => f.Next.Contains(oldField))) {
            var oldFieldIndex = field.Next.IndexOf(oldField);
            field.Next.RemoveAt(oldFieldIndex);
            field.Next.Insert(oldFieldIndex, newFieldInstance);
        }

        newFieldInstance.Next.AddRange(oldField.Next);

        NetworkServer.Destroy(oldField.gameObject);
    }
}