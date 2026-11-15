
using UnityEngine;

public class CCTVController : MonoBehaviour
{
    [Header("Pengaturan Penglihatan")]
    public float viewDistance = 7f;

    [Range(1f, 180f)]
    public float viewAngle = 45f;

    public LayerMask wallLayer;

    [Header("Pengaturan Gerakan CCTV")]
    public Vector2 initialDirection = Vector2.right;
    public float sweepAngle = 90f;
    public float sweepSpeed = 45f;

    [Header("Referensi")]
    public VisionCone visionCone;
    public CCTVAlarmEffect alarmEffect;

    private Transform player;
    private Vector2 currentDirection;
    private bool playerCaught = false;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError(
                "CCTV: Player dengan Tag 'Player' tidak ditemukan!"
            );
        }

        if (visionCone == null)
        {
            visionCone = GetComponentInChildren<VisionCone>();
        }

        if (visionCone == null)
        {
            Debug.LogError("CCTV: VisionCone tidak ditemukan!");
        }

        if (alarmEffect == null)
        {
            Debug.LogError(
                "CCTV: CCTVAlarmEffect belum dihubungkan!"
            );
        }

        currentDirection = initialDirection.normalized;

        if (currentDirection.sqrMagnitude < 0.0001f)
        {
            currentDirection = Vector2.right;
        }

        UpdateVisionCone();
    }

    void Update()
    {
        if (visionCone == null)
            return;

        UpdateSweepDirection();
        UpdateVisionCone();

        bool canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            visionCone.SetDetected();

            if (!playerCaught)
            {
                playerCaught = true;

                Debug.Log("PLAYER TERTANGKAP CCTV!");

                if (alarmEffect != null)
                {
                    alarmEffect.PlayAlarm();
                }
            }
        }
        else
        {
            visionCone.SetNormal();

            playerCaught = false;
        }
    }

    void UpdateSweepDirection()
    {
        float offset = Mathf.PingPong(
            Time.time * sweepSpeed,
            sweepAngle
        ) - sweepAngle / 2f;

        currentDirection =
            Quaternion.Euler(0f, 0f, offset)
            * initialDirection.normalized;
    }

    void UpdateVisionCone()
    {
        if (visionCone == null)
            return;

        visionCone.SetDirection(
            currentDirection,
            viewDistance,
            viewAngle
        );
    }

    bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector2 origin = transform.position;

        Vector2 directionToPlayer =
            (Vector2)player.position - origin;

        if (directionToPlayer.magnitude > viewDistance)
            return false;

        if (directionToPlayer.sqrMagnitude < 0.0001f)
            return true;

        float angle = Vector2.Angle(
            currentDirection,
            directionToPlayer
        );

        if (angle > viewAngle / 2f)
            return false;

        RaycastHit2D hit = Physics2D.Linecast(
            origin,
            player.position,
            wallLayer
        );

        if (hit.collider != null)
            return false;

        return true;
    }
}
