using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyboxManager : NetworkedSingleton<SkyboxManager> {
    private Volume volume;
    [SerializeField] private Light sunLight;
    [SerializeField] private float rotationSpeed = 1f;
   
    private HDRISky hdriskybox;

    protected override void Start() {
        base.Start();
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out hdriskybox);
    }
    
    void FixedUpdate() {
        var newRotation = (hdriskybox.rotation.value + Time.fixedDeltaTime * rotationSpeed) % 360f;
        hdriskybox.rotation.value = newRotation;
        sunLight.transform.rotation = Quaternion.Euler(sunLight.transform.rotation.eulerAngles.x, newRotation, sunLight.transform.rotation.eulerAngles.z);
    }
}