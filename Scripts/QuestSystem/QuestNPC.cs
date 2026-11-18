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
	
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is PlayerScript player)
		{
			if (player.IsQuestActive)
			{
				int questItemCount = player.CurrentQuest.QuestItems.Length;
				int currentQuestItems = 0;
				foreach (ItemData questitem in player.CurrentQuest.QuestItems)
				{
					if (player.inventory.IsInInvetory(questitem.Item_Name))
					{
						currentQuestItems++;

						if (questItemCount != currentQuestItems) continue;

						CompleteQuest(player);
						break;
					}
				}
				DuringDialogue(player.CurrentQuest);
			}
			else
			{
				GiveQuest(player);
			}
		}
	}
	
	private void GiveQuest(PlayerScript player)
	{
		player.CurrentQuest = Quests.Last();
		player.IsQuestActive = true;
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
				}
				break;
			case QuestRewardTypes.Land: //unlock area from list
							   break;
			case QuestRewardTypes.ItemAndLand: 
				foreach (ItemData questReward in player.CurrentQuest.RewardItems)
				{
					player.inventory.AddToInventory(questReward);
				}
				//unlock area from list
				break;
		}
		
		player.CurrentQuest = null;
		player.IsQuestActive = false;
	}

	private void DuringDialogue(QuestData quest)
	{
		//during dialouge
	}
}
