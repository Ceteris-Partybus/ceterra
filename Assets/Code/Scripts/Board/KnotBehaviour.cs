using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class KnotBehaviour : MonoBehaviour {
    // TODO: Rename to Fieldbehaviour and have Field instance as a property.
    // All logic should be in the domain class, this should just handle the effects it has to the actual gameObject.
    [SerializeField] private SplineKnotIndex id;
    [SerializeField] private bool isIntersection = false;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private List<KnotBehaviour> nextKnotOptions;

    void Start() {
        // Settings the splineContainer from the inspector is not possible as this script is attached to a prefab.
        splineContainer = transform.parent.GetComponent<SplineContainer>();
    }

    public SplineKnotIndex Id {
        get {
            return id;
        }
        set {
            id = value;
            nextKnotOptions = NextKnotOptions().ToList();
        }
    }

    public bool IsIntersection {
        get {
            return isIntersection;
        }
        set {
            isIntersection = value;
        }
    }

    public ReadOnlyCollection<KnotBehaviour> NextKnotOptions() {
        splineContainer = transform.parent.GetComponent<SplineContainer>();
        var linkCollection = splineContainer.KnotLinkCollection;
        var links = linkCollection.GetKnotLinks(id);

        var connectedKnots = new List<KnotBehaviour>();

        foreach (var link in links.Select((value, index) => {
            return new { value, index };
        })) {
            // Skip the first and last link as they are seen as part of the main spline
            var id = new SplineKnotIndex(link.value.Spline, link.index == 0 ? 1 : link.index - 1);
            var knotName = KnotUtil.GetKnotName(id);
            var knotGameObject = GameObject.Find(knotName);

            if (knotGameObject != null) {
                var knotBehaviour = knotGameObject.GetComponent<KnotBehaviour>();
                if (knotBehaviour != null) {
                    connectedKnots.Add(knotBehaviour);
                }
            }
        }

        return new ReadOnlyCollection<KnotBehaviour>(connectedKnots);
    }
}
