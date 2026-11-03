using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("Status Inventory")]
    public bool hasAccessCard = false;

    [Header("Slot Inventory")]
    public Image slot1Icon;

    [Header("Tampilan Prototipe")]
    public Color accessCardColor = Color.white;
    public Color emptySlotColor = new Color(1f, 1f, 1f, 0.35f);

    void Start()
    {
        UpdateInventoryUI();
    }

    public void AddAccessCard()
    {
        if (hasAccessCard)
            return;

        hasAccessCard = true;
        UpdateInventoryUI();

        Debug.Log("Access Card tersimpan di inventory!");
    }

    void UpdateInventoryUI()
    {
        if (slot1Icon == null)
        {
            Debug.LogWarning("Slot_1 belum dihubungkan!");
            return;
        }

        slot1Icon.enabled = true;

        slot1Icon.color = hasAccessCard
            ? accessCardColor
            : emptySlotColor;

        slot1Icon.sprite = null;
    }
}