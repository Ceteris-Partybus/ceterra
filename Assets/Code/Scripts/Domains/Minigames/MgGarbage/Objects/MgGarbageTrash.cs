using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

public class MgGarbageTrash : NetworkBehaviour {
    [SerializeField]
    private float fallSpeed = 2f;

    [SerializeField]
    private MgGarbageTrashType trashType;
    public MgGarbageTrashType TrashType => trashType;

    private GameObject destructionLine;
    public GameObject DestructionLine {
        set { destructionLine = value; }
    }

    private GameObject binsHolder;
    public GameObject BinsHolder {
        set { binsHolder = value; }
    }

    private bool isDragging = false;
    private Vector3 dragOffset;

    [ClientCallback]
    void Update() {
        transform.Rotate(Vector3.forward * 360f * Time.deltaTime);

        if (!isDragging) {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            if (destructionLine != null && transform.position.y <= destructionLine.transform.position.y) {
                Destroy(gameObject);
                if (trashType != MgGarbageTrashType.NONE) {
                    MgGarbageContext.Instance.GetLocalPlayer().CmdSubtractScore(1);
                }
            }
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();

        StartCoroutine(WaitForAllPlayers());
        IEnumerator WaitForAllPlayers() {
            yield return new WaitUntil(() => netIdentity != null && netIdentity.observers.Count == GameManager.Singleton.PlayerIds.Count() && MgGarbageContext.Instance != null);
        }

        if (destructionLine == null) {
            destructionLine = MgGarbageContext.Instance.DestructionLine;
        }
        if (binsHolder == null) {
            binsHolder = MgGarbageContext.Instance.BinsHolder;
        }
    }

    [ClientCallback]
    void OnMouseDown() {
        isDragging = true;
        Debug.Log("Dragging started for: " + gameObject.name);
        float z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
        dragOffset = transform.position - mouseWorld;
    }

    [ClientCallback]
    void OnMouseDrag() {
        float z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
        transform.position = new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z) + dragOffset;
    }

    [ClientCallback]
    void OnMouseUp() {
        if (binsHolder != null) {
            foreach (Transform binTransform in binsHolder.transform) {
                MgGarbageBin bin = binTransform.GetComponent<MgGarbageBin>();
                BoxCollider2D binCollider = binTransform.GetComponent<BoxCollider2D>();
                if (bin != null && binCollider != null) {
                    if (binCollider.OverlapPoint(transform.position)) {
                        if (bin.AcceptedTrashType == this.trashType) {
                            MgGarbageContext.Instance.GetLocalPlayer().CmdAddScore(1);
                        }
                        else {
                            MgGarbageContext.Instance.GetLocalPlayer().CmdSubtractScore(1);
                        }
                        Destroy(gameObject);
                    }
                }
            }
        }

        isDragging = false;
    }
}