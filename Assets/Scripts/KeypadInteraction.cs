using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadInteraction : MonoBehaviour
{
    private bool playerNearby = false;
    private PlayerInventory playerInventory;
    private bool isInteracting = false;
    private bool isUnlocked = false; // Penanda jika puzzle sudah berhasil diselesaikan

    [Header("Referensi Puzzle")]
    public PuzzleManager puzzleManager;

    [Header("Pengaturan Notifikasi Kepala Player")]
    public string teksInteraksi = "Buka Kunci";
    public string pesanTanpaKartu = "Perlu Access Card";
    public float durasiPeringatan = 2f;

    private void Update()
    {
        // Hentikan interaksi jika sudah berhasil diselesaikan (isUnlocked == true)
        if (!playerNearby || isInteracting || isUnlocked)
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
        if (!IsPlayer(other))
            return;

        playerNearby = true;

        // Jika sudah berhasil dibuka, jangan tampilkan teks "Buka Kunci" lagi
        if (isUnlocked)
            return;

        playerInventory = other.GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
            playerInventory = other.GetComponentInParent<PlayerInventory>();
        }

        if (!isInteracting && InteractionIndicator.instance != null)
        {
            InteractionIndicator.instance.ShowPrompt(teksInteraksi, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerNearby = false;
        playerInventory = null;

        if (InteractionIndicator.instance != null)
        {
            InteractionIndicator.instance.HidePrompt();
        }
    }

    private void CheckAccessCard()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerInventory tidak ditemukan!");
            return;
        }

        if (!playerInventory.HasAccessCard)
        {
            if (InteractionIndicator.instance != null)
            {
                InteractionIndicator.instance.ShowAlert(
                    pesanTanpaKartu,
                    durasiPeringatan,
                    () => (playerNearby && !isUnlocked) ? teksInteraksi : ""
                );
            }
            return;
        }

        if (puzzleManager == null)
        {
            Debug.LogError("PuzzleManager belum dihubungkan!");
            return;
        }

        if (InteractionIndicator.instance != null)
        {
            InteractionIndicator.instance.HidePrompt();
        }

        isInteracting = true;
        puzzleManager.OpenPuzzle();
    }

    // Dipanggil ketika puzzle ditutup TANPA berhasil diselesaikan (batal/Close)
    public void ResetInteraction()
    {
        isInteracting = false;

        if (playerNearby && !isUnlocked && InteractionIndicator.instance != null)
        {
            InteractionIndicator.instance.ShowPrompt(teksInteraksi, true);
        }
    }

    // Dipanggil oleh PuzzleManager ketika puzzle BERHASIL diselesaikan
    public void CompleteInteraction()
    {
        isUnlocked = true;
        isInteracting = false;

        if (InteractionIndicator.instance != null)
        {
            InteractionIndicator.instance.HidePrompt();
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player") ||
               (other.attachedRigidbody != null &&
                other.attachedRigidbody.CompareTag("Player"));
    }
}