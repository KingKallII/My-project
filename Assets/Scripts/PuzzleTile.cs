
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleTile : MonoBehaviour
{
    [Header("Tile Settings")]
    public int tileNumber;

    [Header("References")]
    public Button button;
    public TextMeshProUGUI numberText;
    public PuzzleManager puzzleManager;

    private void Awake()
    {
        // Ambil Button otomatis jika belum dihubungkan
        if (button == null)
            button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // Pasang listener setiap Tile diaktifkan
        if (button != null)
        {
            button.onClick.RemoveListener(OnTileClicked);
            button.onClick.AddListener(OnTileClicked);
        }
        else
        {
            Debug.LogError(
                "Button tidak ditemukan pada " + gameObject.name
            );
        }

        UpdateNumber();
    }

    private void OnDisable()
    {
        // Lepaskan listener ketika Tile dinonaktifkan
        if (button != null)
            button.onClick.RemoveListener(OnTileClicked);
    }

    public void UpdateNumber()
    {
        if (numberText != null)
        {
            numberText.text =
                tileNumber == 0 ? "" : tileNumber.ToString();
        }
    }

    private void OnTileClicked()
    {
        Debug.Log("TILE DIKLIK: " + gameObject.name);

        if (puzzleManager != null)
        {
            puzzleManager.MoveTile(this);
        }
        else
        {
            Debug.LogError(
                "PuzzleManager belum terhubung pada " +
                gameObject.name
            );
        }
    }
}
