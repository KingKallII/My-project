using UnityEngine;
using UnityEngine.InputSystem;

public class SafeItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID = "exit_key";
    public string itemName = "Exit Key";

    [Header("Tampilan Khusus di Hotbar")]
    public Sprite itemIcon;
    public bool gunakanUkuranKhusus = true;
    public Vector2 ukuranDiHotbar = new Vector2(120f, 120f);

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

            bool success = playerInventory.AddItem(
                itemID,
                itemName,
                itemIcon,
                sizeToUse
            );

            if (success)
            {
                gameObject.SetActive(false);
                Debug.Log(itemName + " berhasil diambil!");
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