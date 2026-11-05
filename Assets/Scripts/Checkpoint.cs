
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Posisi Spawn Player")]
    public Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnPoint == null)
            {
                Debug.LogError(
                    "SpawnPoint belum dihubungkan pada Checkpoint!"
                );
                return;
            }

            CheckpointManager.Instance.SetCheckpoint(
                spawnPoint.position
            );

            Debug.Log(
                "CHECKPOINT TERSIMPAN! Posisi: "
                + spawnPoint.position
            );
        }
    }
}
