using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CCTVAlarmEffect : MonoBehaviour
{
    public static CCTVAlarmEffect Instance;

    [Header("Referensi Utama")]
    [Tooltip("Komponen UI Image pada AlarmOverlay di dalam CCTV_AlarmCanvas.")]
    public Image alarmOverlay;

    [Tooltip("Rigidbody2D milik Player.")]
    public Rigidbody2D playerRb;

    [Header("Gambar Layar 'KAMU TERTANGKAP!'")]
    [Tooltip("Masukkan sprite PNG 'FadePanelTertangkap' (gambar latar hitam bertuliskan KAMU TERTANGKAP!) ke slot ini.")]
    public Sprite caughtScreenSprite;

    [Tooltip("Opsional: Jika Anda membuat objek UI Image/Text terpisah untuk tulisan 'KAMU TERTANGKAP!', masukkan ke sini.")]
    public GameObject caughtExtraObject;

    [Header("Pengaturan Durasi Alarm & Layar Tertangkap")]
    [Tooltip("Durasi layar berkedip merah saat pertama kali terdeteksi CCTV (detik).")]
    public float alarmDuration = 2f;

    [Tooltip("Kecepatan kedipan merah (detik).")]
    public float blinkInterval = 0.2f;

    [Tooltip("Durasi tampilnya layar hitam bertuliskan 'KAMU TERTANGKAP!' sebelum kembali ke Checkpoint (detik).")]
    public float blackDuration = 1.5f;

    private bool alarmPlaying = false;
    private RigidbodyConstraints2D originalConstraints;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        Canvas alarmCanvas = GetComponent<Canvas>();
        if (alarmCanvas == null && alarmOverlay != null)
            alarmCanvas = alarmOverlay.GetComponentInParent<Canvas>();

        if (alarmCanvas != null)
        {
            alarmCanvas.overrideSorting = true;
            if (alarmCanvas.sortingOrder < 30)
                alarmCanvas.sortingOrder = 30;
        }

        if (alarmOverlay != null)
        {
            if (caughtScreenSprite == null && alarmOverlay.sprite != null)
            {
                caughtScreenSprite = alarmOverlay.sprite;
            }

            alarmOverlay.sprite = null;
            // Matikan RaycastTarget dan nonaktifkan objek saat alarm tidak bunyi
            // agar tidak menghalangi klik mouse pada tombol Pause Menu!
            alarmOverlay.raycastTarget = false;
            SetAlpha(0f);
            alarmOverlay.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("AlarmOverlay belum dihubungkan!");
        }

        if (caughtExtraObject != null)
            caughtExtraObject.SetActive(false);
    }

    public void PlayAlarm()
    {
        if (alarmPlaying)
            return;

        if (playerRb == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerRb = playerObj.GetComponent<Rigidbody2D>();
        }

        if (alarmOverlay == null || playerRb == null)
        {
            Debug.LogError("Referensi CCTVAlarmEffect belum lengkap!");
            return;
        }

        StartCoroutine(AlarmSequence());
    }

    IEnumerator AlarmSequence()
    {
        alarmPlaying = true;

        originalConstraints = playerRb.constraints;

        playerRb.linearVelocity = Vector2.zero;
        playerRb.angularVelocity = 0f;
        playerRb.constraints = RigidbodyConstraints2D.FreezeAll;

        Debug.Log("ALARM CCTV AKTIF!");

        // Aktifkan AlarmOverlay hanya selama sekuens alarm berlangsung
        alarmOverlay.gameObject.SetActive(true);
        alarmOverlay.raycastTarget = false;
        alarmOverlay.sprite = null;

        if (caughtExtraObject != null)
            caughtExtraObject.SetActive(false);

        float timer = 0f;
        bool redVisible = false;

        while (timer < alarmDuration)
        {
            redVisible = !redVisible;

            SetColor(Color.red);
            SetAlpha(redVisible ? 0.65f : 0f);

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (caughtScreenSprite != null)
        {
            alarmOverlay.sprite = caughtScreenSprite;
            alarmOverlay.type = Image.Type.Simple;
            alarmOverlay.preserveAspect = false;
            SetColor(Color.white);
            SetAlpha(1f);
        }
        else
        {
            alarmOverlay.sprite = null;
            SetColor(Color.black);
            SetAlpha(1f);
        }

        if (caughtExtraObject != null)
            caughtExtraObject.SetActive(true);

        Debug.Log("LAYAR TERTANGKAP TAMPIL!");

        yield return new WaitForSeconds(blackDuration);

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer(playerRb.gameObject);
        }
        else
        {
            Debug.LogError("CheckpointManager tidak ditemukan!");
        }

        yield return new WaitForSeconds(0.3f);

        playerRb.constraints = originalConstraints;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.angularVelocity = 0f;

        if (caughtExtraObject != null)
            caughtExtraObject.SetActive(false);

        alarmOverlay.sprite = null;
        SetAlpha(0f);
        SetColor(Color.red);
        alarmOverlay.raycastTarget = false;
        alarmOverlay.gameObject.SetActive(false);

        alarmPlaying = false;

        Debug.Log("ALARM SELESAI! PLAYER KEMBALI KE CHECKPOINT.");
    }

    void SetAlpha(float alpha)
    {
        Color color = alarmOverlay.color;
        color.a = alpha;
        alarmOverlay.color = color;
    }

    void SetColor(Color color)
    {
        Color currentColor = alarmOverlay.color;

        currentColor.r = color.r;
        currentColor.g = color.g;
        currentColor.b = color.b;

        alarmOverlay.color = currentColor;
    }
}