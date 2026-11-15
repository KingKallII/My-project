using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadInteraction : MonoBehaviour
{
    private bool playerNearby = false;
    private PlayerInventory playerInventory;
    private bool isInteracting = false;

    [Header("Referensi Puzzle")]
    public PuzzleManager puzzleManager;

    private void Update()
    {
        if (!playerNearby || isInteracting)
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

        Debug.Log("Player mendekati Keypad.");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;
        playerInventory = null;
    }

    private void CheckAccessCard()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerInventory tidak ditemukan!");
            return;
        }

        // Periksa kartu
        if (!playerInventory.hasAccessCard)
        {
            Debug.Log("ACCESS DENIED! ACCESS CARD DIPERLUKAN.");
            return;
        }

        if (puzzleManager == null)
        {
            Debug.LogError("PuzzleManager belum dihubungkan!");
            return;
        }

        // Langsung buka puzzle setelah kartu terdeteksi
        isInteracting = true;

        Debug.Log("ACCESS CARD BERHASIL DIVERIFIKASI!");

        Debug.Log("MEMBUKA PUZZLE!");

        puzzleManager.OpenPuzzle();
    }

    // Dipanggil ketika puzzle ditutup
    public void ResetInteraction()
    {
        isInteracting = false;
    }
}