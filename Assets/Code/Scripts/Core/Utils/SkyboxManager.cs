using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyboxManager : NetworkedSingleton<SkyboxManager> {
    private Volume volume;
    [SerializeField] private Light sunLight;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private bool shouldPersistAcrossScenes = false;
    protected override bool ShouldPersistAcrossScenes => shouldPersistAcrossScenes;

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
        RpcSpawnSmoke(smokeAttenuationStart);
    }

    [Server]
    public void ClearSmoke() {
        RpcSpawnSmoke(smokeAttenuationInitalValue);
    }

    [Server]
    public void AddSmokeAttenuation(float attenuation) {
        RpcAddSpawnSmoke(attenuation);
    }

    [ClientRpc]
    private void RpcAddSpawnSmoke(float end) {
        StartCoroutine(AnimateAttenuation(fog.meanFreePath.value + end));
    }

    [ClientRpc]
    private void RpcSpawnSmoke(float end) {
        StartCoroutine(AnimateAttenuation(end));
    }

    private IEnumerator AnimateAttenuation(float end) {
        var start = fog.meanFreePath.value;

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