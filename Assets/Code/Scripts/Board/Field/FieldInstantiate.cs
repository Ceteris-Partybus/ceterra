using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class FieldInstantiate : NetworkBehaviour {
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject fieldPrefab;

    private FieldList fieldList;
    private List<Field> fields;

    private readonly SyncDictionary<SplineKnotIndex, FieldType> fieldTypeMap = new();

    void Start() {
        this.fieldList = new FieldList();
        this.fields = new List<Field>();

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

                // (we cooked 🍜)
                var field = Field.Create(physicalKnotId++, i, new SplineKnotIndex(i, k), new Vector3(position.x, position.y, position.z), fieldType);
                fields.Add(field);

                GameObject fieldGameObject = Instantiate(fieldPrefab, position, Quaternion.identity);
                fieldGameObject.transform.SetParent(splineContainer.transform);
                FieldBehaviour fieldBehaviour = fieldGameObject.GetComponent<FieldBehaviour>();
                fieldBehaviour.Field = field;

                if (i == 0 && k == 0) {
                    fieldList.Head = field;
                }
            }
        }

        BoardContext.Instance.FieldList = fieldList;

        for (int i = 0; i < fields.Count(); i++) {
            var field = fields.ElementAt(i);
            SplineKnotIndex splineKnotIndex;

            if (field.SplineId == 0 || field.Id != CountSplineFields(field.SplineId) - 1) {
                splineKnotIndex = field.SplineKnotIndex;
            }
            else {
                splineKnotIndex = new SplineKnotIndex(field.SplineId, CountSplineFields(field.SplineId) + 1);
            }

            linkCollection.TryGetKnotLinks(splineKnotIndex, out var links);

            if (field.SplineId == 0 || field.Id != CountSplineFields(field.SplineId) - 1) {
                field.AddNext(FindField(field.SplineId, (field.Id + 1) % CountSplineFields(field.SplineId)));
            }

            if (links != null) {
                var relevantLinks = links.Where((link) => {
                    return link.Spline != field.SplineId;
                });

                for (int k = 0; k < relevantLinks.Count(); k++) {
                    var link = relevantLinks.ElementAt(k);

                    if (field.SplineId == 0 && link.Knot != 0) {
                        continue;
                    }

                    if (link.Knot == 0) {
                        field.AddNext(FindField(link.Spline, link.Knot));
                    }
                    else {
                        field.AddNext(FindField(link.Spline, link.Knot));
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
                    fieldTypeMap[new SplineKnotIndex(i, k)] = FieldType.EVENT;
                }
            }
        }
    }

    private int CountSplineFields(int splineId) {
        return this.fields.Count((field) => {
            return field.SplineId == splineId;
        });
    }

    private Field FindField(int splineId, int fieldId) {
        return this.fields.First((field) => {
            return field.SplineId == splineId && field.Id == fieldId;
        });
    }
}