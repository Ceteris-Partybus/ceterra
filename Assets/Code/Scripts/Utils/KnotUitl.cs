using UnityEngine.Splines;

public class KnotUtil {
    public static string GetKnotName(SplineKnotIndex id) {
        return $"Knot_{id}";
    }
}