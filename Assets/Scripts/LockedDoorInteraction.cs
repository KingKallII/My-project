
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LockedDoorInteraction : MonoBehaviour
{
    [Header("Status Pintu")]
    private bool playerNearby = false;
    private bool isUnlocked = false;
    private bool isTransitioning = false;

    [Header("Referensi Pemain")]
    public Transform player;
    public Transform lorongSpawnPoint;

    [Header("Referensi Pintu")]
    public Collider2D doorBarrier;

    [Header("Transisi Layar")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
            fadePanel.interactable = false;

            // Pastikan panel aktif agar dapat menerima perubahan alpha.
            fadePanel.gameObject.SetActive(true);

            Debug.Log("Fade Panel siap digunakan.");
        }
        else
        {
            Debug.LogError("Fade Panel belum dihubungkan!");
        }
    }

    void Update()
    {
        if (!playerNearby || !isUnlocked || isTransitioning)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(EnterBasementCorridor());
        }
    }

    public void UnlockDoor()
    {
        isUnlocked = true;

        if (doorBarrier != null)
            doorBarrier.enabled = false;

        Debug.Log("PINTU BASEMENT BERHASIL DIBUKA!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (isUnlocked)
                Debug.Log("Tekan E untuk masuk ke lorong basement.");
            else
                Debug.Log("Pintu masih terkunci. Selesaikan puzzle dahulu.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    IEnumerator EnterBasementCorridor()
    {
        if (player == null || lorongSpawnPoint == null ||
            fadePanel == null)
        {
            Debug.LogWarning("Referensi transisi pintu belum lengkap!");
            yield break;
        }

        isTransitioning = true;

        Debug.Log("FADE OUT DIMULAI");

        // Pastikan panel aktif dan berada paling depan.
        fadePanel.gameObject.SetActive(true);
        fadePanel.transform.SetAsLastSibling();
        fadePanel.blocksRaycasts = true;

        // Fade menjadi hitam.
        yield return StartCoroutine(Fade(1f));

        // Tunggu sebentar ketika layar sudah hitam.
        yield return new WaitForSecondsRealtime(0.2f);

        // Pindahkan player ke lorong.
        player.position = lorongSpawnPoint.position;

        Debug.Log("Player berhasil dipindahkan ke lorong basement.");

        // Tunggu satu frame agar posisi player diperbarui.
        yield return null;

        // Fade kembali menjadi normal.
        yield return StartCoroutine(Fade(0f));

        fadePanel.blocksRaycasts = false;

        Debug.Log("FADE SELESAI");

        isTransitioning = false;
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadePanel.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / fadeDuration);

            fadePanel.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                progress
            );

            yield return null;
        }

        fadePanel.alpha = targetAlpha;
    }
}
