using UnityEngine;
public class GachaManager : MonoBehaviour
{
    [SerializeField] private int pullItemAmount;
    [SerializeField] private float SRankPercentage;
    [SerializeField] private float SRrankPercentage;
    [SerializeField] private int pityMax;
    [SerializeField] private int currentPity;

    [SerializeField] private GachaItems currentBanner;
    
    public void GachaPull()
    {
        for (int i = 0; i < pullItemAmount; i++)
        {
            
        }
    }
}
