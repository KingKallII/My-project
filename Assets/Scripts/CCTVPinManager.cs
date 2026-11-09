
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class CCTVPinManager : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject pinPanel;
    public TMP_Text pinDisplay;

    [Header("Notifikasi")]
    public GameObject accessSuccess;
    public GameObject accessFailed;

    [Header("Pengaturan PIN")]
    public string correctPIN = "123";

    [Header("Referensi Pintu CCTV")]
    public CCTVRoomDoorInteraction doorInteraction;

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
    }

    public void AddDigit(int digit)
    {
        if (isProcessing || enteredPIN.Length >= 3)
            return;

        if (digit < 0 || digit > 9)
            return;

        enteredPIN += digit.ToString();

        UpdateDisplay();
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
            Debug.Log("Masukkan PIN sebanyak 3 digit!");
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

        Debug.Log("PIN BENAR! AKSES CCTV BERHASIL.");

        yield return new WaitForSeconds(1.2f);

        if (accessSuccess != null)
            accessSuccess.SetActive(false);

        if (doorInteraction != null)
            doorInteraction.UnlockDoor();

        if (pinPanel != null)
            pinPanel.SetActive(false);

        enteredPIN = "";
        UpdateDisplay();

        isProcessing = false;
    }

    IEnumerator PINFailed()
    {
        isProcessing = true;

        if (accessFailed != null)
            accessFailed.SetActive(true);

        Debug.Log("PIN SALAH! AKSES DITOLAK.");

        yield return new WaitForSeconds(1.2f);

        if (accessFailed != null)
            accessFailed.SetActive(false);

        enteredPIN = "";
        UpdateDisplay();

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
    }

    void UpdateDisplay()
    {
        if (pinDisplay != null)
        {
            pinDisplay.text = enteredPIN.PadRight(3, '*');
        }
    }
}
