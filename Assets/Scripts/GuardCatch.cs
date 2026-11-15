using UnityEngine;
using System.Collections;

public class GuardCatch : MonoBehaviour
{
    private bool playerCaught = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerCaught)
            return;

        if (other.CompareTag("Player"))
        {
            playerCaught = true;

            Debug.Log("PLAYER TERTANGKAP!");

            // Hentikan Player
            PlayerMovement playerMovement =
                other.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.StopMovement();
            }

            // Hentikan Guard
            GuardPatrol guardPatrol =
                GetComponent<GuardPatrol>();

            if (guardPatrol != null)
            {
                guardPatrol.SetCaught();
            }

            // Mulai proses respawn
            StartCoroutine(RespawnAfterCaught(other.gameObject));
        }
    }

    IEnumerator RespawnAfterCaught(GameObject player)
    {
        // Tunggu 1 detik
        yield return new WaitForSeconds(1f);

        // Respawn ke checkpoint
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer(player);
        }

        Debug.Log("PLAYER KEMBALI KE CHECKPOINT!");

        // Aktifkan kembali movement Player
        PlayerMovement playerMovement =
            player.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.ResumeMovement();
        }

        // Guard kembali patroli
        GuardPatrol guardPatrol =
            GetComponent<GuardPatrol>();

        if (guardPatrol != null)
        {
            guardPatrol.ResetGuard();
        }

        // Izinkan Guard menangkap Player lagi
        playerCaught = false;
    }
}