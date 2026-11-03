
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CCTVAlarmEffect : MonoBehaviour
{
    [Header("Referensi")]
    public Image alarmOverlay;
    public Rigidbody2D playerRb;

    [Header("Pengaturan Alarm")]
    public float alarmDuration = 2f;
    public float blinkInterval = 0.2f;
    public float blackDuration = 0.8f;

    private bool alarmPlaying = false;
    private RigidbodyConstraints2D originalConstraints;

    void Start()
    {
        if (alarmOverlay != null)
        {
            SetAlpha(0f);
        }
        else
        {
            Debug.LogError(
                "AlarmOverlay belum dihubungkan!"
            );
        }
    }

    public void PlayAlarm()
    {
        if (alarmPlaying)
            return;

        if (alarmOverlay == null || playerRb == null)
        {
            Debug.LogError(
                "Referensi CCTVAlarmEffect belum lengkap!"
            );
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

        SetColor(Color.black);
        SetAlpha(1f);

        Debug.Log("LAYAR MENJADI HITAM!");

        yield return new WaitForSeconds(blackDuration);

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer(
                playerRb.gameObject
            );
        }
        else
        {
            Debug.LogError(
                "CheckpointManager tidak ditemukan!"
            );
        }

        yield return new WaitForSeconds(0.3f);

        playerRb.constraints = originalConstraints;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.angularVelocity = 0f;

        SetAlpha(0f);
        SetColor(Color.red);

        alarmPlaying = false;

        Debug.Log(
            "ALARM SELESAI! PLAYER KEMBALI KE CHECKPOINT."
        );
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
