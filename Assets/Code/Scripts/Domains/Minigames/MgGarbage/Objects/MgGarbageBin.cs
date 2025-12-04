using Mirror;
using UnityEngine;

public class MgGarbageBin : NetworkBehaviour {
    [SerializeField]
    private MgGarbageTrashType acceptedTrashType;
    public MgGarbageTrashType AcceptedTrashType => acceptedTrashType;

    private bool isOpen = false;

    [SerializeField]
    private Sprite openBinSprite;
    [SerializeField]
    private Sprite closedBinSprite;

    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    void Awake() {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [ClientCallback]
    void Update() {
        if (boxCollider == null || spriteRenderer == null || Camera.main == null || !MgGarbageContext.Instance.HasStarted) {
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        bool pointerInside = boxCollider.OverlapPoint(mousePos2D);

        if (pointerInside) {
            if (!isOpen) {
                isOpen = true;
                Audiomanager.Instance.PlayGarbageBinOpenSound();
                spriteRenderer.sprite = openBinSprite;
            }
        }
        else if (isOpen) {
            isOpen = false;
            Audiomanager.Instance.PlayGarbageBinCloseSound();
            spriteRenderer.sprite = closedBinSprite;
        }
    }
}