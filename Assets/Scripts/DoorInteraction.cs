using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Referensi Pintu")]
    public GameObject door;
    public GameObject puzzlePanel;
    public PuzzleManager puzzleManager;

    [Header("Referensi Teleportasi")]
    public Transform player;
    public Transform lorongSpawnPoint;
    public ScreenFadeController screenFadeController;

    [Header("Status Pintu")]
    public bool isLocked = true;

    [Header("Status Access Card")]
    public bool hasScannedCard = false;

    [Header("Pengaturan Notifikasi Kepala Player")]
    public string teksInteraksi = "Buka Pintu";
    public string pesanTerkunci = "Pintu Terkunci!";
    public float durasiPeringatan = 2f;

    private bool playerNearby = false;
    private bool isTeleporting = false;

    void Start()
    {
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);
    }

    void Update()
    {
        if (!playerNearby || isTeleporting)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        // Jika pintu masih terkunci
        if (isLocked)
        {
            // Periksa apakah kartu sudah di-scan
            if (!hasScannedCard)
            {
                Debug.Log("AKSES DITOLAK! SILAKAN SCAN ACCESS CARD.");

                // Tampilkan peringatan tanpa ikon E, lalu kembali ke "Buka Pintu" jika masih di dekat pintu
                if (InteractionIndicator.instance != null)
                {
                    InteractionIndicator.instance.ShowAlert(
                        pesanTerkunci,
                        durasiPeringatan,
                        () => playerNearby ? teksInteraksi : ""
                    );
                }

                return;
            }

            // Buka puzzle PIN jika kartu sudah di-scan
            if (puzzleManager != null)
            {
                if (InteractionIndicator.instance != null)
                    InteractionIndicator.instance.HidePrompt();

                puzzleManager.OpenPuzzle();
            }
            else
            {
                Debug.LogWarning("PuzzleManager belum dihubungkan!");
            }

            return;
        }

        // Jika pintu sudah terbuka, sembunyikan prompt lalu masuk ke lorong
        if (InteractionIndicator.instance != null)
            InteractionIndicator.instance.HidePrompt();

        TeleportToCorridor();
    }

    // Dipanggil oleh AccessCardScanner
    public void GrantAccess()
    {
        hasScannedCard = true;
        Debug.Log("AKSES PINTU DIBERIKAN!");
    }

    // Dipanggil oleh PuzzleManager
    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("KUNCI PINTU BERHASIL DIBUKA!");
    }

    void TeleportToCorridor()
    {
        if (player == null ||
            lorongSpawnPoint == null ||
            screenFadeController == null)
        {
            Debug.LogWarning(
                "Referensi Player, LorongSpawnPoint, atau " +
                "ScreenFadeController belum lengkap!"
            );

            return;
        }

        isTeleporting = true;

        screenFadeController.FadeAndTeleport(
            player,
            lorongSpawnPoint
        );

        StartCoroutine(ResetTeleportState());
    }

    IEnumerator ResetTeleportState()
    {
        yield return new WaitForSeconds(
            screenFadeController.fadeDuration * 2f + 0.2f
        );

        isTeleporting = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = true;

            if (!isTeleporting && InteractionIndicator.instance != null)
            {
                InteractionIndicator.instance.ShowPrompt(teksInteraksi, true);
            }

            Debug.Log("Tekan E untuk berinteraksi dengan pintu.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = false;

            if (InteractionIndicator.instance != null)
            {
                InteractionIndicator.instance.HidePrompt();
            }
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