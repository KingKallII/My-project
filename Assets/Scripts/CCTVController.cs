using UnityEngine;

public class CCTVController : MonoBehaviour
{
    public enum PresetDirection
    {
        SesuaiInitialDirectionXY = 0,
        Kanan = 1,
        Kiri = 2,
        Bawah = 3,
        Atas = 4,
        SerongKananBawah = 5,
        SerongKiriBawah = 6,
        SerongKananAtas = 7,
        SerongKiriAtas = 8
    }

    [Header("Pengaturan Penglihatan")]
    public float viewDistance = 7f;

    [Range(1f, 180f)]
    public float viewAngle = 45f;

    public LayerMask wallLayer;

    [Header("Pengaturan Arah & Gerakan Swipe CCTV")]
    [Tooltip("Pilih arah hadap tengah CCTV dengan mudah, atau pilih 'SesuaiInitialDirectionXY' untuk mengatur lewat nilai X dan Y di bawah.")]
    public PresetDirection arahHadapCCTV = PresetDirection.SesuaiInitialDirectionXY;

    [Tooltip("Arah tengah sapuan CCTV (contoh: Kanan = 1,0 | Kiri = -1,0 | Bawah = 0,-1 | Atas = 0,1).")]
    public Vector2 initialDirection = Vector2.right;

    [Tooltip("Lebar sudut sapuan kiri-kanan CCTV dalam derajat (contoh: 90 derajat).")]
    public float sweepAngle = 90f;

    [Tooltip("Kecepatan gerak swipe CCTV (pastikan lebih dari 0 agar bergerak, contoh: 45).")]
    public float sweepSpeed = 45f;

    [Header("Referensi")]
    public VisionCone visionCone;
    public CCTVAlarmEffect alarmEffect;

    private Transform player;
    private Vector2 currentDirection;
    private bool playerCaught = false;

    void Awake()
    {
        ResolveConflictingComponents();
    }

    void Start()
    {
        ResolveConflictingComponents();

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
            visionCone = GetComponent<VisionCone>();
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
            alarmEffect = FindFirstObjectByType<CCTVAlarmEffect>();
        }

        currentDirection = GetBaseDirection();
        UpdateVisionCone();
    }

    /// <summary>
    /// Mematikan skrip CCTVVision lama atau duplikat CCTVController pada parent/child
    /// yang sebelumnya menimpa arah VisionCone menjadi selalu menghadap ke kanan dan diam.
    /// </summary>
    void ResolveConflictingComponents()
    {
        if (visionCone == null)
        {
            visionCone = GetComponent<VisionCone>();
            if (visionCone == null)
                visionCone = GetComponentInChildren<VisionCone>();
        }

        // 1. Matikan semua komponen CCTVVision pada objek ini, parent, maupun child
        CCTVVision[] oldVisions = GetComponentsInParent<CCTVVision>(true);
        for (int i = 0; i < oldVisions.Length; i++)
        {
            if (oldVisions[i] != null && oldVisions[i].enabled)
            {
                oldVisions[i].enabled = false;
            }
        }

        CCTVVision[] childVisions = GetComponentsInChildren<CCTVVision>(true);
        for (int i = 0; i < childVisions.Length; i++)
        {
            if (childVisions[i] != null && childVisions[i].enabled)
            {
                childVisions[i].enabled = false;
            }
        }

        // 2. Jika ada dua CCTVController (satu di Parent CCTV_1/CCTV_2 dan satu di Child VisionCone_CCTV),
        //    matikan duplikat di Parent agar tidak saling berebut mengatur VisionCone yang sama
        if (transform.parent != null)
        {
            CCTVController parentController = transform.parent.GetComponent<CCTVController>();
            if (parentController != null && parentController != this && parentController.enabled)
            {
                // Jika pengguna sudah mengatur arah khusus di Parent (bukan default 1,0), salin arahnya sebelum dimatikan
                if (initialDirection == Vector2.right &&
                    arahHadapCCTV == PresetDirection.SesuaiInitialDirectionXY &&
                    parentController.initialDirection != Vector2.right &&
                    parentController.initialDirection.sqrMagnitude > 0.001f)
                {
                    initialDirection = parentController.initialDirection;
                }

                // Pastikan sweepSpeed tetap aktif jika di parent bernilai 0
                if (sweepSpeed <= 0.01f && parentController.sweepSpeed > 0.01f)
                {
                    sweepSpeed = parentController.sweepSpeed;
                }

                parentController.enabled = false;
            }
        }
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

    /// <summary>
    /// Memastikan arah VisionCone tetap diperbarui di LateUpdate agar tidak pernah tertimpa skrip lain.
    /// </summary>
    void LateUpdate()
    {
        if (visionCone == null)
            return;

        UpdateVisionCone();
    }

    Vector2 GetBaseDirection()
    {
        switch (arahHadapCCTV)
        {
            case PresetDirection.Kanan:
                initialDirection = Vector2.right;
                break;
            case PresetDirection.Kiri:
                initialDirection = Vector2.left;
                break;
            case PresetDirection.Bawah:
                initialDirection = Vector2.down;
                break;
            case PresetDirection.Atas:
                initialDirection = Vector2.up;
                break;
            case PresetDirection.SerongKananBawah:
                initialDirection = new Vector2(1f, -1f).normalized;
                break;
            case PresetDirection.SerongKiriBawah:
                initialDirection = new Vector2(-1f, -1f).normalized;
                break;
            case PresetDirection.SerongKananAtas:
                initialDirection = new Vector2(1f, 1f).normalized;
                break;
            case PresetDirection.SerongKiriAtas:
                initialDirection = new Vector2(-1f, 1f).normalized;
                break;
        }

        if (initialDirection.sqrMagnitude < 0.0001f)
        {
            return Vector2.down;
        }

        return initialDirection.normalized;
    }

    void UpdateSweepDirection()
    {
        Vector2 baseDir = GetBaseDirection();

        // Jika sweepAngle atau sweepSpeed > 0, gerakkan kerucut pandang bolak-balik secara mulus
        if (sweepAngle > 0.01f && sweepSpeed > 0.01f)
        {
            float offset = Mathf.PingPong(
                Time.time * sweepSpeed,
                sweepAngle
            ) - (sweepAngle / 2f);

            currentDirection =
                Quaternion.Euler(0f, 0f, offset) * baseDir;
        }
        else
        {
            currentDirection = baseDir;
        }
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

    /// <summary>
    /// Saat nilai di Inspector diubah (termasuk di luar Play Mode), langsung perbarui arah kerucut di Scene.
    /// </summary>
    void OnValidate()
    {
        currentDirection = GetBaseDirection();

        if (visionCone != null && Application.isPlaying)
        {
            UpdateVisionCone();
        }
    }
}