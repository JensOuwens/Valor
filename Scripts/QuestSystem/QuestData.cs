namespace Valor;

using Godot;
using System;

/// <summary>
/// scripatble object that holds all data types the quest needs
/// </summary>
[GlobalClass]
public partial class QuestData : Resource
{
	[Export] public string quest_Name;
	[Export] public ItemData[] QuestItems;
	[Export] public QuestRewardTypes questrewardType;
	[Export] public ItemData[] RewardItems;
	//something for unlocking area's
	[Export] public string startDialouge;
	[Export] public string duringDialouge;
	[Export] public string endDialouge;
	
	public QuestData() : this(null, null, QuestRewardTypes.Item, null, null, null, null) {}

	public QuestData(string questName, ItemData[] questItems, QuestRewardTypes questrewardType,  
		ItemData[] rewardItems, string startDialouge, string duringDialouge, string endDialouge)
	{
		this.quest_Name = questName;
		this.QuestItems = questItems;
		this.questrewardType = questrewardType;
		this.RewardItems = rewardItems;
		this.startDialouge = startDialouge;
		this.duringDialouge = duringDialouge;
		this.endDialouge = endDialouge;
	}
}
