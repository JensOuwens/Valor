using System;
using UnityEngine;

public class CurrencyScript : MonoBehaviour
{
    [SerializeField] private GachaManager gachaManager;

    public void OnTriggerEnter(Collider other)
    {
        gachaManager.gachaTokens++;
        gachaManager.UpdateTokenDisplay();
        Destroy(this.gameObject);
    }
}
