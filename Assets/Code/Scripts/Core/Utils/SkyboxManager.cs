using Mirror;
using System;
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
    private float sunLightIntensityDuration = 3f;
    private float sunLightIntensityInitialValue;

    protected override void Start() {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out hdriskybox);
        volume.profile.TryGet(out fog);
        smokeAttenuationInitalValue = fog.meanFreePath.value;
        sunLightIntensityInitialValue = sunLight.intensity;
        base.Start();
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
        yield return Animate(
            value => fog.meanFreePath.value = value,
            fog.meanFreePath.value,
            end,
            smokeAttenuationDuration
        );
    }

    [Server]
    public void IncreaseSunlight(float targetIntensity) {
        Debug.Log($"Increasing sunlight to {targetIntensity}");
        RpcIncreaseSunlight(targetIntensity);
    }

    [Server]
    public void ResetSunlight() {
        RpcIncreaseSunlight(sunLightIntensityInitialValue);
    }

    [ClientRpc]
    private void RpcIncreaseSunlight(float targetIntensity) {
        StartCoroutine(AnimateSunlight(targetIntensity));
    }

    private IEnumerator AnimateSunlight(float end) {
        yield return Animate(
            value => sunLight.intensity = value,
            sunLight.intensity,
            end,
            sunLightIntensityDuration
        );
    }

    private IEnumerator Animate(Action<float> setValue, float start, float end, float duration) {
        var elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            setValue(Mathf.Lerp(start, end, t));
            yield return null;
        }

        setValue(end);
    }

    [Client]
    public void OnMinigameStarted() {
        SetComponents(false);
    }

    [ClientRpc]
    public void RpcOnBoardSceneEntered() {
        SetComponents(true);
    }

    private void SetComponents(bool isActive) {
        if (!IsInitialized) {
            return;
        }
        fog.active = isActive;
        sunLight.enabled = isActive;
    }
}