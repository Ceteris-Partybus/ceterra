using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyboxManager : NetworkedSingleton<SkyboxManager> {
    private Volume volume;
    [SerializeField] private Light sunlight;
    public float CurrentSunlightIntensity => sunlight.intensity;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private bool shouldPersistAcrossScenes = false;
    protected override bool ShouldPersistAcrossScenes => shouldPersistAcrossScenes;

    private HDRISky hdriskybox;
    private Fog fog;

    private float smokeAttenuationDuration = 3f;
    private float smokeAttenuationInitalValue;
    private float sunlightIntensityDuration = 3f;
    private float sunlightIntensityInitialValue;

    protected override void Start() {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out hdriskybox);
        volume.profile.TryGet(out fog);
        smokeAttenuationInitalValue = fog.meanFreePath.value;
        sunlightIntensityInitialValue = sunlight.intensity;
        base.Start();
    }

    void FixedUpdate() {
        var newRotation = (hdriskybox.rotation.value + Time.fixedDeltaTime * rotationSpeed) % 360f;
        hdriskybox.rotation.value = newRotation;
        sunlight.transform.rotation = Quaternion.Euler(sunlight.transform.rotation.eulerAngles.x, newRotation, sunlight.transform.rotation.eulerAngles.z);
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
        RpcIncreaseSunlight(sunlightIntensityInitialValue);
    }

    [ClientRpc]
    private void RpcIncreaseSunlight(float targetIntensity) {
        StartCoroutine(AnimateSunlight(targetIntensity));
    }

    private IEnumerator AnimateSunlight(float end) {
        yield return Animate(
            value => sunlight.intensity = value,
            sunlight.intensity,
            end,
            sunlightIntensityDuration
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
        sunlight.enabled = isActive;
    }
}