using UnityEngine;

[CreateAssetMenu(fileName = "GachaItemsList", menuName = "ScriptableObjects/GachaItemsList", order = 1)]
public class GachaItems : ScriptableObject
{
    public GachaItem[] items;
}
