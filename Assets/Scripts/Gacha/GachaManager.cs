using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private bool IsBeingDisplayed = false;
    [SerializeField] private GameObject gachaCanvas;

    [SerializeField] private Image[] gachaImages;
    [SerializeField] private TMP_Text[] gachaNameTexts;
    [SerializeField] private TMP_Text[] gacharankTexts;

    private void Start()
    {
        GachaPull();
    }
    
    public void GachaPull()
    {
        List<GachaItem> currentPulls = new List<GachaItem>();
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
            
            currentPulls.Add(currentGachaRankedItems[itemRanomVal]);
        }

        DisplayPulls(currentPulls);
    }

    public void DisplayPulls(List<GachaItem> item)
    {
        for (int i = 0; i < gachaImages.Length; i++)
        {
            gachaImages[i].sprite = item[i].Icon;
            gachaNameTexts[i].text = item[i].Name;
            gacharankTexts[i].text = item[i].GachaRank.ToString();
        }
    }
    
    public void DisplayGacha()
    {
        if (IsBeingDisplayed)
        {
            gachaCanvas.gameObject.SetActive(false);
        }
        else
        {
            gachaCanvas.gameObject.SetActive(true);
        }
    }
}
