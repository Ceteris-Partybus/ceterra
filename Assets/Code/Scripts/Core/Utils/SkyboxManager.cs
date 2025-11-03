using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyboxManager : NetworkedSingleton<SkyboxManager> {
    private Volume volume;
    [SerializeField] private Light sunLight;
    [SerializeField] private float rotationSpeed = 1f;

    private HDRISky hdriskybox;
    private Fog fog;

    private float smokeAttenuationDuration = 3f;
    private float smokeAttenuationInitalValue;

    protected override void Start() {
        base.Start();
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out hdriskybox);
        volume.profile.TryGet(out fog);
        smokeAttenuationInitalValue = fog.meanFreePath.value;
    }

    void FixedUpdate() {
        var newRotation = (hdriskybox.rotation.value + Time.fixedDeltaTime * rotationSpeed) % 360f;
        hdriskybox.rotation.value = newRotation;
        sunLight.transform.rotation = Quaternion.Euler(sunLight.transform.rotation.eulerAngles.x, newRotation, sunLight.transform.rotation.eulerAngles.z);
    }

    [Server]
    public void SpawnSmoke(float smokeAttenuationStart) {
        RpcSpawnSmoke(smokeAttenuationInitalValue, smokeAttenuationStart);
    }

    [Server]
    public void ClearSmoke() {
        RpcSpawnSmoke(fog.meanFreePath.value, smokeAttenuationInitalValue);
    }

    [Server]
    public void AddSmokeAttenuation(float attenuation) {
        RpcSpawnSmoke(fog.meanFreePath.value, fog.meanFreePath.value + attenuation);
    }

    [ClientRpc]
    private void RpcSpawnSmoke(float start, float end) {
        StartCoroutine(AnimateAttenuation(start, end));
    }

    private IEnumerator AnimateAttenuation(float start, float end) {
        fog.meanFreePath.value = start;

        var elapsed = 0f;
        while (elapsed < smokeAttenuationDuration) {
            elapsed += Time.deltaTime;
            var t = elapsed / smokeAttenuationDuration;
            fog.meanFreePath.value = Mathf.Lerp(start, end, t);
            yield return null;
        }

        fog.meanFreePath.value = end;
    }
}