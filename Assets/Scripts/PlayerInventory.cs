
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
            Debug.LogWarning(
                "Slot_1 belum dihubungkan ke PlayerInventory."
            );
            return;
        }

        // Image Slot 1 tetap aktif, baik kosong maupun terisi.
        slot1Icon.enabled = true;

        // Bedakan tampilan slot kosong dan slot berisi kartu.
        slot1Icon.color = hasAccessCard
            ? accessCardColor
            : emptySlotColor;

        // Prototipe masih berupa kotak, tanpa gambar kartu.
        slot1Icon.sprite = null;
    }
}
