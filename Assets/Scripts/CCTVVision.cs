
using UnityEngine;
using System.Collections;

public class CCTVVision : MonoBehaviour
{
    public enum CCTVFacing
    {
        Left,
        Right
    }

    [Header("Pengaturan CCTV")]
    public CCTVFacing facing = CCTVFacing.Right;
    public float viewDistance = 6f;
    [Range(1f, 180f)]
    public float viewAngle = 90f;
    public LayerMask wallLayer;

    [Header("Referensi")]
    public VisionCone visionCone;
    public Transform player;
    public Transform basementStartPoint;

    private bool playerCaught = false;
    private Rigidbody2D playerRb;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null)
            playerRb = player.GetComponent<Rigidbody2D>();

        if (visionCone == null)
            visionCone = GetComponentInChildren<VisionCone>();

        if (visionCone == null)
            Debug.LogError("VisionCone CCTV belum ditemukan!");

        if (player == null)
            Debug.LogError("Player tidak ditemukan!");

        if (basementStartPoint == null)
            Debug.LogError("Basement Start Point belum dihubungkan!");

        UpdateCone();
    }

    void Update()
    {
        UpdateCone();

        if (player == null || visionCone == null)
            return;

        if (!playerCaught && CanSeePlayer())
        {
            playerCaught = true;
            visionCone.SetDetected();

            Debug.Log("PLAYER TERTANGKAP CCTV!");

            StartCoroutine(RespawnPlayer());
        }
        else if (!playerCaught)
        {
            visionCone.SetNormal();
        }
    }

    void UpdateCone()
    {
        if (visionCone == null)
            return;

        Vector2 direction =
            facing == CCTVFacing.Left
            ? Vector2.left
            : Vector2.right;

        visionCone.SetDirection(
            direction,
            viewDistance,
            viewAngle
        );
    }

    bool CanSeePlayer()
    {
        Vector2 origin = transform.position;

        Vector2 directionToPlayer =
            (Vector2)player.position - origin;

        if (directionToPlayer.magnitude > viewDistance)
            return false;

        if (directionToPlayer.sqrMagnitude < 0.0001f)
            return true;

        Vector2 facingDirection =
            facing == CCTVFacing.Left
            ? Vector2.left
            : Vector2.right;

        float angle = Vector2.Angle(
            facingDirection,
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

    IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(0.5f);

        if (player == null || basementStartPoint == null)
            yield break;

        player.position = basementStartPoint.position;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        Debug.Log("Player dikembalikan ke awal basement!");

        yield return new WaitForSeconds(0.5f);

        playerCaught = false;
        visionCone.SetNormal();
    }
}
