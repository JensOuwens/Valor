using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// TODO: add ui
/// </summary>
public class GachaManager : MonoBehaviour
{
    [SerializeField] private int pullItemAmount;
    [SerializeField] private int pityMax; 
    private int currentPity;

    [SerializeField] private GachaItems currentBanner;
    [SerializeField] private PlayerInventory playerInventory;

    private void Start()
    {
        GachaPull();
    }
    
    public void GachaPull()
    {
        for (int i = 0; i < pullItemAmount; i++)
        {
            Rank currentRank;
            
            if (currentPity >= pityMax)
            {
                currentRank = Rank.Sr;
                currentPity = 0;
                Debug.Log("PiTYFULL");
            }
            else
            {
                int RandomVal = Random.Range(0, 100);
            
                if (RandomVal <= 80) currentRank = Rank.A;
                else if (RandomVal <= 97) currentRank = Rank.S;
                else currentRank = Rank.Sr;

                if (currentRank != Rank.Sr) currentPity++;
            }

            List<GachaItem> currentGachaRankedItems = new  List<GachaItem>();
            
            foreach (GachaItem gachaItem in currentBanner.items)
            {
                if (gachaItem.GachaRank == currentRank) currentGachaRankedItems.Add(gachaItem);
            }
            
            int randommax = currentGachaRankedItems.Count;
            int itemRanomVal = Random.Range(0, randommax);

            if (!playerInventory.CheckForDuplicates(currentGachaRankedItems[itemRanomVal]))
            {
                playerInventory.AddToInventory(currentGachaRankedItems[itemRanomVal], 1); 
            }
        }
        playerInventory.DebugPrint();
    }
}
