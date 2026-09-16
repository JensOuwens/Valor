using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventoryItem
{
    public GachaItem GachaItem;
    public int Count;

    public InventoryItem(GachaItem gachaItem, int count)
    {
        this.GachaItem = gachaItem;
        this.Count = count;
    }
}

/// <summary>
/// TODO: update inventory when getting new ones, grey out ones not owned, pause surroundings when inventory is on, button to toggle inventory on/off.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    private List<InventoryItem> GachaInventory = new List<InventoryItem>();
    private bool IsBeingDisplayed = false;
    
    [SerializeField] private GachaItems allitems;
    [SerializeField] private GameObject inventoryCanvas;
    [SerializeField] private Image[] GachaItemsInventoryImage;
    [SerializeField] private TMP_Text[] GachaItemsInventoryName;
    [SerializeField] private TMP_Text[] GachaItemsInventoryCount;

    private void Awake()
    {
        for (int i = 0; i < GachaItemsInventoryImage.Length; i++)
        {
            GachaItemsInventoryImage[i].sprite = allitems.items[i].Icon;
        }

        for (int i = 0; i < GachaItemsInventoryName.Length; i++)
        {
            GachaItemsInventoryName[i].text = allitems.items[i].Name;
        }
        
        for (int i = 0; i < GachaItemsInventoryCount.Length; i++)
        {
            GachaItemsInventoryCount[i].text = "Count: 0";
        }
    }
    
    public void AddToInventory(GachaItem item, int count)
    {
        InventoryItem inventoryItem = new InventoryItem(item, count);
        GachaInventory.Add(inventoryItem);
    }

    public bool CheckForDuplicates(GachaItem item)
    {
        foreach (InventoryItem i in GachaInventory)
        {
            if (item.name == i.GachaItem.name)
            {
                i.Count++;
                return true;
            }
        }

        return false;
    }
    
    public void DebugPrint()
    {
        foreach (InventoryItem i in GachaInventory)
        {
            Debug.Log(i.GachaItem.name + " " + i.GachaItem.GachaRank + " " + i.Count);
        }
    }

    public void DisplayInventory()
    {
        if (IsBeingDisplayed)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }
        else
        {
            inventoryCanvas.gameObject.SetActive(true);
        }
    }

    public void UpdateDisplay()
    {
        
    }
}
