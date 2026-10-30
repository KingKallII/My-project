
using UnityEngine;

public class GuardVision : MonoBehaviour
{
    [Header("Pengaturan Penglihatan")]
    [Min(0.1f)]
    public float viewDistance = 5f;

    [Range(1f, 360f)]
    public float viewAngle = 90f;

    public LayerMask wallLayer;

    [Header("Referensi")]
    public VisionCone visionCone;

    [Header("Arah Awal Guard")]
    public Vector2 initialDirection = Vector2.down;

    [Header("Pengaturan Arah")]
    public float directionThreshold = 0.01f;

    private Transform player;
    private Vector2 currentDirection;
    private Vector2 previousPosition;

    void Start()
    {
        // Cari Player berdasarkan Tag.
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "GuardVision: Player dengan Tag 'Player' tidak ditemukan!"
            );
        }

        // Cari VisionCone jika belum diisi di Inspector.
        if (visionCone == null)
        {
            visionCone = GetComponentInChildren<VisionCone>();
        }

        if (visionCone == null)
        {
            Debug.LogError(
                "GuardVision: VisionCone tidak ditemukan! " +
                "Masukkan objek VisionCone ke Inspector."
            );
        }

        // Atur arah awal.
        currentDirection = initialDirection.normalized;

        if (currentDirection.sqrMagnitude < 0.0001f)
        {
            currentDirection = Vector2.down;
        }

        previousPosition = transform.position;

        UpdateConeDirection();
    }

    void Update()
    {
        UpdateMovementDirection();
        UpdateConeDirection();

        if (visionCone != null)
        {
            if (CanSeePlayer())
            {
                visionCone.SetDetected();
            }
            else
            {
                visionCone.SetNormal();
            }
        }
    }

    void UpdateMovementDirection()
    {
        Vector2 currentPosition = transform.position;

        Vector2 movement =
            currentPosition - previousPosition;

        // Arah cone mengikuti gerak aktual guard.
        if (movement.magnitude > directionThreshold)
        {
            currentDirection = movement.normalized;
        }

        previousPosition = currentPosition;
    }

    void UpdateConeDirection()
    {
        if (visionCone == null)
            return;

        // Kirim nilai terbaru ke bentuk visual cone.
        visionCone.SetDirection(
            currentDirection,
            viewDistance,
            viewAngle
        );
    }

    public bool PlayerIsVisible()
    {
        return CanSeePlayer();
    }

    bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector2 origin = transform.position;

        Vector2 directionToPlayer =
            (Vector2)player.position - origin;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        // Periksa jarak penglihatan.
        if (distanceToPlayer > viewDistance)
            return false;

        if (directionToPlayer.sqrMagnitude < 0.0001f)
            return true;

        // Periksa sudut penglihatan.
        float angle = Vector2.Angle(
            currentDirection,
            directionToPlayer
        );

        if (angle > viewAngle / 2f)
            return false;

        // Periksa apakah pandangan terhalang dinding.
        RaycastHit2D hit = Physics2D.Linecast(
            origin,
            player.position,
            wallLayer
        );

        if (hit.collider != null)
            return false;

        return true;
    }

    // Dipanggil jika dibutuhkan oleh sistem lain.
    public Vector2 GetCurrentDirection()
    {
        return currentDirection;
    }
}
