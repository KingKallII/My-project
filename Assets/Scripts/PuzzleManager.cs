
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

    [Header("Notification")]
    public TMP_Text notificationText;
    public float notificationDuration = 2f;

    private bool puzzleSolved = false;
    private Coroutine notificationCoroutine;

    void Start()
    {
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }

    public void OpenPuzzle()
    {
        if (puzzlePanel == null)
        {
            Debug.LogWarning("PuzzlePanel belum dihubungkan!");
            return;
        }

        puzzleSolved = false;
        HideNotification();
        ShufflePuzzle();
        puzzlePanel.SetActive(true);

        Debug.Log("PUZZLE DIBUKA!");
    }

    void Update()
    {
        if (puzzlePanel == null || !puzzlePanel.activeSelf)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.xKey.wasPressedThisFrame)
            CancelPuzzle();

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            ConfirmPuzzle();
    }

    public void CancelPuzzle()
    {
        HideNotification();

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        Debug.Log("PUZZLE DIBATALKAN!");
    }

    public void ConfirmPuzzle()
    {
        if (puzzleSolved)
            return;

        if (tiles == null || tiles.Length != 9)
        {
            Debug.LogWarning("Puzzle harus memiliki 9 tile!");
            return;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            int expectedNumber = (i == 8) ? 0 : i + 1;

            if (tiles[i] == null ||
                tiles[i].tileNumber != expectedNumber)
            {
                ShowNotification("Kode Salah!");
                Debug.Log("KODE SALAH!");
                return;
            }
        }

        puzzleSolved = true;
        HideNotification();

        Debug.Log("PUZZLE BERHASIL DIKONFIRMASI!");

        if (doorInteraction != null)
        {
            doorInteraction.UnlockDoor();
        }
        else
        {
            Debug.LogWarning(
                "DoorInteraction belum dihubungkan pada PuzzleManager!"
            );
            puzzleSolved = false;
            return;
        }

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);
    }

    void ShufflePuzzle()
    {
        if (tiles == null || tiles.Length != 9)
        {
            Debug.LogWarning("Isi Tiles dengan 9 objek PuzzleTile!");
            return;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
                return;

            tiles[i].tileNumber = (i == 8) ? 0 : i + 1;
            tiles[i].UpdateNumber();
        }

        int emptyIndex = 8;

        for (int i = 0; i < 100; i++)
        {
            int row = emptyIndex / 3;
            int col = emptyIndex % 3;

            int[] candidates = new int[4];
            int count = 0;

            if (row > 0) candidates[count++] = emptyIndex - 3;
            if (row < 2) candidates[count++] = emptyIndex + 3;
            if (col > 0) candidates[count++] = emptyIndex - 1;
            if (col < 2) candidates[count++] = emptyIndex + 1;

            int swapIndex = candidates[Random.Range(0, count)];

            int temp = tiles[emptyIndex].tileNumber;
            tiles[emptyIndex].tileNumber = tiles[swapIndex].tileNumber;
            tiles[swapIndex].tileNumber = temp;

            tiles[emptyIndex].UpdateNumber();
            tiles[swapIndex].UpdateNumber();

            emptyIndex = swapIndex;
        }
    }

    public void MoveTile(PuzzleTile clickedTile)
    {
        if (puzzleSolved || clickedTile == null)
            return;

        int clickedIndex = System.Array.IndexOf(tiles, clickedTile);

        if (clickedIndex < 0)
            return;

        int emptyIndex = -1;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null && tiles[i].tileNumber == 0)
            {
                emptyIndex = i;
                break;
            }
        }

        if (emptyIndex < 0 || clickedTile.tileNumber == 0)
            return;

        int row = clickedIndex / 3;
        int col = clickedIndex % 3;
        int emptyRow = emptyIndex / 3;
        int emptyCol = emptyIndex % 3;

        int distance = Mathf.Abs(row - emptyRow)
                     + Mathf.Abs(col - emptyCol);

        if (distance != 1)
            return;

        int temp = clickedTile.tileNumber;
        clickedTile.tileNumber = 0;
        tiles[emptyIndex].tileNumber = temp;

        clickedTile.UpdateNumber();
        tiles[emptyIndex].UpdateNumber();

        HideNotification();
    }

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
