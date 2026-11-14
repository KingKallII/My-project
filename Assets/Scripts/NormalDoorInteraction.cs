using UnityEngine;
using UnityEngine.InputSystem;

public class NormalDoorInteraction : MonoBehaviour
{
    [Header("Referensi Pintu")]
    public GameObject doorVisual;
    public GameObject doorBarrier;

    [Header("Pengaturan Notifikasi Kepala Player")]
    public string teksBukaPintu = "Buka Pintu";
    public string teksTutupPintu = "Tutup Pintu";

    private bool playerNearby = false;
    private bool isOpened = false;

    void Update()
    {
        if (!playerNearby)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        if (doorVisual == null || doorBarrier == null)
        {
            Debug.LogWarning(
                "Hubungkan Door Visual dan Door Barrier di Inspector!"
            );
            return;
        }

        // Ubah status pintu
        isOpened = !isOpened;

        // Atur visual pintu
        SpriteRenderer sprite =
            doorVisual.GetComponent<SpriteRenderer>();

        if (sprite != null)
            sprite.enabled = !isOpened;

        // Atur penghalang fisik pintu
        Collider2D barrierCollider =
            doorBarrier.GetComponent<Collider2D>();

        if (barrierCollider != null)
            barrierCollider.enabled = !isOpened;

        // Perbarui teks indikator di atas kepala player secara langsung
        UpdateDoorPrompt();

        if (isOpened)
        {
            Debug.Log("Pintu terbuka! Player bisa masuk.");
        }
        else
        {
            Debug.Log("Pintu tertutup!");
        }
    }

    void UpdateDoorPrompt()
    {
        if (InteractionIndicator.instance == null)
            return;

        string pesan = isOpened ? teksTutupPintu : teksBukaPintu;
        InteractionIndicator.instance.ShowPrompt(pesan);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = true;
            UpdateDoorPrompt();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = false;

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