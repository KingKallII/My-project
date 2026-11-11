
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class SafePinManager : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject pinPanel;
    public TMP_Text pinDisplay;

    [Header("Notifikasi")]
    public GameObject accessSuccess;
    public GameObject accessFailed;

    [Header("Pengaturan PIN")]
    public string correctPIN = "456";

    [Header("Referensi Brankas")]
    public SafeInteraction safeInteraction;

    [Header("Player Movement")]
    public PlayerMovement playerMovement;

    private string enteredPIN = "";
    private bool isProcessing = false;

    void Start()
    {
        if (pinPanel != null)
            pinPanel.SetActive(false);

        if (accessSuccess != null)
            accessSuccess.SetActive(false);

        if (accessFailed != null)
            accessFailed.SetActive(false);

        UpdateDisplay();
    }

    void Update()
    {
        if (pinPanel != null &&
            pinPanel.activeSelf &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePinPanel();
        }
    }

    private void LockPlayer()
    {
        if (playerMovement != null)
            playerMovement.StopMovement();
    }

    private void UnlockPlayer()
    {
        if (playerMovement != null)
            playerMovement.ResumeMovement();
    }

    public void OpenPinPanel()
    {
        if (pinPanel == null || isProcessing)
            return;

        enteredPIN = "";
        UpdateDisplay();

        if (accessSuccess != null)
            accessSuccess.SetActive(false);

        if (accessFailed != null)
            accessFailed.SetActive(false);

        pinPanel.SetActive(true);

        // Hentikan gerakan pemain
        LockPlayer();

        Debug.Log("KEYPAD BRANKAS DIBUKA!");
    }

    public void AddDigit(int digit)
    {
        if (isProcessing || enteredPIN.Length >= 3)
            return;

        if (digit < 0 || digit > 9)
            return;

        enteredPIN += digit.ToString();

        UpdateDisplay();

        Debug.Log("PIN BRANKAS: " + enteredPIN);
    }

    public void DeleteDigit()
    {
        if (isProcessing || enteredPIN.Length == 0)
            return;

        enteredPIN = enteredPIN.Substring(
            0, enteredPIN.Length - 1
        );

        UpdateDisplay();
    }

    public void SubmitPIN()
    {
        if (isProcessing)
            return;

        if (enteredPIN.Length != 3)
        {
            Debug.Log("PIN harus terdiri dari 3 angka!");
            return;
        }

        if (enteredPIN == correctPIN)
        {
            StartCoroutine(PINSuccess());
        }
        else
        {
            StartCoroutine(PINFailed());
        }
    }

    IEnumerator PINSuccess()
    {
        isProcessing = true;

        if (accessSuccess != null)
            accessSuccess.SetActive(true);

        if (accessFailed != null)
            accessFailed.SetActive(false);

        Debug.Log("PIN BRANKAS BENAR!");

        yield return new WaitForSecondsRealtime(1.5f);

        if (accessSuccess != null)
            accessSuccess.SetActive(false);

        if (pinPanel != null)
            pinPanel.SetActive(false);

        if (safeInteraction != null)
            safeInteraction.UnlockSafe();

        enteredPIN = "";
        UpdateDisplay();

        // Aktifkan kembali gerakan pemain
        UnlockPlayer();

        isProcessing = false;
    }

    IEnumerator PINFailed()
    {
        isProcessing = true;

        if (accessFailed != null)
            accessFailed.SetActive(true);

        if (accessSuccess != null)
            accessSuccess.SetActive(false);

        Debug.Log("PIN BRANKAS SALAH!");

        yield return new WaitForSecondsRealtime(1.5f);

        if (accessFailed != null)
            accessFailed.SetActive(false);

        enteredPIN = "";
        UpdateDisplay();

        // Tetap terkunci karena keypad masih terbuka
        isProcessing = false;
    }

    public void ClosePinPanel()
    {
        if (isProcessing)
            return;

        enteredPIN = "";
        UpdateDisplay();

        if (pinPanel != null)
            pinPanel.SetActive(false);

        // Aktifkan kembali gerakan pemain
        UnlockPlayer();
    }

    void UpdateDisplay()
    {
        if (pinDisplay != null)
        {
            pinDisplay.text = enteredPIN.PadRight(3, '*');
        }
    }
}
