
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFadeController : MonoBehaviour
{
    [Header("Referensi Fade Panel")]
    public Image fadeImage;

    [Header("Pengaturan Fade")]
    public float fadeDuration = 1f;

    private bool isFading = false;

    public void FadeAndTeleport(
        Transform player,
        Transform spawnPoint
    )
    {
        if (isFading)
            return;

        if (fadeImage == null)
        {
            Debug.LogWarning("Fade Image belum dihubungkan!");
            return;
        }

        if (player == null || spawnPoint == null)
        {
            Debug.LogWarning(
                "Player atau LorongSpawnPoint belum dihubungkan!"
            );
            return;
        }

        StartCoroutine(
            FadeTeleportRoutine(player, spawnPoint)
        );
    }

    private IEnumerator FadeTeleportRoutine(
        Transform player,
        Transform spawnPoint
    )
    {
        isFading = true;
        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.SetAsLastSibling();

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        // Fade menjadi hitam.
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        // Pindahkan pemain ke lorong.
        player.position = spawnPoint.position;

        // Tunggu satu frame agar posisi diperbarui.
        yield return null;

        // Fade kembali menjadi normal.
        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);

        isFading = false;
    }
}
