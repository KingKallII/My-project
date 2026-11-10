using UnityEngine;
using System.Collections;

public class CCTVVision : MonoBehaviour
{
    public enum CCTVFacing
    {
        Left,
        Right,
        Down,
        Up
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

    void Awake()
    {
        // Jika pada objek CCTV ini (atau parent/child-nya) sudah dipasang CCTVController (yang bisa swipe),
        // nonaktifkan CCTVVision statis ini agar tidak menimpa arah VisionCone menjadi selalu menghadap ke kanan!
        if (GetComponent<CCTVController>() != null ||
            GetComponentInChildren<CCTVController>() != null ||
            GetComponentInParent<CCTVController>() != null)
        {
            enabled = false;
            return;
        }
    }

    void Start()
    {
        if (GetComponent<CCTVController>() != null ||
            GetComponentInChildren<CCTVController>() != null ||
            GetComponentInParent<CCTVController>() != null)
        {
            enabled = false;
            return;
        }

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

    Vector2 GetFacingVector()
    {
        switch (facing)
        {
            case CCTVFacing.Left:
                return Vector2.left;
            case CCTVFacing.Down:
                return Vector2.down;
            case CCTVFacing.Up:
                return Vector2.up;
            case CCTVFacing.Right:
            default:
                return Vector2.right;
        }
    }

    void UpdateCone()
    {
        if (visionCone == null)
            return;

        visionCone.SetDirection(
            GetFacingVector(),
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

        float angle = Vector2.Angle(
            GetFacingVector(),
            directionToPlayer
        );

        if (angle > viewAngle / 2f)
            return false;

        if (wallLayer.value != 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(
                origin,
                player.position,
                wallLayer
            );

            if (hit.collider != null)
                return false;
        }

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