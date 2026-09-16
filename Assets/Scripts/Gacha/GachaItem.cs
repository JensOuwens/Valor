using UnityEngine;

public enum Rank
{
    A,
    S,
    Sr
}

[CreateAssetMenu(fileName = "GachaItem", menuName = "ScriptableObjects/GachaItem", order = 1)]
public class GachaItem : ScriptableObject
{
    public string Name;
    public Rank GachaRank;
    public Sprite Icon;
    public GameObject Model;
}
