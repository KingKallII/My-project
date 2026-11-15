
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleTile : MonoBehaviour
{
    public int tileNumber;
    public Button button;
    public TextMeshProUGUI numberText;
    public PuzzleManager puzzleManager;

    void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnTileClicked);

        UpdateNumber();
    }

    public void UpdateNumber()
    {
        if (numberText != null)
        {
            if (tileNumber == 0)
                numberText.text = "";
            else
                numberText.text = tileNumber.ToString();
        }
    }

    void OnTileClicked()
    {
        if (puzzleManager != null)
            puzzleManager.MoveTile(this);
    }
}
