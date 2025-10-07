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

    [SerializeField]
    private GameObject destructionLine;
    public GameObject DestructionLine {
        set { destructionLine = value; }
    }

    [SerializeField]
    private GameObject binsHolder;
    public GameObject BinsHolder {
        set { binsHolder = value; }
    }

    private bool isDragging = false;
    private Vector3 dragOffset;

    [ClientCallback]
    void Update() {
        if (!isDragging) {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            if (destructionLine != null && transform.position.y <= destructionLine.transform.position.y) {
                Destroy(gameObject);
                MgGarbageContext.Instance.GetLocalPlayer().CmdSubtractScore(1);
            }
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();

        StartCoroutine(WaitForAllPlayers());
        IEnumerator WaitForAllPlayers() {
            yield return new WaitUntil(() => netIdentity != null && netIdentity.observers.Count == GameManager.Singleton.PlayerIds.Count());
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
        Debug.Log("ENDED DRAGGIN");
        if (binsHolder != null) {
            Debug.Log("HOLDER AINT NULL");
            foreach (Transform binTransform in binsHolder.transform) {
                MgGarbageBin bin = binTransform.GetComponent<MgGarbageBin>();
                BoxCollider2D binCollider = binTransform.GetComponent<BoxCollider2D>();
                Debug.Log("BIN: " + bin + " COLLIDER: " + binCollider);
                if (bin != null && binCollider != null) {
                    Debug.Log("BOTH AINT NULL");
                    if (binCollider.OverlapPoint(transform.position)) {
                        Debug.Log("HELL THEY EVEN OVERLAP!");
                        if (bin.AcceptedTrashType == this.trashType) {
                            Debug.Log("TRASH TYPE MATCHES");
                            MgGarbageContext.Instance.GetLocalPlayer().CmdAddScore(1);
                        }
                        else {
                            Debug.Log("TRASH TYPE DOES NOT MATCH");
                            MgGarbageContext.Instance.GetLocalPlayer().CmdSubtractScore(1);
                        }
                        Debug.Log("DESTROYING TRASH");
                        Destroy(gameObject);
                    }
                }
            }
        }

        isDragging = false;
    }
}