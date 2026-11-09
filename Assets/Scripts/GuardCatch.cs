using UnityEngine;
using System.Collections;

public class GuardCatch : MonoBehaviour
{
    [Header("Referensi Minigame Kertas Gunting Batu")]
    [Tooltip("Masukkan objek yang memiliki skrip RPSMinigameManager (atau biarkan kosong agar dicari otomatis).")]
    public RPSMinigameManager minigameManager;

    private bool playerCaught = false;
    private bool isGuardKnockedOut = false;
    private GuardPatrol guardPatrol;

    void Start()
    {
        guardPatrol = GetComponent<GuardPatrol>();

        if (minigameManager == null)
        {
            minigameManager = FindFirstObjectByType<RPSMinigameManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCatchPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCatchPlayer(other);
    }

    private void TryCatchPlayer(Collider2D other)
    {
        if ( guardPatrol == null )
            guardPatrol = GetComponent<GuardPatrol>();

        // Jangan tangkap Player jika Guard sedang pingsan atau sedang dalam minigame
        if (playerCaught || isGuardKnockedOut || (guardPatrol != null && guardPatrol.IsKnockedOut()))
            return;

        if (other.CompareTag("Player"))
        {
            playerCaught = true;

            Debug.Log("PLAYER TERTANGKAP! MEMULAI MINIGAME KERTAS GUNTING BATU!");

            // Hentikan pergerakan Player
            PlayerMovement playerMovement =
                other.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.StopMovement();
            }

            // Hentikan pergerakan Guard
            if (guardPatrol != null)
            {
                guardPatrol.SetCaught();
            }

            // Cari RPSMinigameManager jika belum terhubung
            if (minigameManager == null)
            {
                minigameManager = RPSMinigameManager.Instance != null
                    ? RPSMinigameManager.Instance
                    : FindFirstObjectByType<RPSMinigameManager>();
            }

            // Jika Minigame tersedia, buka Minigame Kertas Gunting Batu
            if (minigameManager != null)
            {
                minigameManager.StartMinigame(other.gameObject, this, guardPatrol);
            }
            else
            {
                // Fallback jika Minigame belum dipasang di scene: langsung respawn ke checkpoint
                Debug.LogWarning("RPSMinigameManager belum ada di scene! Melakukan respawn biasa.");
                StartCoroutine(RespawnAfterCaughtFallback(other.gameObject));
            }
        }
    }

    /// <summary>
    /// Dipanggil oleh RPSMinigameManager ketika Player MENANG minigame.
    /// Guard pingsan sementara sesuai stunDuration di GuardPatrol.
    /// </summary>
    public void OnMinigamePlayerWon()
    {
        isGuardKnockedOut = true;
        playerCaught = false;
    }

    /// <summary>
    /// Dipanggil oleh GuardPatrol ketika waktu pingsan (stunDuration) habis dan Guard sadar kembali.
    /// </summary>
    public void OnGuardWokeUp()
    {
        isGuardKnockedOut = false;
        playerCaught = false;
    }

    /// <summary>
    /// Dipanggil oleh RPSMinigameManager ketika Player KALAH minigame.
    /// </summary>
    public void OnMinigamePlayerLost()
    {
        isGuardKnockedOut = false;
        playerCaught = false;
    }

    IEnumerator RespawnAfterCaughtFallback(GameObject player)
    {
        yield return new WaitForSeconds(1f);

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer(player);
        }

        PlayerMovement playerMovement =
            player.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.ResumeMovement();
        }

        if (guardPatrol != null)
        {
            guardPatrol.ResetGuard();
        }

        playerCaught = false;
    }
}