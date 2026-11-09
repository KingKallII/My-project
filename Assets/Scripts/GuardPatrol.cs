using UnityEngine;
using System.Collections.Generic;

public class GuardPatrol : MonoBehaviour
{
    [Header("Titik Patroli")]
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;
    public Transform pointD;

    [Header("Kecepatan")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3.5f;

    [Header("Pengaturan Gerak 4 Arah (Kardinal)")]
    [Tooltip("Batas toleransi sejajar sumbu X/Y agar gerakan lurus dan tidak bergetar.")]
    public float axisAlignThreshold = 0.05f;

    [Tooltip("Selisih jarak minimum untuk berpindah sumbu gerak agar penjaga tidak zig-zag tiap frame.")]
    public float axisSwitchThreshold = 0.2f;

    [Tooltip("Jarak toleransi saat tiba di titik patroli agar penjaga langsung lanjut ke titik berikutnya.")]
    public float waypointReachThreshold = 0.2f;

    [Header("Deteksi Dinding & Invisible Barrier")]
    public LayerMask wallLayer;
    public float collisionRadius = 0.35f;

    [Tooltip("Jarak deteksi tambahan di depan penjaga agar tidak menembus Invisible Barrier.")]
    public float barrierSkinWidth = 0.08f;

    [Tooltip("Waktu jalan ekstra setelah melewati ujung tembok/barrier sebelum berbelok agar badan tidak nyangkut di sudut.")]
    public float cornerClearanceTime = 0.16f;

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

    [Header("Pengaturan Animasi Jalan")]
    [Min(0.01f)]
    public float walkAnimationSpeed = 0.15f;

    [Header("Pengaturan Pingsan & Barrier Guard (Saat Kalah Minigame)")]
    [Tooltip("Berapa detik Guard pingsan sebelum sadar dan kembali berpatroli (misal 6 detik).")]
    [Min(0.5f)]
    public float stunDuration = 6f;

    [Tooltip("Ukuran collider penghalang (barrier) di sekitar badan Guard saat pingsan agar tidak bisa diinjak Player.")]
    public Vector2 knockedOutBarrierSize = new Vector2(1.15f, 1.35f);

    [Tooltip("Offset posisi barrier di sekitar badan Guard saat pingsan.")]
    public Vector2 knockedOutBarrierOffset = new Vector2(0f, -0.05f);

    [Header("Animasi Guard Pingsan & Halo Pusing Looping")]
    [Tooltip("Masukkan gambar badan Guard yang tertunduk lemas/pingsan.")]
    public Sprite knockedOutSprite;

    [Tooltip("Opsional: Jika gambar badan Guard + burung pusing sudah digabung per frame.")]
    public Sprite[] knockedOutBodyFrames;

    [Tooltip("Masukkan frame-frame cincin burung/bintang pusing (Halo) ke daftar ini.")]
    public Sprite[] dizzyHaloFrames;

    [Tooltip("Posisi tinggi cincin burung pusing di atas kepala Guard.")]
    public Vector2 dizzyHaloOffset = new Vector2(0f, 1.15f);

    [Tooltip("Ukuran skala cincin burung pusing di atas kepala Guard.")]
    public float dizzyHaloScale = 1f;

    [Tooltip("Kecepatan pergantian frame Halo pusing (semakin kecil semakin cepat & mulus, misal 0.09 detik).")]
    [Range(0.03f, 0.5f)]
    public float knockedOutAnimSpeed = 0.09f;

    [Tooltip("Gerakan mengayun halus 60 FPS pada Halo agar animasi tidak terkesan patah-patah.")]
    public bool smoothHaloMotion = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private GuardDetection guardDetection;
    private GuardVision guardVision;
    private GuardCatch guardCatch;
    private Transform player;

    private bool isChasing;
    private bool isCaught;
    private bool isKnockedOut;

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

    private bool movingHorizontally = true;
    private Vector2 activeBypassDir = Vector2.zero;
    private Vector2 activeBlockedDir = Vector2.zero;
    private float bypassClearanceTimer = 0f;

    private float walkAnimationTimer;
    private int currentWalkFrame;

    // Variabel animasi pingsan, timer sadar, dan barrier anti-injak
    private float stunRemainingTimer;
    private float knockedOutFrameTimer;
    private float haloContinuousTime;
    private int currentKnockedOutBodyFrame;
    private int currentDizzyHaloFrame;

    private List<Sprite> validHaloFrames = new List<Sprite>();
    private List<Sprite> validBodyFrames = new List<Sprite>();

    private GameObject dizzyHaloObject;
    private SpriteRenderer dizzyHaloRenderer;

    private GameObject knockedOutBarrierObject;
    private BoxCollider2D knockedOutBarrierCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        guardDetection = GetComponent<GuardDetection>();
        guardVision = GetComponent<GuardVision>();
        guardCatch = GetComponent<GuardCatch>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        patrolPoints = new Transform[]
        {
            pointA,
            pointB,
            pointC,
            pointD
        };

        currentPointIndex = 0;

        RebuildValidAnimationFrames();
        SetIdleSprite();
    }

