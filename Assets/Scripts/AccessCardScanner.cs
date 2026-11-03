using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class AccessCardScanner : MonoBehaviour
{
    [Header("Inventory Player")]
    public PlayerInventory playerInventory;

    [Header("Referensi Pintu")]
    public DoorInteraction doorInteraction;

    [Header("Notification")]
    public TMP_Text notificationText;
    public float notificationDuration = 2f;

    private bool playerNearby;
    private bool accessGranted;
    private Coroutine notificationCoroutine;

    void Start()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        Debug.Log("SCANNER AKTIF!");

        Debug.Log("Door Interaction: " +
            (doorInteraction != null
                ? doorInteraction.name
                : "BELUM DIHUBUNGKAN"));
    }

    void Update()
    {
        if (!playerNearby || accessGranted)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            ScanAccessCard();
    }

    void ScanAccessCard()
    {
        Debug.Log("TOMBOL SCAN DITEKAN!");

        if (playerInventory == null)
        {
            Debug.LogError("PLAYER INVENTORY TIDAK DITEMUKAN!");
            return;
        }

        Debug.Log("Status kartu: " +
            playerInventory.hasAccessCard);

        if (!playerInventory.hasAccessCard)
        {
            ShowNotification("Access Card Required!");
            Debug.LogWarning("KARTU BELUM DIMILIKI!");
            return;
        }

        if (doorInteraction == null)
        {
            Debug.LogError("DOOR INTERACTION BELUM DIHUBUNGKAN!");
            return;
        }

        doorInteraction.GrantAccess();

        accessGranted = true;

        Debug.Log("SCAN BERHASIL!");
        Debug.Log("Status scan pintu: " +
            doorInteraction.hasScannedCard);

        ShowNotification("Access Granted!");
    }

    void ShowNotification(string message)
    {
        if (notificationText == null)
            return;

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        notificationCoroutine =
            StartCoroutine(HideNotificationAfterDelay());
    }

    IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        notificationCoroutine = null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;

        playerInventory = other.GetComponent<PlayerInventory>();

        if (playerInventory == null)
            playerInventory =
                other.GetComponentInParent<PlayerInventory>();

        Debug.Log("PLAYER MASUK AREA SCANNER!");

        Debug.Log("Inventory ditemukan: " +
            (playerInventory != null));
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;
        playerInventory = null;

        Debug.Log("PLAYER KELUAR AREA SCANNER!");
    }
}