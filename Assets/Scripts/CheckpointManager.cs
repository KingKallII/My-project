using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        checkpointPosition = transform.position;
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;

        Debug.Log(
            "CHECKPOINT TERSIMPAN! Posisi: " + checkpointPosition
        );
    }

    public void RespawnPlayer(GameObject player)
    {
        if (!hasCheckpoint)
        {
            Debug.LogWarning("BELUM ADA CHECKPOINT!");
            return;
        }

        Rigidbody2D playerRb =
            player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            // Hentikan gerakan Player
            playerRb.linearVelocity = Vector2.zero;

            // Teleport menggunakan Rigidbody2D
            playerRb.position =
                new Vector2(
                    checkpointPosition.x,
                    checkpointPosition.y
                );
        }
        else
        {
            // Cadangan kalau Rigidbody2D tidak ditemukan
            player.transform.position = checkpointPosition;
        }

        Debug.Log(
            "PLAYER RESPAWN DI CHECKPOINT! Posisi: "
            + checkpointPosition
        );
    }
}