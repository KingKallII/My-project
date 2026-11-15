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
    public Vector2 iconSize;

    public InventoryItem(
        string id,
        string name,
        Sprite itemIcon,
        Sprite preview = null,
        Vector2? customSize = null)
    {
        itemID = id;
        itemName = name;
        icon = itemIcon;
        previewImage = preview;
        iconSize = customSize ?? new Vector2(120f, 120f);
    }
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Status Inventory")]
    public bool hasAccessCard = false;

    [Header("Hotbar Slots")]
    public Image[] slotIcons;

    [Header("Access Card Default Settings")]
    public Sprite accessCardSprite;
    public Vector2 accessCardHotbarSize = new Vector2(140f, 85f);
    public Vector2 defaultHotbarSize = new Vector2(120f, 120f);

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
                accessCardSprite,
                null,
                accessCardHotbarSize
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

    public bool AddItem(string id, string itemName, Sprite icon, Vector2? customSize = null)
    {
        return AddItemInternal(id, itemName, icon, null, customSize);
    }

    public bool AddDocumentItem(
        string id,
        string itemName,
        Sprite icon,
        Sprite preview,
        Vector2? customSize = null)
    {
        return AddItemInternal(id, itemName, icon, preview, customSize);
    }

    private bool AddItemInternal(
        string id,
        string itemName,
        Sprite icon,
        Sprite preview,
        Vector2? customSize = null)
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

        Vector2 targetSize = customSize ?? defaultHotbarSize;
        if (!customSize.HasValue && id == "access_card")
        {
            targetSize = accessCardHotbarSize;
        }

        items.Add(new InventoryItem(
            id,
            itemName,
            icon,
            preview,
            targetSize
        ));

        if (id == "access_card")
            hasAccessCard = true;

        UpdateInventoryUI();

        Debug.Log("Item berhasil diambil: " + itemName);
        return true;
    }

    public bool AddAccessCard(Sprite icon, Vector2? customSize = null)
    {
        return AddItem("access_card", "Access Card", icon, customSize ?? accessCardHotbarSize);
    }

    // Kompatibilitas dengan script Access Card lama
    public void AddAccessCard()
    {
        AddAccessCard(accessCardSprite, accessCardHotbarSize);
    }

    public bool AddExitKey(Sprite icon, Vector2? customSize = null)
    {
        return AddItem("exit_key", "Exit Key", icon, customSize);
    }

    public bool HasItem(string id)
    {
        return items.Exists(item => item.itemID == id);
    }

    public void SelectSlot(int index)
    {
        if (slotIcons == null || index < 0 || index >= slotIcons.Length)
            return;

        // Jika slot yang dipilih kosong, tutup preview
        if (index >= items.Count)
        {
            CloseDocumentPreview();
            return;
        }

        InventoryItem item = items[index];

        // Jika item BUKAN dokumen (tidak memiliki previewImage), jangan buka DocumentPreviewPanel
        if (item.previewImage == null)
        {
            CloseDocumentPreview();
            selectedSlot = index;
            Debug.Log("Memilih item non-dokumen: " + item.itemName);
            return;
        }

        // Toggle: jika menekan angka slot dokumen yang sama saat preview terbuka, tutup preview
        if (selectedSlot == index && documentPreviewPanel != null && documentPreviewPanel.activeSelf)
        {
            CloseDocumentPreview();
            return;
        }

        if (documentPreviewPanel == null || documentPreviewImage == null)
        {
            Debug.LogWarning("DocumentPreviewPanel atau DocumentPreviewImage belum dihubungkan!");
            return;
        }

        selectedSlot = index;

        documentPreviewImage.sprite = item.previewImage;
        documentPreviewImage.type = Image.Type.Simple;
        documentPreviewImage.preserveAspect = true;
        documentPreviewImage.color = Color.white;

        documentPreviewPanel.SetActive(true);

        if (playerMovement != null)
            playerMovement.StopMovement();

        Debug.Log("Membuka preview dokumen: " + item.itemName);
    }

    public void CloseDocumentPreview()
    {
        bool previewWasOpen =
            documentPreviewPanel != null &&
            documentPreviewPanel.activeSelf;

        selectedSlot = -1;

        if (documentPreviewPanel != null)
            documentPreviewPanel.SetActive(false);

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
                slotIcons[i].gameObject.SetActive(true);
                slotIcons[i].sprite = items[i].icon;
                slotIcons[i].enabled = items[i].icon != null;
                slotIcons[i].color = Color.white;
                slotIcons[i].type = Image.Type.Simple;
                slotIcons[i].preserveAspect = true;

                RectTransform rt = slotIcons[i].GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = items[i].iconSize;
                }
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].enabled = false;
            }
        }
    }
}