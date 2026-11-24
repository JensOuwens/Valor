using System;
using System.Linq;
using Godot;
using DialogueManager = Valor.UI.DialogueManager;

namespace Valor.QuestSystem;

/// <summary>
/// Handles quest giving and completion, also giving out rewards
/// TODO: add dialouge thingies
/// </summary>
public partial class QuestNPC : Node3D
{
	[Export] public QuestData[] Quests;
	private Area3D talkCollider;

	public override void _Ready()
	{
		this.talkCollider = GetNode<Area3D>("TalkCollision");
		this.talkCollider.BodyEntered += OnBodyEntered;
		this.talkCollider.BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is not PlayerScript player) return;
		player.TryRegisterSpeakCommand(() => DialogueEntered(player));
	}
	
	private void OnBodyExited(Node3D body)
	{
		if (body is not PlayerScript player) return;
		player.ResetAttackCommand();
		DialogueManager.Instance.HideQuestPopup(this);
	}

	/// <summary>
	/// Function called when player presses the speak button
	/// </summary>
	/// <param name="player"></param>
	private void DialogueEntered(PlayerScript player)
	{
		if (!player.IsQuestActive)
		{
			GiveQuest();
		}
		else
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
	}
	
	private void GiveQuest() => 
		DialogueManager.Instance.ShowQuestPopup(this.Quests.Last(), QuestData.QuestDialogueState.Start, this);

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
					_ = player.Playerpickedup(questReward.Item_Name);
				}
				break;
			case QuestRewardTypes.Land: //unlock area from list
				break;
			case QuestRewardTypes.ItemAndLand: 
				foreach (ItemData questReward in player.CurrentQuest.RewardItems)
				{
					player.inventory.AddToInventory(questReward);
					_ = player.Playerpickedup(questReward.Item_Name);
				}
				//unlock area from list
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		
		player.CurrentQuest = null;
		player.IsQuestActive = false;
	}

	private void DuringDialogue(QuestData quest)
	{
		//during dialouge
	}
}