using Godot;
using System;
using System.Linq;
using Valor;

/// <summary>
/// Handles quest giving and completion, also giving out rewards
/// TODO: add dialouge thingies
/// </summary>
public partial class QuestNPC : Area3D
{
	[Export] public QuestData[] Quests;
	[Export] public RichTextLabel QuestLabel;
	
	public override void _Ready()
	{
		this.BodyEntered += OnBodyEntered;
		this.QuestLabel.Text = "No quest";
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is not PlayerScript player) return;
		if (player.IsQuestActive)
		{
			int questItemCount = player.CurrentQuest.QuestItems.Length;
			int currentQuestItems = 0;
			foreach (ItemData questItem in player.CurrentQuest.QuestItems)
			{
				if (!player.inventory.IsInInvetory(questItem.Item_Name)) continue;
				currentQuestItems++;

				if (questItemCount != currentQuestItems) continue;

				CompleteQuest(player);
				break;
			}
			DuringDialogue(player.CurrentQuest);
		}
		else
			GiveQuest(player);
	}
	
	private void GiveQuest(PlayerScript player)
	{
		player.CurrentQuest = this.Quests.Last();
		player.IsQuestActive = true;

		this.QuestLabel.Text = player.CurrentQuest.quest_Name + " - give me: " + player.CurrentQuest.QuestItems[0].Item_Name;
		//also play dialouge i geuss
	}
	
	private void CompleteQuest(PlayerScript player)
	{
		foreach (ItemData questitem in player.CurrentQuest.QuestItems)
		{
			if (player.inventory.IsInInvetory(questitem.Item_Name))
				player.inventory.RemoveFromInventory(questitem.Item_Name);
		}
		
		//completed dialouge

		switch (player.CurrentQuest.questrewardType)
		{
			case QuestRewardTypes.Item:
				foreach (ItemData questReward in player.CurrentQuest.RewardItems)
				{
					player.inventory.AddToInventory(questReward);
					player.Playerpickedup(questReward.Item_Name);
				}
				break;
			case QuestRewardTypes.Land: //unlock area from list
							   break;
			case QuestRewardTypes.ItemAndLand: 
				foreach (ItemData questReward in player.CurrentQuest.RewardItems)
				{
					player.inventory.AddToInventory(questReward);
					player.Playerpickedup(questReward.Item_Name);
				}
				//unlock area from list
				break;
		}
		
		player.CurrentQuest = null;
		player.IsQuestActive = false;

		this.QuestLabel.Text = "No quest";
	}

	private void DuringDialogue(QuestData quest)
	{
		//during dialouge
	}
}
