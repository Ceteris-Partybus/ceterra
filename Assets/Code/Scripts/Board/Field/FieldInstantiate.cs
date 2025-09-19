using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

// FieldInstantiate creates field behaviours on the spline
public class FieldInstantiate : NetworkedSingleton<FieldInstantiate> {
    protected override bool ShouldPersistAcrossScenes => true;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject normalFieldPrefab;
    [SerializeField] private GameObject questionFieldPrefab;
    [SerializeField] private GameObject eventFieldPrefab;
    [SerializeField] private GameObject catastropheFieldPrefab;

    private readonly SyncDictionary<SplineKnotIndex, FieldType> fieldTypeMap = new();
    private Dictionary<SplineKnotIndex, FieldBehaviour> fields = new();

    protected override void Start() {
        base.Start();
        FillFieldTypeMap();

        var splines = splineContainer.Splines;
        foreach (var (splineId, spline) in splines.Select((s, i) => (i, s))) {
            var isclosed = spline.Closed;
            var knots = spline.Knots;
            var knotStart = isclosed ? 0 : 1;
            var knotEnd = isclosed ? knots.Count() : knots.Count() - 1;

            var first = CreateField(splineId, knotStart++);
            fields.Add(first.SplineKnotIndex, first);

            var previous = first;
            foreach (var knotId in Enumerable.Range(knotStart, knotEnd - knotStart)) {
                var current = CreateField(splineId, knotId);
                fields.Add(current.SplineKnotIndex, current);
                previous.AddNext(current);
                previous = current;
            }
            if (isclosed) {
                previous.AddNext(first);
            }
        }
        LinkBranches();
        BoardContext.Instance.FieldBehaviourList = new FieldBehaviourList(fields);
    }

    private FieldBehaviour CreateField(int splineId, int knotId) {
        var splineKnotIndex = new SplineKnotIndex(splineId, knotId);
        if (!fieldTypeMap.TryGetValue(splineKnotIndex, out var type)) {
            throw new Exception($"Field type for knot not found in fieldTypeMap.");
        }

        var prefab = type switch {
            FieldType.NORMAL => normalFieldPrefab,
            FieldType.QUESTION => questionFieldPrefab,
            FieldType.EVENT => eventFieldPrefab,
            FieldType.CATASTROPHE => catastropheFieldPrefab,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var spline = splineContainer.Splines.ElementAt(splineId);
        var absolutePosition = (float3)splineContainer.transform.position + spline.Knots.ElementAt(knotId).Position;
        var normalizedPosition = spline.ConvertIndexUnit(knotId, PathIndexUnit.Knot, PathIndexUnit.Normalized);
        return Instantiate(prefab, absolutePosition, Quaternion.identity, splineContainer.transform)
                .GetComponent<FieldBehaviour>()
                .Initialize(type, splineKnotIndex, normalizedPosition);
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

    // TODO: Placeholder, replace with actual logic later
    private void FillFieldTypeMap() {
        for (int i = 0; i < splineContainer.Splines.Count(); i++) {
            var spline = splineContainer.Splines.ElementAt(i);
            for (int k = 0; k < spline.Knots.Count(); k++) {
                if (!fieldTypeMap.ContainsKey(new SplineKnotIndex(i, k))) {
                    var fieldTypes = Enum.GetValues(typeof(FieldType)).Cast<FieldType>().ToArray();
                    var randomFieldType = fieldTypes[UnityEngine.Random.Range(0, fieldTypes.Length)];
                    // fieldTypeMap[new SplineKnotIndex(i, k)] = randomFieldType;
                    if (i == 0 && k == 0) {
                        fieldTypeMap[new SplineKnotIndex(i, k)] = FieldType.NORMAL;
                    }
                    else {
                        fieldTypeMap[new SplineKnotIndex(i, k)] = FieldType.CATASTROPHE;
                    }
                }
            }
        }
    }
}