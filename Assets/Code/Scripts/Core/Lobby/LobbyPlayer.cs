using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[Serializable]
public class MaterialColorInfo {
    public int index;
    public Color color;

    public MaterialColorInfo() { }
}

public class LobbyPlayer : NetworkRoomPlayer {
    private const int INVALID_INDEX = -1;
    [SyncVar] private int ping;
    public int Ping => ping;
    [SyncVar] private string playerName;
    public string PlayerName => playerName;

    [SyncVar(hook = nameof(OnSelectedCharacterChanged))] private int selectedCharacterIndex = INVALID_INDEX;
    private GameObject currentCharacterInstance;
    public GameObject CurrentCharacterInstance => currentCharacterInstance;
    [SyncVar(hook = nameof(OnSelectedDiceChanged))] private int selectedDiceIndex = INVALID_INDEX;
    private GameObject currentDiceInstance;
    public GameObject CurrentDiceInstance => currentDiceInstance;

    [SyncVar(hook = nameof(OnMaterialColorsChanged))]
    private MaterialColorInfo[] materialColorInfo;

    private GameObject CharacterModel => GameManager.Singleton.GetCharacter(selectedCharacterIndex);
    private GameObject DiceModel => GameManager.Singleton.GetDice(selectedDiceIndex);

    private Coroutine faceCameraCoroutine;

    private void OnMaterialColorsChanged(MaterialColorInfo[] _old, MaterialColorInfo[] _new) {
        if (currentCharacterInstance != null) {
            ApplyMaterialColors();
        }
    }

    public override void OnClientEnterRoom() {
        gameObject.transform.position = LobbySpawnPointManager.Instance.GetSpawnPoint(index).position;
        faceCameraCoroutine ??= StartCoroutine(FaceCameraCoroutine());

        IEnumerator FaceCameraCoroutine() {
            while (true) {
                currentCharacterInstance?.GetComponent<Character>().FaceCamera();
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void StopFacingCameraCoroutine() {
        if (faceCameraCoroutine != null) {
            StopCoroutine(faceCameraCoroutine);
            faceCameraCoroutine = null;
        }
    }

    public override void IndexChanged(int _old, int _new) {
        gameObject.transform.position = LobbySpawnPointManager.Instance.GetSpawnPoint(_new).position;
    }

    [Command]
    public void CmdSetCharacterSelection(int characterIndex, int diceIndex, string playerName, MaterialColorInfo[] materialColorInfo) {
        this.selectedCharacterIndex = characterIndex;
        this.selectedDiceIndex = diceIndex;
        this.playerName = playerName;
        this.materialColorInfo = materialColorInfo;
    }

    [Command]
    private void CmdChangeSelectedCharacter() {
        ChangeSelectedCharacter();
    }

    [Command]
    private void CmdChangeSelectedDice() {
        ChangeSelectedDice();
    }

    private void OnSelectedCharacterChanged(int _old, int _new) {
        ChangeSelectedCharacter();
        if (isLocalPlayer) { CmdChangeSelectedCharacter(); }
    }

    private void OnSelectedDiceChanged(int _old, int _new) {
        ChangeSelectedDice();
        if (isLocalPlayer) { CmdChangeSelectedDice(); }
    }

    private void ChangeSelectedCharacter() {
        if (currentCharacterInstance != null) {
            DetachDiceFromCharacter();
            Destroy(currentCharacterInstance);
        }
        currentCharacterInstance = Instantiate(CharacterModel, transform);
        ApplyMaterialColors();
        AttachDiceToCharacter();
    }

    private void ApplyMaterialColors() {
        if (materialColorInfo == null) { return; }

        var smr = currentCharacterInstance.GetComponentInChildren<SkinnedMeshRenderer>();
        foreach (var colorInfo in materialColorInfo) {
            smr.materials[colorInfo.index].color = colorInfo.color;
        }
    }

    private void ChangeSelectedDice() {
        if (currentDiceInstance != null) { Destroy(currentDiceInstance); }
        currentDiceInstance = Instantiate(DiceModel);
        currentDiceInstance.GetComponent<Dice>().Hide();
        AttachDiceToCharacter();
    }

    private void AttachDiceToCharacter() {
        var dicePosition = transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.CompareTag("DicePosition"));
        currentDiceInstance?.transform.SetParent(dicePosition, false);
    }

    private void DetachDiceFromCharacter() {
        currentDiceInstance?.transform.SetParent(null, false);
    }

    private const float PING_UPDATE_INTERVAL = 2f;
    private Coroutine pingUpdateCoroutine;
    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();
        pingUpdateCoroutine = StartCoroutine(UpdatePing());

        IEnumerator UpdatePing() {
            yield return new WaitUntil(() => NetworkClient.ready);
            while (isLocalPlayer) {
                CmdUpdatePing((int)(NetworkTime.rtt * 1000));
                yield return new WaitForSeconds(PING_UPDATE_INTERVAL);
            }
        }
    }

    public override void OnStopLocalPlayer() {
        base.OnStopLocalPlayer();
        if (pingUpdateCoroutine != null) {
            StopCoroutine(pingUpdateCoroutine);
            pingUpdateCoroutine = null;
        }
    }

    [Command]
    private void CmdUpdatePing(int newPing) {
        ping = newPing;
    }
}