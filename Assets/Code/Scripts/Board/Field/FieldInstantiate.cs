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

    private FieldBehaviourList fieldBehaviourList;
    private List<FieldBehaviour> fieldBehaviours;

    private readonly SyncDictionary<SplineKnotIndex, FieldType> fieldTypeMap = new();

    protected override void Start() {
        base.Start();
        this.fieldBehaviourList = new FieldBehaviourList();
        this.fieldBehaviours = new List<FieldBehaviour>();

        FillFieldTypeMap();

        var splines = splineContainer.Splines;
        var linkCollection = splineContainer.KnotLinkCollection;

        for (int i = 0; i < splines.Count(); i++) {
            var knots = splines.ElementAt(i).Knots;

            var physicalKnotId = 0;
            for (int k = 0; k < knots.Count(); k++) {
                if (i != 0 && (k == 0 || k == knots.Count() - 1)) {
                    continue;
                }

                var knot = knots.ElementAt(k);

                // knot.Position is relative to splineContainer, so we need to adjust it
                float3 position = (float3)splineContainer.transform.position + knot.Position;

                if (!fieldTypeMap.TryGetValue(new SplineKnotIndex(i, k), out var fieldType)) {
                    throw new Exception($"Field type for knot {i}, {k} not found in fieldTypeMap.");
                }

                // Create the appropriate field prefab based on type
                GameObject fieldPrefab = fieldType switch {
                    FieldType.NORMAL => normalFieldPrefab,
                    FieldType.QUESTION => questionFieldPrefab,
                    FieldType.EVENT => eventFieldPrefab,
                    FieldType.CATASTROPHE => catastropheFieldPrefab,
                    _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null)
                };

                GameObject fieldGameObject = Instantiate(fieldPrefab, position, Quaternion.identity);
                fieldGameObject.transform.SetParent(splineContainer.transform);
                FieldBehaviour fieldBehaviour = fieldGameObject.GetComponent<FieldBehaviour>();

                fieldBehaviour.Initialize(physicalKnotId++, i, fieldType, new SplineKnotIndex(i, k), new Vector3(position.x, position.y, position.z));
                fieldBehaviours.Add(fieldBehaviour);

                if (i == 0 && k == 0) {
                    fieldBehaviourList.Head = fieldBehaviour;
                }
            }
        }

        BoardContext.Instance.FieldBehaviourList = fieldBehaviourList;

        for (int i = 0; i < fieldBehaviours.Count(); i++) {
            var fieldBehaviour = fieldBehaviours.ElementAt(i);
            SplineKnotIndex splineKnotIndex;

            if (fieldBehaviour.SplineId == 0 || fieldBehaviour.FieldId != CountSplineFields(fieldBehaviour.SplineId) - 1) {
                splineKnotIndex = fieldBehaviour.SplineKnotIndex;
            }
            else {
                splineKnotIndex = new SplineKnotIndex(fieldBehaviour.SplineId, CountSplineFields(fieldBehaviour.SplineId) + 1);
            }

            linkCollection.TryGetKnotLinks(splineKnotIndex, out var links);

            if (fieldBehaviour.SplineId == 0 || fieldBehaviour.FieldId != CountSplineFields(fieldBehaviour.SplineId) - 1) {
                fieldBehaviour.AddNext(FindFieldBehaviour(fieldBehaviour.SplineId, (fieldBehaviour.FieldId + 1) % CountSplineFields(fieldBehaviour.SplineId)));
            }

            if (links != null) {
                var relevantLinks = links.Where((link) => {
                    return link.Spline != fieldBehaviour.SplineId;
                });

                for (int k = 0; k < relevantLinks.Count(); k++) {
                    var link = relevantLinks.ElementAt(k);

                    if (fieldBehaviour.SplineId == 0 && link.Knot != 0) {
                        continue;
                    }

                    if (link.Knot == 0) {
                        fieldBehaviour.AddNext(FindFieldBehaviour(link.Spline, link.Knot));
                    }
                    else {
                        fieldBehaviour.AddNext(FindFieldBehaviour(link.Spline, link.Knot));
                    }
                }
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

    private int CountSplineFields(int splineId) {
        return this.fieldBehaviours.Count((fieldBehaviour) => {
            return fieldBehaviour.SplineId == splineId;
        });
    }

    private FieldBehaviour FindFieldBehaviour(int splineId, int fieldId) {
        return this.fieldBehaviours.First((fieldBehaviour) => {
            return fieldBehaviour.SplineId == splineId && fieldBehaviour.FieldId == fieldId;
        });
    }
}