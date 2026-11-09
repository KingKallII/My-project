
using UnityEngine;
using UnityEngine.InputSystem;

public class NormalDoorInteraction : MonoBehaviour
{
    [Header("Referensi Pintu")]
    public GameObject doorVisual;
    public GameObject doorBarrier;

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

        if (isOpened)
        {
            Debug.Log("Pintu terbuka! Player bisa masuk.");
        }
        else
        {
            Debug.Log("Pintu tertutup!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Tekan E untuk membuka/menutup pintu.");
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
