
using UnityEngine;

public class GuardPatrol : MonoBehaviour
{
    [Header("Titik Patroli")]
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;

    [Header("Kecepatan")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3.5f;

    [Header("Deteksi Dinding")]
    public LayerMask wallLayer;
    public float collisionRadius = 0.35f;

    [Header("Sprite Berjalan (Fallback)")]
    public Sprite idleSprite;
    public Sprite walkingAwaySprite;
    public Sprite walkingBackSprite;
    public Sprite walkingProfile1;
    public Sprite walkingProfile2;
    public Sprite profileHoodieSprite;
    public Sprite overheadWalkingSprite;
    public Sprite sideViewSprite;
    public Sprite topDownWalkingSprite;
    public Sprite walkingStickerSprite;

    [Header("Sprite Idle Setiap Arah")]
    public Sprite idleFrontSprite;
    public Sprite idleBackSprite;
    public Sprite idleLeftSprite;
    public Sprite idleRightSprite;

    [Header("Animasi Berjalan ke Belakang")]
    public Sprite walkingAwayFrame1;
    public Sprite walkingAwayFrame2;

    [Header("Animasi Berjalan ke Depan")]
    public Sprite walkingFrontFrame1;
    public Sprite walkingFrontFrame2;

    [Header("Animasi Berjalan ke Kiri")]
    public Sprite walkingLeftFrame1;
    public Sprite walkingLeftFrame2;

    [Header("Animasi Berjalan ke Kanan")]
    public Sprite walkingRightFrame1;
    public Sprite walkingRightFrame2;

    [Header("Pengaturan Animasi")]
    [Min(0.01f)]
    public float walkAnimationSpeed = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    private bool isChasing;
    private bool isCaught;

    private Transform[] patrolPoints;
    private int currentPointIndex;

    private enum FacingDirection
    {
        Front,
        Back,
        Left,
        Right
    }

    private FacingDirection lastDirection = FacingDirection.Front;
    private FacingDirection? animatedDirection = null;

    private float walkAnimationTimer;
    private int currentWalkFrame;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        // Urutan patroli: C -> B -> A -> B -> C
        patrolPoints = new Transform[]
        {
            pointC,
            pointB,
            pointA,
            pointB
        };

        currentPointIndex = 0;
        SetIdleSprite();
    }

    void Update()
    {
        if (isCaught || rb == null)
            return;

        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
            // Tentukan arah dominan gerakan penjaga.
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                lastDirection = velocity.x < 0
                    ? FacingDirection.Left
                    : FacingDirection.Right;
            }
            else
            {
                lastDirection = velocity.y > 0
                    ? FacingDirection.Back
                    : FacingDirection.Front;
            }

