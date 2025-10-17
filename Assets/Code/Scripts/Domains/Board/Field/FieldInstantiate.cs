using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class FieldInstantiate : NetworkedSingleton<FieldInstantiate> {
    protected override bool ShouldPersistAcrossScenes => true;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private InspectorFieldTypeMap inspectorFieldTypeMap;
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
        base.Start();
        if (isServer) { return; }
        SetFieldBehaviourList();
    }

    public override void OnStartServer() {
        var fieldTypeMap = inspectorFieldTypeMap.ToDictionary();
        var splines = splineContainer.Splines;
        foreach (var (splineId, spline) in splines.Select((s, i) => (i, s))) {
            var isclosed = spline.Closed;
            var knots = spline.Knots;
            var knotStart = isclosed ? 0 : 1;
            var knotEnd = isclosed ? knots.Count() : knots.Count() - 1;

            var first = CreateField(splineId, knotStart++, fieldTypeMap);
            fields.Add(first.SplineKnotIndex, first);

            var previous = first;
            foreach (var knotId in Enumerable.Range(knotStart, knotEnd - knotStart)) {
                var current = CreateField(splineId, knotId, fieldTypeMap);
                if (current == null) { continue; }
                fields.Add(current.SplineKnotIndex, current);
                previous.AddNext(current);
                previous = current;
            }
            if (isclosed) {
                previous.AddNext(first);
            }
        }
        LinkBranches();

        SetFieldBehaviourList();
    }

    private void SetFieldBehaviourList() {
        BoardContext.Instance.FieldBehaviourList = new FieldBehaviourList(fields);
    }

    private FieldBehaviour CreateField(int splineId, int knotId, Dictionary<SplineKnotIndex, FieldType> fieldTypeMap) {
        var splineKnotIndex = new SplineKnotIndex(splineId, knotId);
        if (!fieldTypeMap.TryGetValue(splineKnotIndex, out var type)) {
            return null;
        }

        var spline = splineContainer.Splines.ElementAt(splineId);
        var normalizedPosition = spline.ConvertIndexUnit(knotId, PathIndexUnit.Knot, PathIndexUnit.Normalized);
        var fieldInstance = Instantiate(GetPrefabByType(type), spline.Knots.ElementAt(knotId).Position, Quaternion.identity);
        fieldInstance.transform.SetParent(splineContainer.transform, false);

        NetworkServer.Spawn(fieldInstance);
        return fieldInstance.GetComponent<FieldBehaviour>().Initialize(splineKnotIndex, normalizedPosition);
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