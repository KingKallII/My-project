
using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadInteraction : MonoBehaviour
{
    private bool playerNearby = false;
    private PlayerInventory playerInventory;

    [Header("Referensi Puzzle")]
    public PuzzleManager puzzleManager;

    private void Update()
    {
        if (!playerNearby)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            CheckAccessCard();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;

        playerInventory = other.GetComponent<PlayerInventory>();

        if (playerInventory == null)
        {
            playerInventory =
                other.GetComponentInParent<PlayerInventory>();
        }

        Debug.Log("Tekan E untuk berinteraksi dengan Keypad");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;
        playerInventory = null;

        Debug.Log("Player menjauh dari Keypad");
    }

    private void CheckAccessCard()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning(
                "PlayerInventory tidak ditemukan pada Player!"
            );
            return;
        }

        if (!playerInventory.hasAccessCard)
        {
            Debug.Log("Access Card diperlukan untuk menggunakan Keypad!");
            return;
        }

        if (puzzleManager == null)
        {
            Debug.LogError(
                "PuzzleManager belum dihubungkan pada Keypad!"
            );
            return;
        }

        Debug.Log("Access Card terdeteksi. Membuka panel puzzle.");

        puzzleManager.OpenPuzzle();
    }
}
