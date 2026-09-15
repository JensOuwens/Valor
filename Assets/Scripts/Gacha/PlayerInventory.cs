using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO: scriptable objects are keeping lifetime pulls not only plays
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    private List<GachaItem> GachaInventory = new List<GachaItem>();
    
    public void AddToInventory(GachaItem item)
    {
        GachaInventory.Add(item);
    }

    public bool CheckForDuplicates(GachaItem item)
    {
        foreach (GachaItem i in GachaInventory)
        {
            if (item.name == i.name)
            {
                i.Count++;
                return true;
            }
        }

        return false;
    }
    
    public void DebugPrint()
    {
        foreach (GachaItem i in GachaInventory)
        {
            Debug.Log(i.name + " " + i.GachaRank + " " + i.Count);
        }
    }
}
