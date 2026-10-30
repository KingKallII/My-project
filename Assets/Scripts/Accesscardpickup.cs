
using UnityEngine;
using UnityEngine.InputSystem;

public class AccessCardPickup : MonoBehaviour
{
    private bool playerNearby = false;
    private PlayerInventory playerInventory;

    private void Update()
    {
        if (playerNearby &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUpCard();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            playerInventory =
                other.GetComponent<PlayerInventory>();

            if (playerInventory == null)
            {
                playerInventory =
                    other.GetComponentInParent<PlayerInventory>();
            }

            Debug.Log("Tekan E untuk mengambil Access Card");

            if (playerInventory == null)
            {
                Debug.LogWarning(
                    "PlayerInventory tidak ditemukan pada Player!"
                );
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            playerInventory = null;

            Debug.Log("Player menjauh dari Access Card");
        }
    }

    private void PickUpCard()
    {
        if (playerInventory == null)
        {
            Debug.LogError(
                "Access Card gagal diambil: PlayerInventory tidak ditemukan!"
            );
            return;
        }

        // Tambahkan kartu ke inventory pemain.
        playerInventory.AddAccessCard();

        Debug.Log("Access Card berhasil diambil!");

        // Hapus objek kartu dari scene.
        Destroy(gameObject);
    }
}
