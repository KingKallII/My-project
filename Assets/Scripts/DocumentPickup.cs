using UnityEngine;
using UnityEngine.InputSystem;

public class DocumentPickup : MonoBehaviour
{
    [Header("Document Settings")]
    public string itemID = "warehouse_note";
    public string itemName = "Catatan Teka-Teki Gudang";

    [Header("Item Images")]
    public Sprite itemIcon;
    public Sprite documentPreview;

    [Header("Ukuran Khusus di Hotbar")]
    public bool gunakanUkuranKhusus = true;
    public Vector2 ukuranDiHotbar = new Vector2(110f, 130f);

    [Header("Player Inventory")]
    public PlayerInventory playerInventory;

    private bool playerNearby;

    private void Update()
    {
        if (!playerNearby)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (playerInventory == null)
            {
                Debug.LogWarning("PlayerInventory belum dihubungkan!");
                return;
            }

            Vector2? sizeToUse = gunakanUkuranKhusus ? ukuranDiHotbar : (Vector2?)null;

            bool success = playerInventory.AddDocumentItem(
                itemID,
                itemName,
                itemIcon,
                documentPreview,
                sizeToUse
            );

            if (success)
            {
                gameObject.SetActive(false);
                Debug.Log("Dokumen berhasil diambil!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = true;
            if (playerInventory == null)
                playerInventory = other.GetComponentInParent<PlayerInventory>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
            playerNearby = false;
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player") ||
               (other.attachedRigidbody != null &&
                other.attachedRigidbody.CompareTag("Player"));
    }
}