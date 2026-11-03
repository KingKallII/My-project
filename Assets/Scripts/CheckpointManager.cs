
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("Checkpoint Awal Basement")]
    public Transform basementStartPoint;

    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (basementStartPoint != null)
        {
            checkpointPosition = basementStartPoint.position;
            hasCheckpoint = true;

            Debug.Log(
                "BASEMENT START POINT TERHUBUNG: "
                + checkpointPosition
            );
        }
        else
        {
            Debug.LogError(
                "Basement Start Point belum dihubungkan!"
            );
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;

        Debug.Log(
            "CHECKPOINT TERSIMPAN! Posisi: "
            + checkpointPosition
        );
    }

    public void RespawnPlayer(GameObject player)
    {
        if (!hasCheckpoint)
        {
            Debug.LogWarning("BELUM ADA CHECKPOINT!");
            return;
        }

        if (player == null)
            return;

        Rigidbody2D playerRb =
            player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.position = new Vector2(
                checkpointPosition.x,
                checkpointPosition.y
            );
        }
        else
        {
            player.transform.position = checkpointPosition;
        }

        Debug.Log(
            "PLAYER RESPAWN DI CHECKPOINT! Posisi: "
            + checkpointPosition
        );
    }
}
