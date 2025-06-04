using Mirror.BouncyCastle.Asn1.Misc;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class KnotInstantiate : MonoBehaviour {
    [SerializeField] private GameObject knotPrefab;
    [SerializeField] private GameState gameState;
    [SerializeField] private SplineContainer splineContainer;

    void Start() {
        foreach (var spline in splineContainer.Splines.Select((value, index) => {
            return new { value, index };
        })) {
            var lane = new Lane(spline.index);
            gameState.Board.AddLane(lane);
            var knotIndex = 0;
            foreach (var knot in spline.value.Knots.Select((value, index) => {
                return new { value, index };
            })) {
                // Skip first and last knots for splines after the first one
                // First knot of index > 1 is the branch point of the main spline
                // Last knot of index > 1 is the merge point of the main spline
                if (spline.index > 0 && (knot.index == 0 || knot.index == spline.value.Knots.Count() - 1)) {
                    continue;
                }

                GameObject newKnot = Instantiate(knotPrefab, knot.value.Position, Quaternion.identity);
                newKnot.transform.SetParent(splineContainer.transform); // Must be called before setting properties, as the logic in KnotBehaviour.Start() requires the parent to be set.
                // SetKnotProperties(newKnot, spline.index, knot.index);

                var field = Field.Builder()
                    .WithId(knotIndex++)
                    .WithLane(lane)
                    .WithType(GetFieldType(spline.index, knotIndex))
                    .WithPosition(knot.value.Position)
                    .WithSplineKnotIndex(new SplineKnotIndex(spline.index, knot.index))
                    .Build();

                lane.AddField(field);
            }
        }
    }

    private FieldType GetFieldType(int splineId, int knotId) {
        // TODO: Take splineId into account if needed
        // For now, knotId determines the type
        return knotId switch {
            0 => FieldType.START,
            1 => FieldType.QUESTION,
            2 => FieldType.EVENT,
            3 => FieldType.CATASTROPHE,
            _ => FieldType.NORMAL,
        };
    }

    private Color GetFieldColor(FieldType fieldType) {
        return fieldType.ToColor();
    }

    // private void SetKnotProperties(GameObject knot, int splineId, int knotId) {
    //     KnotBehaviour knotBehaviour = knot.GetComponent<KnotBehaviour>();
    //     var id = new SplineKnotIndex(splineId, knotId);
    //     if (knotBehaviour != null) {
    //         knotBehaviour.Id = id;
    //         knotBehaviour.IsIntersection = IsIntersection(id);
    //     }
    //     knot.GetComponent<Renderer>().material.color = GetFieldColor(GetFieldType(splineId, knotId));
    //     knot.name = KnotUtil.GetKnotName(id);
    // }

    // private bool IsIntersection(SplineKnotIndex id) {
    //     var linkCollection = splineContainer.KnotLinkCollection;
    //     var links = linkCollection.GetKnotLinks(id);

    //     var validLinks = new Collection<SplineKnotIndex>();
    //     // interseciton class?
    //     // wrapper classes for splines (lines, field) instead of splines and knots
    //     // => field.onLand(player)

    //     return links.Count > 1;
    // }
}
