
using UnityEngine;
using UnityEngine.InputSystem;

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
        if (isLocked)
        {
            if (puzzleManager != null)
            {
                puzzleManager.OpenPuzzle();
            }
            else if (puzzlePanel != null)
            {
                puzzlePanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning(
                    "PuzzleManager dan PuzzlePanel belum dihubungkan!"
                );
            }

            return;
        }

        // Pintu sudah terbuka: pindahkan pemain ke lorong.
        TeleportToCorridor();
    }

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

        // Izinkan interaksi kembali setelah proses fade selesai.
        StartCoroutine(ResetTeleportState());
    }

    System.Collections.IEnumerator ResetTeleportState()
    {
        yield return new WaitForSeconds(
            screenFadeController.fadeDuration * 2f + 0.2f
        );

        isTeleporting = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Tekan E untuk berinteraksi dengan pintu.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
