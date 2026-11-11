
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[System.Serializable]
public class InventoryItem
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    public Sprite previewImage;

    public InventoryItem(
        string id,
        string name,
        Sprite itemIcon,
        Sprite preview = null)
    {
        itemID = id;
        itemName = name;
        icon = itemIcon;
        previewImage = preview;
    }
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Status Inventory")]
    public bool hasAccessCard = false;

    [Header("Hotbar Slots")]
    public Image[] slotIcons;

    [Header("Item Icons")]
    public Sprite accessCardSprite;

    [Header("Document Preview UI")]
    public GameObject documentPreviewPanel;
    public Image documentPreviewImage;

    [Header("Interaction UI")]
    public GameObject safePinPanel;

    [Header("Player Movement")]
    public PlayerMovement playerMovement;

    [Header("Inventory Data")]
    public List<InventoryItem> items = new List<InventoryItem>();

    private int selectedSlot = -1;

    public bool HasAccessCard =>
        hasAccessCard || HasItem("access_card");

    public bool HasExitKey =>
        HasItem("exit_key");

    private void Start()
    {
        if (hasAccessCard && !HasItem("access_card"))
        {
            items.Add(new InventoryItem(
                "access_card",
                "Access Card",
                accessCardSprite
            ));
        }

        if (documentPreviewPanel != null)
            documentPreviewPanel.SetActive(false);

        UpdateInventoryUI();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Jangan memilih item ketika keypad PIN terbuka
        if (safePinPanel != null && safePinPanel.activeSelf)
            return;

        // Pilih slot Hotbar 1-4
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectSlot(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectSlot(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectSlot(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            SelectSlot(3);

        // Tutup preview dengan Escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseDocumentPreview();
    }

    public bool AddItem(string id, string itemName, Sprite icon)
    {
        return AddItemInternal(id, itemName, icon, null);
    }

    public bool AddDocumentItem(
        string id,
        string itemName,
        Sprite icon,
        Sprite preview)
    {
        return AddItemInternal(id, itemName, icon, preview);
    }

    private bool AddItemInternal(
        string id,
        string itemName,
        Sprite icon,
        Sprite preview)
    {
        if (HasItem(id))
        {
            Debug.Log("Item sudah dimiliki: " + itemName);
            return false;
        }

        if (slotIcons == null || slotIcons.Length == 0)
        {
            Debug.LogWarning("Slot Icons belum diatur!");
            return false;
        }

        if (items.Count >= slotIcons.Length)
        {
            Debug.LogWarning("Inventory penuh!");
            return false;
        }

        items.Add(new InventoryItem(
            id,
            itemName,
            icon,
            preview
        ));

        if (id == "access_card")
            hasAccessCard = true;

        UpdateInventoryUI();

        Debug.Log("Item berhasil diambil: " + itemName);
        return true;
    }

    public bool AddAccessCard(Sprite icon)
    {
        return AddItem("access_card", "Access Card", icon);
    }

    // Kompatibilitas dengan script Access Card lama
    public void AddAccessCard()
    {
        AddAccessCard(accessCardSprite);
    }

    public bool AddExitKey(Sprite icon)
    {
        return AddItem("exit_key", "Exit Key", icon);
    }

    public bool HasItem(string id)
    {
        return items.Exists(item => item.itemID == id);
    }

    
    public void SelectSlot(int index)
    {
        if (slotIcons == null ||
            index < 0 ||
            index >= slotIcons.Length)
            return;

        if (index >= items.Count)
        {
            CloseDocumentPreview();
            return;
        }

        InventoryItem item = items[index];

        Debug.Log("Memilih item: " + item.itemName);

        if (item.previewImage == null)
        {
            Debug.LogWarning(
                "Preview Image kosong untuk item: " + item.itemName
            );

            CloseDocumentPreview();
            return;
        }

        if (documentPreviewPanel == null)
        {
            Debug.LogWarning("DocumentPreviewPanel belum dihubungkan!");
            return;
        }

        if (documentPreviewImage == null)
        {
            Debug.LogWarning("DocumentPreviewImage belum dihubungkan!");
            return;
        }

        documentPreviewImage.sprite = item.previewImage;
        documentPreviewImage.preserveAspect = true;

        documentPreviewPanel.SetActive(true);

        if (playerMovement != null)
            playerMovement.StopMovement();

        Debug.Log("PREVIEW DOKUMEN BERHASIL DIBUKA!");
    }


    public void CloseDocumentPreview()
    {
        bool previewWasOpen =
            documentPreviewPanel != null &&
            documentPreviewPanel.activeSelf;

        selectedSlot = -1;

        if (documentPreviewPanel != null)
            documentPreviewPanel.SetActive(false);

        // Lanjutkan gerakan hanya jika preview sebelumnya terbuka
        if (previewWasOpen && playerMovement != null)
            playerMovement.ResumeMovement();
    }

    public void UpdateInventoryUI()
    {
        if (slotIcons == null)
            return;

        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] == null)
                continue;

            if (i < items.Count)
            {
                slotIcons[i].sprite = items[i].icon;
                slotIcons[i].enabled = items[i].icon != null;
                slotIcons[i].color = Color.white;
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].enabled = false;
            }
        }
    }
}
