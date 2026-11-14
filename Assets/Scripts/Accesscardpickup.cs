using UnityEngine;
using UnityEngine.InputSystem;

public class AccessCardPickup : MonoBehaviour
{
    [Header("Player")]
    public PlayerInventory playerInventory;

    [Header("Tampilan Khusus di Hotbar")]
    [Tooltip("Masukkan sprite kartu baru untuk hotbar. Jika kosong, otomatis memakai Access Card Sprite di PlayerInventory.")]
    public Sprite hotbarIcon;
    public bool gunakanUkuranKhusus = true;
    public Vector2 ukuranDiHotbar = new Vector2(140f, 85f);

    [Header("Pengaturan Notifikasi Kepala Player")]
    public string teksInteraksi = "Ambil Access Card";

    private bool playerNearby = false;
    private bool isCollected = false;

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

        if (playerInventory.HasAccessCard)
        {
            Debug.Log("Player sudah memiliki Access Card!");
            return;
        }

        Sprite iconToUse = hotbarIcon != null ? hotbarIcon : playerInventory.accessCardSprite;
        Vector2? sizeToUse = gunakanUkuranKhusus ? ukuranDiHotbar : (Vector2?)null;

        bool success = playerInventory.AddAccessCard(iconToUse, sizeToUse);

        if (!success)
            return;

        isCollected = true;

        // Sembunyikan ikon E & teks di atas kepala player sebelum objek kartu dinonaktifkan
        if (InteractionIndicator.instance != null)
        {
            InteractionIndicator.instance.HidePrompt();
        }

        gameObject.SetActive(false);

        Debug.Log("ACCESS CARD BERHASIL DIAMBIL!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other) || isCollected)
            return;

        playerNearby = true;

        if (playerInventory == null)
            playerInventory = other.GetComponentInParent<PlayerInventory>();

        if (playerInventory != null && !playerInventory.HasAccessCard)
        {
            // Munculkan ikon E + "Ambil Access Card" di atas kepala player
            if (InteractionIndicator.instance != null)
            {
                InteractionIndicator.instance.ShowPrompt(teksInteraksi, true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerNearby = false;

        // Sembunyikan indikator saat player menjauh dari Access Card
        if (InteractionIndicator.instance != null)
        {
            InteractionIndicator.instance.HidePrompt();
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player") ||
               (other.attachedRigidbody != null &&
                other.attachedRigidbody.CompareTag("Player"));
    }
}