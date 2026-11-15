
using UnityEngine;
using UnityEngine.InputSystem;

public class CCTVRoomDoorInteraction : MonoBehaviour
{
    [Header("Referensi Barrier")]
    public Collider2D doorBarrier;

    [Header("Referensi UI PIN CCTV")]
    public GameObject pinPanel;
    public CCTVPinManager pinManager;

    private bool playerNearby = false;
    private bool isUnlocked = false;

    void Start()
    {
        if (doorBarrier != null)
            doorBarrier.enabled = true;

        if (pinPanel != null)
            pinPanel.SetActive(false);

        Debug.Log("CCTV Door Interaction siap.");
    }

    void Update()
    {
        if (!playerNearby || isUnlocked)
            return;

        if (pinPanel != null && pinPanel.activeSelf)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Tombol E terdeteksi!");

            if (pinPanel == null)
            {
                Debug.LogError("Pin Panel belum dihubungkan!");
                return;
            }

            if (pinManager == null)
            {
                Debug.LogError("CCTVPinManager belum dihubungkan!");
                return;
            }

            pinManager.OpenPinPanel();
        }
    }

    public void UnlockDoor()
    {
        isUnlocked = true;

        if (doorBarrier != null)
            doorBarrier.enabled = false;

        Debug.Log("Barrier CCTV berhasil dinonaktifkan!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("PLAYER MENDEKATI PINTU CCTV!");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("PLAYER MENJAUHI PINTU CCTV!");
        }
    }
}
