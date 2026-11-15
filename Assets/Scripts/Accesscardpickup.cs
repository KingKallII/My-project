using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class AccessCardPickup : MonoBehaviour
{
    [Header("Player")]
    public PlayerInventory playerInventory;

    [Header("UI Interaksi")]
    public GameObject interactionPrompt;
    public TMP_Text promptText;

    private bool playerNearby = false;
    private bool isCollected = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (!playerNearby || isCollected)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectAccessCard();
        }
    }

    void CollectAccessCard()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerInventory belum ditemukan!");
            return;
        }

        if (playerInventory.hasAccessCard)
        {
            Debug.Log("Player sudah memiliki Access Card!");
            return;
        }

        // Masukkan kartu ke inventory
        playerInventory.AddAccessCard();

        isCollected = true;

        // Sembunyikan petunjuk
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Hilangkan objek kartu
        gameObject.SetActive(false);

        Debug.Log("ACCESS CARD BERHASIL DIAMBIL!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isCollected)
            return;

        playerNearby = true;

        playerInventory = other.GetComponent<PlayerInventory>();

        if (playerInventory == null)
            playerInventory =
                other.GetComponentInParent<PlayerInventory>();

        if (playerInventory != null &&
            !playerInventory.hasAccessCard)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);

            if (promptText != null)
                promptText.text = "Tekan E untuk mengambil Access Card";
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        playerInventory = null;
    }
}