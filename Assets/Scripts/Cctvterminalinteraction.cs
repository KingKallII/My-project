using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CCTVTerminalInteraction : MonoBehaviour
{
    [Header("Referensi Minigame Labirin CCTV")]
    [Tooltip("Masukkan objek yang memiliki skrip CCTVMazeMinigameManager (atau kosongkan agar dicari otomatis).")]
    public CCTVMazeMinigameManager mazeManager;

    [Header("UI Petunjuk Interaksi (Opsional)")]
    public GameObject interactionPrompt;
    public TMP_Text promptText;

    private bool playerNearby = false;
    private GameObject nearbyPlayer;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (mazeManager == null)
            mazeManager = FindFirstObjectByType<CCTVMazeMinigameManager>();
    }

    void Update()
    {
        if (!playerNearby || nearbyPlayer == null)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (mazeManager == null)
            {
                mazeManager = CCTVMazeMinigameManager.Instance != null
                    ? CCTVMazeMinigameManager.Instance
                    : FindFirstObjectByType<CCTVMazeMinigameManager>();
            }

            if (mazeManager != null)
            {
                if (mazeManager.AreAllCCTVsDisabled())
                {
                    Debug.Log("Seluruh CCTV sudah nonaktif.");
                    return;
                }

                if (interactionPrompt != null)
                    interactionPrompt.SetActive(false);

                mazeManager.OpenComputerMaze(nearbyPlayer);
            }
            else
            {
                Debug.LogWarning("CCTVMazeMinigameManager belum dipasang di scene!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;
        nearbyPlayer = other.gameObject;

        if (mazeManager != null && mazeManager.AreAllCCTVsDisabled())
            return;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);

        if (promptText != null)
            promptText.text = "Tekan E untuk meretas komputer CCTV";

        Debug.Log("Tekan E untuk membuka komputer CCTV.");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;
        nearbyPlayer = null;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
}