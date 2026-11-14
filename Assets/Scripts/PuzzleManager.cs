using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public PuzzleTile[] tiles;
    public GameObject puzzlePanel;
    public DoorInteraction doorInteraction;
    public KeypadInteraction keypadInteraction;

    [Header("Player Settings")]
    public PlayerMovement playerMovement;

    [Header("Hotbar Settings")]
    public GameObject hotbarPanel;

    [Header("Notification")]
    public TMP_Text notificationText;
    public float notificationDuration = 2f;

    [Header("Notifikasi Kepala Player")]
    public string pesanKunciTerbuka = "Kunci Berhasil Dibuka";
    public float durasiNotifikasiKepala = 2f;

    private bool puzzleSolved = false;
    private Coroutine notificationCoroutine;

    void Start()
    {
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }

    // ==============================
    // MEMBUKA PUZZLE
    // ==============================

    public void OpenPuzzle()
    {
        if (puzzlePanel == null)
        {
            Debug.LogWarning("PuzzlePanel belum dihubungkan!");
            return;
        }

        if (puzzlePanel.activeSelf)
            return;

        puzzleSolved = false;

        HideNotification();
        ShufflePuzzle();

        // Tampilkan puzzle
        puzzlePanel.SetActive(true);

        // Sembunyikan Hotbar
        if (hotbarPanel != null)
            hotbarPanel.SetActive(false);

        // Hentikan pergerakan karakter
        if (playerMovement != null)
            playerMovement.StopMovement();

        Debug.Log("PUZZLE DIBUKA!");
        Debug.Log("HOTBAR DISEMBUNYIKAN!");
        Debug.Log("PLAYER BERHENTI BERGERAK!");
    }

    // ==============================
    // INPUT KEYBOARD
    // ==============================

    void Update()
    {
        if (puzzlePanel == null ||
            !puzzlePanel.activeSelf)
            return;

        if (Keyboard.current == null)
            return;

        // Tombol X untuk membatalkan puzzle
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            CancelPuzzle();
            return;
        }

        // Tombol Enter untuk konfirmasi
        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            ConfirmPuzzle();
        }
    }

    // ==============================
    // MEMBATALKAN PUZZLE
    // ==============================

    public void CancelPuzzle()
    {
        HideNotification();

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        RestorePlayerControl();

        // Reset interaksi keypad karena puzzle dibatalkan
        KeypadInteraction keypad = keypadInteraction != null
            ? keypadInteraction
            : FindFirstObjectByType<KeypadInteraction>();

        if (keypad != null)
            keypad.ResetInteraction();

        Debug.Log("PUZZLE DIBATALKAN!");
        Debug.Log("HOTBAR KEMBALI!");
        Debug.Log("PLAYER BISA BERGERAK!");
    }

    // ==============================
    // KONFIRMASI PUZZLE
    // ==============================

    public void ConfirmPuzzle()
    {
        if (puzzleSolved)
            return;

        if (tiles == null || tiles.Length != 9)
        {
            Debug.LogWarning("Puzzle harus memiliki 9 tile!");
            return;
        }

        // Periksa susunan angka
        for (int i = 0; i < tiles.Length; i++)
        {
            int expectedNumber = (i == 8) ? 0 : i + 1;

            if (tiles[i] == null ||
                tiles[i].tileNumber != expectedNumber)
            {
                ShowNotification("Kode Salah!");

                Debug.Log("KODE SALAH!");

                // Puzzle tetap terbuka
                return;
            }
        }

        // Puzzle berhasil
        puzzleSolved = true;

        HideNotification();

        Debug.Log("PUZZLE BERHASIL DIKONFIRMASI!");

        // Buka kunci pintu
        if (doorInteraction != null)
        {
            doorInteraction.UnlockDoor();
        }
        else
        {
            Debug.LogWarning(
                "DoorInteraction belum dihubungkan!"
            );

            puzzleSolved = false;
            return;
        }

        // Tutup puzzle, matikan prompt keypad, dan munculkan teks "Kunci Berhasil Dibuka"
        ClosePuzzleSuccess();

        Debug.Log("PINTU BERHASIL DIBUKA!");
    }

    // ==============================
    // MENUTUP PUZZLE SETELAH BERHASIL
    // ==============================

    void ClosePuzzleSuccess()
    {
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        RestorePlayerControl();

        // Tandai interaksi keypad selesai agar teks "Buka Kunci" tidak muncul lagi
        KeypadInteraction keypad = keypadInteraction != null
            ? keypadInteraction
            : FindFirstObjectByType<KeypadInteraction>();

        if (keypad != null)
            keypad.CompleteInteraction();

        // Tampilkan pemberitahuan hijau "Kunci Berhasil Dibuka" di atas kepala player
        if (InteractionIndicator.instance != null)
        {
            InteractionIndicator.instance.ShowSuccess(
                pesanKunciTerbuka,
                durasiNotifikasiKepala
            );
        }
    }

    // ==============================
    // MENGEMBALIKAN KONTROL PLAYER
    // ==============================

    void RestorePlayerControl()
    {
        // Tampilkan kembali Hotbar
        if (hotbarPanel != null)
            hotbarPanel.SetActive(true);

        // Aktifkan kembali pergerakan Player
        if (playerMovement != null)
            playerMovement.ResumeMovement();
    }

    // ==============================
    // MENGACAK PUZZLE
    // ==============================

    void ShufflePuzzle()
    {
        if (tiles == null || tiles.Length != 9)
        {
            Debug.LogWarning(
                "Isi Tiles dengan 9 objek PuzzleTile!"
            );

            return;
        }

        // Susunan awal puzzle
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
                return;

            tiles[i].tileNumber = (i == 8) ? 0 : i + 1;

            tiles[i].UpdateNumber();
        }

        int emptyIndex = 8;

        // Pengacakan dengan perpindahan valid
        for (int i = 0; i < 100; i++)
        {
            int row = emptyIndex / 3;
            int col = emptyIndex % 3;

            int[] candidates = new int[4];
            int count = 0;

            if (row > 0)
                candidates[count++] = emptyIndex - 3;

            if (row < 2)
                candidates[count++] = emptyIndex + 3;

            if (col > 0)
                candidates[count++] = emptyIndex - 1;

            if (col < 2)
                candidates[count++] = emptyIndex + 1;

            int swapIndex =
                candidates[Random.Range(0, count)];

            int temp = tiles[emptyIndex].tileNumber;

            tiles[emptyIndex].tileNumber =
                tiles[swapIndex].tileNumber;

            tiles[swapIndex].tileNumber = temp;

            tiles[emptyIndex].UpdateNumber();
            tiles[swapIndex].UpdateNumber();

            emptyIndex = swapIndex;
        }
    }

    // ==============================
    // MEMINDAHKAN TILE
    // ==============================

    public void MoveTile(PuzzleTile clickedTile)
    {
        if (puzzleSolved || clickedTile == null)
            return;

        int clickedIndex =
            System.Array.IndexOf(tiles, clickedTile);

        if (clickedIndex < 0)
            return;

        int emptyIndex = -1;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null &&
                tiles[i].tileNumber == 0)
            {
                emptyIndex = i;
                break;
            }
        }

        if (emptyIndex < 0 ||
            clickedTile.tileNumber == 0)
            return;

        int row = clickedIndex / 3;
        int col = clickedIndex % 3;

        int emptyRow = emptyIndex / 3;
        int emptyCol = emptyIndex % 3;

        int distance =
            Mathf.Abs(row - emptyRow) +
            Mathf.Abs(col - emptyCol);

        if (distance != 1)
            return;

        int temp = clickedTile.tileNumber;

        clickedTile.tileNumber = 0;

        tiles[emptyIndex].tileNumber = temp;

        clickedTile.UpdateNumber();
        tiles[emptyIndex].UpdateNumber();

        HideNotification();
    }

    // ==============================
    // NOTIFIKASI
    // ==============================

    void ShowNotification(string message)
    {
        if (notificationText == null)
        {
            Debug.LogWarning(
                "Notification Text belum dihubungkan: " + message
            );

            return;
        }

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

    void HideNotification()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }
}