using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventoryItem
{
    public GachaItem gachaItem;
    public int Count;

    public InventoryItem(GachaItem gachaItem, int count)
    {
        this.gachaItem = gachaItem;
        this.Count = count;
    }
}

/// <summary>
/// TODO: pause surroundings when inventory is on
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    private List<InventoryItem> GachaInventory = new List<InventoryItem>();
    private bool IsBeingDisplayed = false;
    
    [SerializeField] private GachaItems allitems;
    [SerializeField] private GameObject inventoryCanvas;
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private PlayerMovement playerMovement;
    
    [SerializeField] private Image[] GachaItemsInventoryImage;
    [SerializeField] private TMP_Text[] GachaItemsInventoryName;
    [SerializeField] private TMP_Text[] GachaItemsInventoryCount;
    [SerializeField] private TMP_Text[] GachaItemsInventoryRank;
    [SerializeField] private Button[] GachaItemsInventoryButtons;

    [SerializeField] private GachaItem firstGacha;

    private void Awake()
    {
        for (int i = 0; i < GachaItemsInventoryImage.Length; i++)
        {
            GachaItemsInventoryImage[i].sprite = allitems.items[i].Icon;
            GachaItemsInventoryImage[i].color =  new Color(GachaItemsInventoryImage[i].color.r, GachaItemsInventoryImage[i].color.g, GachaItemsInventoryImage[i].color.b, 0.4f);
        }

        for (int i = 0; i < GachaItemsInventoryName.Length; i++)
        {
            GachaItemsInventoryName[i].text = allitems.items[i].Name;
        }
        
        for (int i = 0; i < GachaItemsInventoryCount.Length; i++)
        {
            GachaItemsInventoryCount[i].text = "Count: 0";
        }
        for (int i = 0; i < GachaItemsInventoryRank.Length; i++)
        {
            GachaItemsInventoryRank[i].text = allitems.items[i].GachaRank.ToString();
        }
        for (int i = 0; i < GachaItemsInventoryButtons.Length; i++)
        {
            int index = i;

            GachaItemsInventoryButtons[i].onClick.AddListener(
                () => SelectCharacter(index)
            );
        }

        AddToInventory(firstGacha, 1);
        playerMovement.ReplaceModel(firstGacha);
    }
    
    public void AddToInventory(GachaItem item, int count)
    {
        InventoryItem inventoryItem = new InventoryItem(item, count);
        GachaInventory.Add(inventoryItem);
        for (int i = 0; i < GachaItemsInventoryImage.Length; i++)
        {
            if (item.Icon == GachaItemsInventoryImage[i].sprite)
            {
                GachaItemsInventoryImage[i].color = new Color(GachaItemsInventoryImage[i].color.r,
                    GachaItemsInventoryImage[i].color.g, GachaItemsInventoryImage[i].color.b, 1f);
            }
        }
        UpdateDisplay(item);
    }

    public bool CheckForDuplicates(GachaItem item)
    {
        foreach (InventoryItem i in GachaInventory)
        {
            if (item.Name == i.gachaItem.Name)
            {
                i.Count++;
                UpdateDisplay(i.gachaItem);
                return true;
            }
        }

        return false;
    }
    
    public void DebugPrint()
    {
        foreach (InventoryItem i in GachaInventory)
        {
            Debug.Log(i.gachaItem.Name + " " + i.gachaItem.GachaRank + " " + i.Count);
        }
    }

    public void DisplayInventory()
    {
        if (IsBeingDisplayed)
        {
            inventoryCanvas.gameObject.SetActive(false);
            playerCanvas.gameObject.SetActive(true);
            IsBeingDisplayed = false;
        }
        else
        {
            inventoryCanvas.gameObject.SetActive(true);
            playerCanvas.gameObject.SetActive(false);
            IsBeingDisplayed = true;
        }
    }

    public void UpdateDisplay(GachaItem item)
    {
        int index = System.Array.IndexOf(allitems.items, item);

        if (index == -1)
        {
            Debug.LogError("Item not found in allitems: " + item.Name);
            return;
        }

        if (index >= GachaItemsInventoryCount.Length)
        {
            Debug.LogError("UI count array is too small for item index: " + index);
            return;
        }

        InventoryItem inventoryItem = GachaInventory.Find(
            i => i.gachaItem == item
        );

        if (inventoryItem == null)
        {
            Debug.LogError("Item not found in inventory: " + item.Name);
            return;
        }

        GachaItemsInventoryCount[index].text =
            "Count: " + inventoryItem.Count;
    }
    
    private void SelectCharacter(int index)
    {
        if (index < 0 || index >= allitems.items.Length)
        {
            Debug.LogError("Invalid GachaItem index: " + index);
            return;
        }

        GachaItem selectedItem = allitems.items[index];

        InventoryItem inventoryItem = GachaInventory.Find(
            i => i.gachaItem == selectedItem
        );

        if (inventoryItem == null || inventoryItem.Count <= 0)
        {
            Debug.Log("You don't own " + selectedItem.Name);
            return;
        }

        playerMovement.ReplaceModel(selectedItem);

        Debug.Log("Selected character: " + selectedItem.Name);
    }
}