    /// <summary>
    /// Menyaring daftar frame Halo dan Body agar slot kosong atau potongan slice kecil yang tidak sengaja
    /// terbuat oleh Unity Automatic Slicer tidak menyebabkan jeda kosong (pause) saat looping.
    /// </summary>
    void RebuildValidAnimationFrames()
    {
        validHaloFrames.Clear();
        if (dizzyHaloFrames != null && dizzyHaloFrames.Length > 0)
        {
            float maxWidth = 0f;
            for (int i = 0; i < dizzyHaloFrames.Length; i++)
            {
                if (dizzyHaloFrames[i] != null)
                {
                    float w = dizzyHaloFrames[i].rect.width;
                    if (w > maxWidth)
                        maxWidth = w;
                }
            }

            for (int i = 0; i < dizzyHaloFrames.Length; i++)
            {
                Sprite s = dizzyHaloFrames[i];
                // Abaikan slot null atau potongan slice nyasar yang ukurannya kurang dari 45% ukuran Halo utama
                if (s != null && s.rect.width >= maxWidth * 0.45f)
                {
                    validHaloFrames.Add(s);
                }
            }
        }

        validBodyFrames.Clear();
        if (knockedOutBodyFrames != null && knockedOutBodyFrames.Length > 0)
        {
            for (int i = 0; i < knockedOutBodyFrames.Length; i++)
            {
                if (knockedOutBodyFrames[i] != null)
                    validBodyFrames.Add(knockedOutBodyFrames[i]);
            }
        }
    }

