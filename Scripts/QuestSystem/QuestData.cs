using Godot;
using Valor.UI;

namespace Valor.QuestSystem;

/// <summary>
/// scripatble object that holds all data types the quest needs
/// </summary>
[GlobalClass]
public partial class QuestData : Resource
{
	[Export] public string NpcName;
	[Export] public string QuestName;
	[Export] public ItemData[] QuestItems;
	[Export] public QuestRewardTypes questrewardType;
	[Export] public ItemData[] RewardItems;
	//TODO: something for unlocking area's
	
	[Export]
	public Godot.Collections.Array<QuestDialogueEntry> QuestDialogue { get; set; }

	public enum QuestDialogueState
	{
		Start,
		During,
		End
	}
	
	public QuestData() : this(null, null, QuestRewardTypes.Item, null, null) {}

	public QuestData(string questName, ItemData[] questItems, QuestRewardTypes questrewardType,  
		ItemData[] rewardItems, Godot.Collections.Array<QuestDialogueEntry> questDialogue)
	{
		this.QuestName = questName;
		this.QuestItems = questItems;
		this.questrewardType = questrewardType;
		this.RewardItems = rewardItems;
		this.QuestDialogue = questDialogue;
	}
}