            UpdateWalkingSprite();
        }
        else
        {
            ResetAnimation();
            SetIdleSprite();
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (isCaught)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isChasing)
            ChasePlayer();
        else
            Patrol();
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPointIndex];

        if (targetPoint == null)
            return;

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = targetPoint.position;
        Vector2 difference = targetPosition - currentPosition;

        if (difference.sqrMagnitude < 0.0025f)
        {
            currentPointIndex++;

            if (currentPointIndex >= patrolPoints.Length)
                currentPointIndex = 0;

            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = difference.normalized;
        float stepDistance = moveSpeed * Time.fixedDeltaTime;

        if (IsWallBlocking(direction, stepDistance))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 newPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            stepDistance
        );

        rb.MovePosition(newPosition);
        rb.linearVelocity = direction * moveSpeed;

        if (difference.sqrMagnitude <= stepDistance * stepDistance)
        {
            currentPointIndex++;

            if (currentPointIndex >= patrolPoints.Length)
                currentPointIndex = 0;
        }
    }

    void ChasePlayer()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 playerPosition = player.position;
        Vector2 difference = playerPosition - currentPosition;

        if (difference.sqrMagnitude < 0.0025f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = difference.normalized;
        float stepDistance = chaseSpeed * Time.fixedDeltaTime;

        if (IsWallBlocking(direction, stepDistance))
        {
            rb.linearVelocity = Vector2.zero;
            StopChase();
            return;
        }

        Vector2 newPosition = Vector2.MoveTowards(
            currentPosition,
            playerPosition,
            stepDistance
        );

        rb.MovePosition(newPosition);
        rb.linearVelocity = direction * chaseSpeed;
    }

    bool IsWallBlocking(Vector2 direction, float distance)
    {
        if (distance <= 0.01f)
            return false;

        RaycastHit2D hit = Physics2D.CircleCast(
            rb.position,
            collisionRadius,
            direction,
            distance,
            wallLayer
        );

        return hit.collider != null;
    }

    void UpdateWalkingSprite()
    {
        if (spriteRenderer == null)
            return;

        Sprite frame1 = null;
        Sprite frame2 = null;
        Sprite fallbackSprite = null;

        spriteRenderer.flipX = false;

        switch (lastDirection)
        {
            case FacingDirection.Back:
                frame1 = walkingAwayFrame1;
                frame2 = walkingAwayFrame2;
                fallbackSprite = walkingAwaySprite != null
                    ? walkingAwaySprite
                    : (walkingBackSprite != null
                        ? walkingBackSprite
                        : overheadWalkingSprite);
                break;

            case FacingDirection.Front:
                frame1 = walkingFrontFrame1;
                frame2 = walkingFrontFrame2;
                fallbackSprite = topDownWalkingSprite != null
                    ? topDownWalkingSprite
                    : idleSprite;
                break;

            case FacingDirection.Left:
                frame1 = walkingLeftFrame1;
                frame2 = walkingLeftFrame2;
                fallbackSprite = walkingProfile2 != null
                    ? walkingProfile2
                    : (sideViewSprite != null
                        ? sideViewSprite
                        : profileHoodieSprite);
                break;

            case FacingDirection.Right:
                frame1 = walkingRightFrame1;
                frame2 = walkingRightFrame2;
                fallbackSprite = walkingProfile1 != null
                    ? walkingProfile1
                    : (sideViewSprite != null
                        ? sideViewSprite
                        : profileHoodieSprite);
                break;
        }

        if (frame1 != null && frame2 != null)
        {
            AnimateFrames(frame1, frame2);
        }
        else
        {
            ResetAnimation();

            if (fallbackSprite != null)
                spriteRenderer.sprite = fallbackSprite;
        }
    }

    void AnimateFrames(Sprite frame1, Sprite frame2)
    {
        if (animatedDirection != lastDirection)
        {
            animatedDirection = lastDirection;
            walkAnimationTimer = 0f;
            currentWalkFrame = 0;
            spriteRenderer.sprite = frame1;
            return;
        }

        walkAnimationTimer += Time.deltaTime;

        if (walkAnimationTimer >= walkAnimationSpeed)
        {
            walkAnimationTimer = 0f;
            currentWalkFrame = 1 - currentWalkFrame;

            spriteRenderer.sprite = currentWalkFrame == 0
                ? frame1
                : frame2;
        }
    }

    void ResetAnimation()
    {
        walkAnimationTimer = 0f;
        currentWalkFrame = 0;
        animatedDirection = null;
    }

    void SetIdleSprite()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = false;

        switch (lastDirection)
        {
            case FacingDirection.Back:
                spriteRenderer.sprite = idleBackSprite != null
                    ? idleBackSprite
                    : (walkingBackSprite != null
                        ? walkingBackSprite
                        : idleSprite);
                break;

            case FacingDirection.Front:
                spriteRenderer.sprite = idleFrontSprite != null
                    ? idleFrontSprite
                    : idleSprite;
                break;

            case FacingDirection.Left:
                spriteRenderer.sprite = idleLeftSprite != null
                    ? idleLeftSprite
                    : (profileHoodieSprite != null
                        ? profileHoodieSprite
                        : walkingProfile2);
                break;

            case FacingDirection.Right:
                spriteRenderer.sprite = idleRightSprite != null
                    ? idleRightSprite
                    : (profileHoodieSprite != null
                        ? profileHoodieSprite
                        : walkingProfile1);
                break;
        }
    }

    public void StartChase()
    {
        if (isCaught)
            return;

        isChasing = true;
        Debug.Log("GUARD MULAI MENGEJAR!");
    }

    public void StopChase()
    {
        isChasing = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log("GUARD BERHENTI MENGEJAR DAN KEMBALI PATROLI!");
    }

    public void SetCaught()
    {
        isCaught = true;
        isChasing = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        ResetAnimation();
        SetIdleSprite();
    }

    public void ResetGuard()
    {
        isCaught = false;
        isChasing = false;
        currentPointIndex = 0;

        if (rb != null)
        {
            rb.position = pointC != null
                ? (Vector2)pointC.position
                : rb.position;

            rb.linearVelocity = Vector2.zero;
        }

        if (pointC != null)
            transform.position = pointC.position;

        ResetAnimation();
        SetIdleSprite();

        Debug.Log("GUARD KEMBALI PATROLI!");
    }

    public bool IsChasing()
    {
        return isChasing;
    }

    public bool IsCaught()
    {
        return isCaught;
    }
}
