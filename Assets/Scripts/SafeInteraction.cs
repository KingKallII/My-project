using UnityEngine;
using UnityEngine.InputSystem;

public class SafeInteraction : MonoBehaviour
{
    [Header("Referensi Brankas")]
    public Collider2D safeBarrier;

    [Header("Visual Brankas")]
    public GameObject safeClosedVisual;
    public GameObject safeOpenVisual;

    [Header("Referensi UI PIN")]
    public GameObject safePinPanel;
    public SafePinManager safePinManager;

    [Header("Referensi Exit Key")]
    public GameObject exitKeyItem;

    [Header("Pengaturan Notifikasi Kepala Player")]
    public string teksBukaBrankas = "Buka Brankas";

    private bool playerNearby = false;
    private bool isUnlocked = false;
    private bool wasPinPanelOpen = false;

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

        // Exit Key belum muncul sebelum brankas dibuka
        if (exitKeyItem != null)
            exitKeyItem.SetActive(false);
    }

    void Update()
    {
        if (!playerNearby || isUnlocked)
            return;

        bool isPinPanelCurrentlyOpen = safePinPanel != null && safePinPanel.activeSelf;

        // Jika pemain menutup panel PIN (misal tekan Escape/Close) dan brankas masih terkunci, munculkan lagi prompt E
        if (wasPinPanelOpen && !isPinPanelCurrentlyOpen)
        {
            if (InteractionIndicator.instance != null)
                InteractionIndicator.instance.ShowPrompt(teksBukaBrankas);
        }
        wasPinPanelOpen = isPinPanelCurrentlyOpen;

        // Jangan membuka panel berulang kali
        if (isPinPanelCurrentlyOpen)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Pemain berinteraksi dengan brankas!");

            if (safePinManager != null)
            {
                // Sembunyikan indikator di atas kepala saat layar keypad PIN muncul
                if (InteractionIndicator.instance != null)
                    InteractionIndicator.instance.HidePrompt();

                safePinManager.OpenPinPanel();
                wasPinPanelOpen = true;
            }
            else
            {
                Debug.LogWarning("SafePinManager belum dihubungkan!");
            }
        }
    }

    // Dipanggil ketika PIN benar
    public void UnlockSafe()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;

        // Pastikan prompt "Buka Brankas" disembunyikan saat brankas terbuka
        if (InteractionIndicator.instance != null)
            InteractionIndicator.instance.HidePrompt();

        // Hilangkan penghalang brankas
        if (safeBarrier != null)
            safeBarrier.enabled = false;

        // Ganti visual brankas
        if (safeClosedVisual != null)
            safeClosedVisual.SetActive(false);

        if (safeOpenVisual != null)
            safeOpenVisual.SetActive(true);

        // Munculkan Exit Key
        if (exitKeyItem != null)
            exitKeyItem.SetActive(true);

        Debug.Log("BRANKAS BERHASIL DIBUKA!");
        Debug.Log("EXIT KEY MUNCUL!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = true;

            if (!isUnlocked)
            {
                if (InteractionIndicator.instance != null)
                    InteractionIndicator.instance.ShowPrompt(teksBukaBrankas);

                Debug.Log("Tekan E untuk membuka brankas.");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = false;
            wasPinPanelOpen = false;

            if (InteractionIndicator.instance != null)
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