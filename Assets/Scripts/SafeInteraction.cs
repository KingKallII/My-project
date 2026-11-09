
using UnityEngine;
using UnityEngine.InputSystem;

public class SafeInteraction : MonoBehaviour
{
    [Header("Referensi Brankas")]
    public Collider2D safeBarrier;

    [Header("Visual Brankas (Opsional)")]
    public GameObject safeClosedVisual;
    public GameObject safeOpenVisual;

    [Header("Referensi UI PIN")]
    public GameObject safePinPanel;
    public SafePinManager safePinManager;

    private bool playerNearby = false;
    private bool isUnlocked = false;

    void Start()
    {
        // Brankas terkunci pada awal permainan
        if (safeBarrier != null)
            safeBarrier.enabled = true;

        if (safePinPanel != null)
            safePinPanel.SetActive(false);

        if (safeOpenVisual != null)
            safeOpenVisual.SetActive(false);

        if (safeClosedVisual != null)
            safeClosedVisual.SetActive(true);
    }

    void Update()
    {
        if (!playerNearby || isUnlocked)
            return;

        // Jangan membuka panel berulang kali
        if (safePinPanel != null && safePinPanel.activeSelf)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Pemain berinteraksi dengan brankas!");

            if (safePinManager != null)
            {
                safePinManager.OpenPinPanel();
            }
            else
            {
                Debug.LogWarning(
                    "SafePinManager belum dihubungkan!"
                );
            }
        }
    }

    // Dipanggil ketika PIN benar
    public void UnlockSafe()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;

        // Hilangkan penghalang brankas
        if (safeBarrier != null)
            safeBarrier.enabled = false;

        // Ganti visual jika sprite terbuka tersedia
        if (safeClosedVisual != null)
            safeClosedVisual.SetActive(false);

        if (safeOpenVisual != null)
            safeOpenVisual.SetActive(true);

        Debug.Log("BRANKAS BERHASIL DIBUKA!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (!isUnlocked)
                Debug.Log("Tekan E untuk membuka brankas.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
