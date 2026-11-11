
using UnityEngine;
using UnityEngine.InputSystem;

public class SafeItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID = "exit_key";
    public string itemName = "Exit Key";
    public Sprite itemIcon;

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

            bool success = playerInventory.AddItem(
                itemID,
                itemName,
                itemIcon
            );

            if (success)
            {
                gameObject.SetActive(false);
                Debug.Log("Exit Key berhasil diambil!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
            playerNearby = true;
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