    void Update()
    {
        // Jika Guard sedang pingsan, jalankan animasi looping Halo & hitung mundur waktu pingsan
        if (isKnockedOut)
        {
            UpdateKnockedOutState();
            return;
        }

        if (isCaught || rb == null)
            return;

        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
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

        if (isKnockedOut || isCaught)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isChasing)
            ChasePlayer();
        else
            Patrol();
    }

    /// <summary>
    /// Memutar animasi Halo secara terus-menerus (looping mulus tanpa jeda)
    /// dan menghitung mundur waktu pingsan sampai Guard sadar kembali.
    /// </summary>
    void UpdateKnockedOutState()
    {
        // 1. Hitung mundur durasi pingsan
        stunRemainingTimer -= Time.deltaTime;
        if (stunRemainingTimer <= 0f)
        {
            WakeUpGuard();
            return;
        }

        haloContinuousTime += Time.deltaTime;
        knockedOutFrameTimer += Time.deltaTime;

        // 2. Pergantian frame sprite looping tanpa jeda (menggunakan pengurangan interval agar presisi)
        float interval = Mathf.Max(0.03f, knockedOutAnimSpeed);
        if (knockedOutFrameTimer >= interval)
        {
            knockedOutFrameTimer -= interval;

            if (spriteRenderer != null && validBodyFrames.Count > 0)
            {
                currentKnockedOutBodyFrame =
                    (currentKnockedOutBodyFrame + 1) % validBodyFrames.Count;
                spriteRenderer.sprite = validBodyFrames[currentKnockedOutBodyFrame];
            }

            if (dizzyHaloRenderer != null && validHaloFrames.Count > 0)
            {
                currentDizzyHaloFrame =
                    (currentDizzyHaloFrame + 1) % validHaloFrames.Count;
                dizzyHaloRenderer.sprite = validHaloFrames[currentDizzyHaloFrame];
            }
        }

        // 3. Gerakan halus 60 FPS & kunci titik tengah (center-lock) agar Halo tidak patah-patah
        if (dizzyHaloObject != null && dizzyHaloRenderer != null && dizzyHaloRenderer.sprite != null)
        {
            // Kompensasi titik tengah bounds sprite jika pivot hasil slice tidak persis di tengah
            Vector2 spriteCenterOffset = (Vector2)dizzyHaloRenderer.sprite.bounds.center * dizzyHaloScale;

            float bobX = 0f;
            float bobY = 0f;
            float tiltAngle = 0f;
            float pulseScale = 1f;

            if (smoothHaloMotion)
            {
                bobX = Mathf.Cos(haloContinuousTime * 7f) * 0.045f;
                bobY = Mathf.Sin(haloContinuousTime * 14f) * 0.03f;
                tiltAngle = Mathf.Sin(haloContinuousTime * 7f) * 5f;
                pulseScale = 1f + Mathf.Sin(haloContinuousTime * 14f) * 0.035f;
            }

            dizzyHaloObject.transform.localPosition = new Vector3(
                dizzyHaloOffset.x - spriteCenterOffset.x + bobX,
                dizzyHaloOffset.y - spriteCenterOffset.y + bobY,
                0f
            );

            dizzyHaloObject.transform.localRotation = Quaternion.Euler(0f, 0f, tiltAngle);
            dizzyHaloObject.transform.localScale = Vector3.one * (dizzyHaloScale * pulseScale);
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPointIndex];

        if (targetPoint == null)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = targetPoint.position;
        Vector2 difference = targetPosition - currentPosition;

        if (difference.magnitude <= waypointReachThreshold ||
            (Mathf.Abs(difference.x) <= axisAlignThreshold &&
             Mathf.Abs(difference.y) <= axisAlignThreshold))
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            ClearBypassState();
            targetPoint = patrolPoints[currentPointIndex];

            if (targetPoint == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            targetPosition = targetPoint.position;
            difference = targetPosition - currentPosition;
        }

        float stepDistance = moveSpeed * Time.fixedDeltaTime;
        Vector2 cardinalDirection = GetCardinalMoveDirection(difference, stepDistance);

        if (cardinalDirection == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        ApplyCardinalVelocity(cardinalDirection, difference, moveSpeed);
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

        float stepDistance = chaseSpeed * Time.fixedDeltaTime;
        Vector2 cardinalDirection = GetCardinalMoveDirection(difference, stepDistance);

        if (cardinalDirection == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        ApplyCardinalVelocity(cardinalDirection, difference, chaseSpeed);
    }

    void ApplyCardinalVelocity(Vector2 cardinalDirection, Vector2 difference, float speed)
    {
        if (activeBypassDir != Vector2.zero)
        {
            rb.linearVelocity = cardinalDirection * speed;
            return;
        }

        float dt = Time.fixedDeltaTime;

        if (Mathf.Abs(cardinalDirection.x) > 0f)
        {
            float maxSpeedX = dt > 0f ? Mathf.Abs(difference.x) / dt : speed;
            float finalSpeed = Mathf.Min(speed, maxSpeedX);
            rb.linearVelocity = new Vector2(cardinalDirection.x * finalSpeed, 0f);
        }
        else if (Mathf.Abs(cardinalDirection.y) > 0f)
        {
            float maxSpeedY = dt > 0f ? Mathf.Abs(difference.y) / dt : speed;
            float finalSpeed = Mathf.Min(speed, maxSpeedY);
            rb.linearVelocity = new Vector2(0f, cardinalDirection.y * finalSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    Vector2 GetCardinalMoveDirection(Vector2 difference, float stepDistance)
    {
        if (activeBypassDir != Vector2.zero && activeBlockedDir != Vector2.zero)
        {
            bool stillBlockedBeside = IsWallBlocking(activeBlockedDir, stepDistance + 0.35f);

            if (!stillBlockedBeside)
            {
                bypassClearanceTimer -= Time.fixedDeltaTime;

                if (bypassClearanceTimer > 0f && !IsWallBlocking(activeBypassDir, stepDistance))
                {
                    return activeBypassDir;
                }

                Vector2 clearTurnDir = activeBlockedDir;
                movingHorizontally = Mathf.Abs(clearTurnDir.x) > 0f;
                ClearBypassState();
                return clearTurnDir;
            }
            else
            {
                bypassClearanceTimer = cornerClearanceTime;

                if (!IsWallBlocking(activeBypassDir, stepDistance))
                {
                    return activeBypassDir;
                }

                Vector2 reverseBypass = -activeBypassDir;
                if (!IsWallBlocking(reverseBypass, stepDistance))
                {
                    activeBypassDir = reverseBypass;
                    return activeBypassDir;
                }

                ClearBypassState();
            }
        }

        float absX = Mathf.Abs(difference.x);
        float absY = Mathf.Abs(difference.y);

        Vector2 horizontalDir = absX > axisAlignThreshold
            ? (difference.x > 0f ? Vector2.right : Vector2.left)
            : Vector2.zero;

        Vector2 verticalDir = absY > axisAlignThreshold
            ? (difference.y > 0f ? Vector2.up : Vector2.down)
            : Vector2.zero;

        if (horizontalDir == Vector2.zero && verticalDir == Vector2.zero)
        {
            if (absX >= absY && absX > 0.001f)
                horizontalDir = difference.x > 0f ? Vector2.right : Vector2.left;
            else if (absY > 0.001f)
                verticalDir = difference.y > 0f ? Vector2.up : Vector2.down;
        }

        if (horizontalDir == Vector2.zero)
        {
            movingHorizontally = false;
        }
        else if (verticalDir == Vector2.zero)
        {
            movingHorizontally = true;
        }
        else if (movingHorizontally && absY > absX + axisSwitchThreshold)
        {
            movingHorizontally = false;
        }
        else if (!movingHorizontally && absX > absY + axisSwitchThreshold)
        {
            movingHorizontally = true;
        }

        Vector2 primaryDir = movingHorizontally ? horizontalDir : verticalDir;

        if (primaryDir != Vector2.zero && !IsWallBlocking(primaryDir, stepDistance))
        {
            return primaryDir;
        }

        if (primaryDir != Vector2.zero)
        {
            Vector2 bestBypass = ChooseBestBypassDirection(primaryDir, difference, stepDistance);

            if (bestBypass != Vector2.zero)
            {
                activeBlockedDir = primaryDir;
                activeBypassDir = bestBypass;
                bypassClearanceTimer = cornerClearanceTime;
                movingHorizontally = Mathf.Abs(bestBypass.x) > 0f;
                return activeBypassDir;
            }
        }

        return Vector2.zero;
    }

    Vector2 ChooseBestBypassDirection(Vector2 blockedDir, Vector2 difference, float stepDistance)
    {
        Vector2 perpA;
        Vector2 perpB;

        if (Mathf.Abs(blockedDir.x) > 0f)
        {
            perpA = difference.y >= 0f ? Vector2.up : Vector2.down;
            perpB = -perpA;
        }
        else
        {
            perpA = difference.x >= 0f ? Vector2.right : Vector2.left;
            perpB = -perpA;
        }

        bool canMoveA = !IsWallBlocking(perpA, stepDistance);
        bool canMoveB = !IsWallBlocking(perpB, stepDistance);

        if (canMoveA && !canMoveB)
            return perpA;

        if (canMoveB && !canMoveA)
            return perpB;

        if (!canMoveA && !canMoveB)
            return Vector2.zero;

        float openDistA = FindOpeningDistance(perpA, blockedDir);
        float openDistB = FindOpeningDistance(perpB, blockedDir);

        if (openDistB < openDistA - 0.25f)
            return perpB;

        return perpA;
    }

    float FindOpeningDistance(Vector2 perpDir, Vector2 blockedDir)
    {
        const float step = 0.5f;
        const int maxSteps = 24;

        Vector2 startPos = rb.position;

        for (int i = 1; i <= maxSteps; i++)
        {
            Vector2 prevPos = startPos + perpDir * ((i - 1) * step);
            Vector2 samplePos = startPos + perpDir * (i * step);

            if (IsWallBlockingAt(prevPos, perpDir, step))
            {
                return 999f;
            }

            if (!IsWallBlockingAt(samplePos, blockedDir, 0.5f))
            {
                return i * step;
            }
        }

        return 500f;
    }

    void ClearBypassState()
    {
        activeBypassDir = Vector2.zero;
        activeBlockedDir = Vector2.zero;
        bypassClearanceTimer = 0f;
    }

    bool IsWallBlocking(Vector2 direction, float distance)
    {
        return IsWallBlockingAt(rb.position, direction, distance);
    }

    bool IsWallBlockingAt(Vector2 origin, Vector2 direction, float distance)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return false;

        int combinedMask = wallLayer.value | (1 << 0);
        float castDistance = Mathf.Max(distance, 0.02f) + barrierSkinWidth;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            origin,
            collisionRadius,
            direction.normalized,
            castDistance,
            combinedMask
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i].collider;

            if (col == null)
                continue;

            if (col.gameObject == gameObject || col.transform.IsChildOf(transform))
                continue;

            if (col.isTrigger)
                continue;

            if (col.CompareTag("Player"))
                continue;

            return true;
        }

        return false;
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
        if (isCaught || isKnockedOut)
            return;

        isChasing = true;
        ClearBypassState();
        Debug.Log("GUARD MULAI MENGEJAR!");
    }

    public void StopChase()
    {
        isChasing = false;
        ClearBypassState();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log("GUARD BERHENTI MENGEJAR DAN KEMBALI PATROLI!");
    }

    public void SetCaught()
    {
        isCaught = true;
        isChasing = false;
        ClearBypassState();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        ResetAnimation();
        SetIdleSprite();
    }

    /// <summary>
    /// Dipanggil saat Player MENANG minigame Kertas Gunting Batu.
    /// Guard akan pingsan selama 'stunDuration' detik, mengaktifkan barrier agar tidak bisa diinjak,
    /// dan menampilkan animasi Halo pusing secara looping mulus.
    /// </summary>
    public void KnockOutGuard()
    {
        RebuildValidAnimationFrames();

        isKnockedOut = true;
        isCaught = false;
        isChasing = false;
        ClearBypassState();

        stunRemainingTimer = stunDuration;
        knockedOutFrameTimer = 0f;
        haloContinuousTime = 0f;
        currentKnockedOutBodyFrame = 0;
        currentDizzyHaloFrame = 0;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        ResetAnimation();

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.color = Color.white;

            if (validBodyFrames.Count > 0)
            {
                spriteRenderer.sprite = validBodyFrames[0];
            }
            else if (knockedOutSprite != null)
            {
                spriteRenderer.sprite = knockedOutSprite;
            }
            else
            {
                SetIdleSprite();
            }
        }

        // 1. Aktifkan Halo pusing di atas kepala Guard
        if (validHaloFrames.Count > 0)
        {
            if (dizzyHaloObject == null)
            {
                dizzyHaloObject = new GameObject("DizzyHaloEffect");
                dizzyHaloObject.transform.SetParent(transform, false);
                dizzyHaloRenderer = dizzyHaloObject.AddComponent<SpriteRenderer>();
            }

            dizzyHaloObject.SetActive(true);
            dizzyHaloRenderer.sprite = validHaloFrames[0];

            if (spriteRenderer != null)
            {
                dizzyHaloRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                dizzyHaloRenderer.sortingOrder = spriteRenderer.sortingOrder + 2;
            }

            Vector2 spriteCenterOffset = (Vector2)validHaloFrames[0].bounds.center * dizzyHaloScale;
            dizzyHaloObject.transform.localPosition = new Vector3(
                dizzyHaloOffset.x - spriteCenterOffset.x,
                dizzyHaloOffset.y - spriteCenterOffset.y,
                0f
            );
            dizzyHaloObject.transform.localScale = Vector3.one * dizzyHaloScale;
        }

        // 2. Aktifkan Barrier fisik di sekitar Guard agar tidak bisa diinjak Player
        EnableKnockedOutBarrier();

        // 3. Matikan sementara VisionCone dan DetectionBar selama Guard pingsan
        if (guardVision != null)
        {
            if (guardVision.visionCone != null)
                guardVision.visionCone.gameObject.SetActive(false);

            guardVision.enabled = false;
        }

        if (guardDetection != null)
        {
            guardDetection.detection = 0f;
            if (guardDetection.detectionBar != null)
                guardDetection.detectionBar.value = 0f;

            guardDetection.enabled = false;
        }

        Debug.Log("GUARD PINGSAN SELAMA " + stunDuration + " DETIK!");
    }

    /// <summary>
    /// Membuat collider padat (non-trigger) di sekitar Guard saat pingsan
    /// dan menggeser Player sedikit ke luar jika sebelumnya berhimpitan dengan badan Guard.
    /// </summary>
    void EnableKnockedOutBarrier()
    {
        if (knockedOutBarrierObject == null)
        {
            knockedOutBarrierObject = new GameObject("GuardKnockedOutBarrier");
            knockedOutBarrierObject.transform.SetParent(transform, false);

            Rigidbody2D barrierRb = knockedOutBarrierObject.AddComponent<Rigidbody2D>();
            barrierRb.bodyType = RigidbodyType2D.Kinematic;
            barrierRb.simulated = true;
            barrierRb.useFullKinematicContacts = true;
            barrierRb.gravityScale = 0f;
            barrierRb.constraints = RigidbodyConstraints2D.FreezeAll;

            knockedOutBarrierCollider = knockedOutBarrierObject.AddComponent<BoxCollider2D>();
            knockedOutBarrierCollider.isTrigger = false;
        }

        knockedOutBarrierObject.transform.localPosition = Vector3.zero;
        knockedOutBarrierObject.transform.localScale = Vector3.one;
        knockedOutBarrierCollider.size = knockedOutBarrierSize;
        knockedOutBarrierCollider.offset = knockedOutBarrierOffset;
        knockedOutBarrierObject.SetActive(true);

        // Jika posisi Player terlalu dekat / menempel di dalam area barrier Guard saat minigame selesai,
        // geser Player sedikit keluar agar tidak terjebak di dalam collider Guard
        if (player != null)
        {
            Vector2 barrierCenter = (Vector2)transform.position +
                Vector2.Scale(knockedOutBarrierOffset, (Vector2)transform.lossyScale);
            Vector2 halfSize = Vector2.Scale(knockedOutBarrierSize, (Vector2)transform.lossyScale) * 0.5f +
                new Vector2(0.35f, 0.35f);

            Vector2 diff = (Vector2)player.position - barrierCenter;

            if (Mathf.Abs(diff.x) < halfSize.x && Mathf.Abs(diff.y) < halfSize.y)
            {
                float pushX = halfSize.x - Mathf.Abs(diff.x);
                float pushY = halfSize.y - Mathf.Abs(diff.y);

                Vector2 newPlayerPos = player.position;
                if (pushX < pushY)
                {
                    float signX = diff.x >= 0f ? 1f : -1f;
                    newPlayerPos.x = barrierCenter.x + signX * (halfSize.x + 0.08f);
                }
                else
                {
                    float signY = diff.y >= 0f ? 1f : -1f;
                    newPlayerPos.y = barrierCenter.y + signY * (halfSize.y + 0.08f);
                }

                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                    playerRb.position = newPlayerPos;

                player.position = newPlayerPos;
            }
        }
    }

    /// <summary>
    /// Dipanggil otomatis setelah waktu pingsan (stunDuration) habis.
    /// Guard sadar kembali, barrier anti-injak dan Halo dimatikan, lalu Guard kembali berpatroli.
    /// </summary>
    public void WakeUpGuard()
    {
        if (!isKnockedOut)
            return;

        isKnockedOut = false;
        isCaught = false;
        isChasing = false;
        ClearBypassState();

        if (dizzyHaloObject != null)
            dizzyHaloObject.SetActive(false);

        if (knockedOutBarrierObject != null)
            knockedOutBarrierObject.SetActive(false);

        if (guardVision != null)
        {
            guardVision.enabled = true;
            if (guardVision.visionCone != null)
                guardVision.visionCone.gameObject.SetActive(true);
        }

        if (guardDetection != null)
        {
            guardDetection.detection = 0f;
            if (guardDetection.detectionBar != null)
                guardDetection.detectionBar.value = 0f;

            guardDetection.enabled = true;
        }

        if (guardCatch == null)
            guardCatch = GetComponent<GuardCatch>();

        if (guardCatch != null)
            guardCatch.OnGuardWokeUp();

        ResetAnimation();
        SetIdleSprite();

        Debug.Log("WAKTU PINGSAN HABIS! GUARD SADAR DAN KEMBALI PATROLI!");
    }

    public void ResetGuard()
    {
        isKnockedOut = false;
        isCaught = false;
        isChasing = false;
        ClearBypassState();

        if (dizzyHaloObject != null)
            dizzyHaloObject.SetActive(false);

        if (knockedOutBarrierObject != null)
            knockedOutBarrierObject.SetActive(false);

        if (guardVision != null)
        {
            guardVision.enabled = true;
            if (guardVision.visionCone != null)
                guardVision.visionCone.gameObject.SetActive(true);
        }

        if (guardDetection != null)
        {
            guardDetection.detection = 0f;
            if (guardDetection.detectionBar != null)
                guardDetection.detectionBar.value = 0f;

            guardDetection.enabled = true;
        }

        if (guardCatch == null)
            guardCatch = GetComponent<GuardCatch>();

        if (guardCatch != null)
            guardCatch.OnGuardWokeUp();

        currentPointIndex = 0;

        if (rb != null)
        {
            rb.position = pointA != null
                ? (Vector2)pointA.position
                : rb.position;

            rb.linearVelocity = Vector2.zero;
        }

        if (pointA != null)
            transform.position = pointA.position;

        ResetAnimation();
        SetIdleSprite();

        Debug.Log("GUARD KEMBALI PATROLI DARI TITIK A!");
    }

    public Vector2 GetFacingDirection()
    {
        bool isTrackingPlayer = isChasing ||
            (guardDetection != null && guardDetection.detection > 0.01f);

        if (isTrackingPlayer && player != null)
        {
            Vector2 dirToPlayer = (Vector2)player.position - (Vector2)transform.position;

            if (dirToPlayer.sqrMagnitude > 0.0001f)
            {
                return dirToPlayer.normalized;
            }
        }

        switch (lastDirection)
        {
            case FacingDirection.Front:
                return Vector2.down;

            case FacingDirection.Back:
                return Vector2.up;

            case FacingDirection.Left:
                return Vector2.left;

            case FacingDirection.Right:
                return Vector2.right;

            default:
                return Vector2.down;
        }
    }

    public bool IsChasing()
    {
        return isChasing;
    }

    public bool IsCaught()
    {
        return isCaught;
    }

    public bool IsKnockedOut()
    {
        return isKnockedOut;
    }
}