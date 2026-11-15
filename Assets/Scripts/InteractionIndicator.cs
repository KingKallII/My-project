using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionIndicator : MonoBehaviour
{
    public static InteractionIndicator instance;

    [Header("Referensi UI")]
    public GameObject promptContainer;
    public Image keyIconImage;
    public TMP_Text promptText;

    private Coroutine alertCoroutine;
    private bool isShowingAlert = false;

    private void Awake()
    {
        instance = this;
        if (promptContainer != null)
            promptContainer.SetActive(false);
    }

    private void LateUpdate()
    {
        if (transform.parent != null)
        {
            float parentScaleX = Mathf.Sign(transform.parent.localScale.x);
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * parentScaleX;
            transform.localScale = currentScale;
        }
    }

    public void ShowPrompt(string message, bool showKeyIcon = true)
    {
        if (isShowingAlert)
            return;

        if (promptContainer != null)
            promptContainer.SetActive(true);

        if (keyIconImage != null)
            keyIconImage.gameObject.SetActive(showKeyIcon);

        if (promptText != null)
        {
            promptText.color = Color.white;
            promptText.text = message;
        }
    }

    public void HidePrompt()
    {
        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
            alertCoroutine = null;
        }

        isShowingAlert = false;

        if (promptContainer != null)
            promptContainer.SetActive(false);
    }

    /// <summary>
    /// Menampilkan pesan peringatan sementara (merah muda terang)
    /// </summary>
    public void ShowAlert(string alertMessage, float duration = 2f, System.Func<string> getReturnPrompt = null)
    {
        ShowCustomMessage(alertMessage, new Color(1f, 0.35f, 0.35f), duration, getReturnPrompt);
    }

    /// <summary>
    /// Menampilkan pesan keberhasilan sementara (hijau terang)
    /// </summary>
    public void ShowSuccess(string successMessage, float duration = 2f, System.Func<string> getReturnPrompt = null)
    {
        ShowCustomMessage(successMessage, new Color(0.35f, 1f, 0.45f), duration, getReturnPrompt);
    }

    private void ShowCustomMessage(string message, Color textColor, float duration, System.Func<string> getReturnPrompt)
    {
        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);

        alertCoroutine = StartCoroutine(AlertRoutine(message, textColor, duration, getReturnPrompt));
    }

    private IEnumerator AlertRoutine(string message, Color textColor, float duration, System.Func<string> getReturnPrompt)
    {
        isShowingAlert = true;

        if (promptContainer != null)
            promptContainer.SetActive(true);

        if (keyIconImage != null)
            keyIconImage.gameObject.SetActive(false);

        if (promptText != null)
        {
            promptText.color = textColor;
            promptText.text = message;
        }

        yield return new WaitForSeconds(duration);

        isShowingAlert = false;
        alertCoroutine = null;

        string nextPrompt = getReturnPrompt != null ? getReturnPrompt() : "";
        if (!string.IsNullOrEmpty(nextPrompt))
        {
            ShowPrompt(nextPrompt, true);
        }
        else
        {
            if (promptContainer != null)
                promptContainer.SetActive(false);
        }
    }
}