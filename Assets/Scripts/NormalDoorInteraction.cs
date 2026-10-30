
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
        if (isOpened || !playerNearby)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        if (doorVisual == null || doorBarrier == null)
        {
            Debug.LogWarning(
                "Hubungkan Door Visual dan Door Barrier di Inspector!"
            );
            return;
        }

        isOpened = true;

        // Hilangkan gambar pintu.
        SpriteRenderer sprite =
            doorVisual.GetComponent<SpriteRenderer>();

        if (sprite != null)
            sprite.enabled = false;

        // Hilangkan penghalang fisik pintu.
        Collider2D barrierCollider =
            doorBarrier.GetComponent<Collider2D>();

        if (barrierCollider != null)
            barrierCollider.enabled = false;

        Debug.Log("Pintu terbuka! Player bisa masuk.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Tekan E untuk membuka pintu.");
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
