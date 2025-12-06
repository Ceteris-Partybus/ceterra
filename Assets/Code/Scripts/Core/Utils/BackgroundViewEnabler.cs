using System.Linq;
using UnityEngine;

public class BackgroundViewEnabler : MonoBehaviour {
    void Update() {
        try {
            FindObjectsByType<FieldBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList().ForEach(go => go.gameObject.SetActive(true));
            FindObjectsByType<SkyboxManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList().First().gameObject.SetActive(true);
        }
        catch { }
    }
}
