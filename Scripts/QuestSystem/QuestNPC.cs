using System;
using System.Linq;
using Godot;
using Valor.UI;
using DialogueManager = Valor.UI.DialogueManager;

namespace Valor.QuestSystem;

/// <summary>
/// Handles quest giving and completion, also giving out rewards
/// </summary>
public partial class QuestNPC : Node3D
{
	[Export] public string NpcName;
	[Export] public QuestData[] Quests;
	[Export] public AnimationPlayer AnimationPlayer;
	private Area3D talkCollider;

	public override void _Ready()
	{
		this.talkCollider = GetNode<Area3D>("TalkCollision");
		this.talkCollider.BodyEntered += OnBodyEntered;
		this.talkCollider.BodyExited += OnBodyExited;
		
		Animation anim = this.AnimationPlayer.GetAnimation("Idle");
		if (anim != null) anim.LoopMode = Animation.LoopModeEnum.Linear;
		this.AnimationPlayer.Play("Idle");
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
		DialogueManager.Instance.HideQuestPopup();
		DialogueManager.Instance.HideDialoguePopup();
	}

	/// <summary>
	/// Function called when player presses the speak button
	/// </summary>
	/// <param name="player"></param>
	private void DialogueEntered(PlayerScript player)
	{
		if (!player.IsQuestActive) // TODO: this currently makes it so that the player can have only one quest, thus limiting player freedom
			GiveQuest(player);
		else
		{
			if (player.CurrentQuest.NpcName != this.NpcName) return;
			int questItemCount = player.CurrentQuest.QuestItems.Length;
			int currentQuestItems = 0;

			foreach (ItemData questItem in player.CurrentQuest.QuestItems)
			{
				if (!player.inventory.IsInInvetory(questItem.Item_Name)) continue;
				currentQuestItems++;

				if (questItemCount != currentQuestItems) continue;

				CompleteQuest(player);
				return;
			}

			DuringDialogue(player.CurrentQuest, player);
		}
	}
	
	private void GiveQuest(PlayerScript player) => 
		DialogueManager.Instance.ShowQuestPopup(this.Quests.Last(), QuestData.QuestDialogueState.Start, player, this);

	private void CompleteQuest(PlayerScript player) => 
		DialogueManager.Instance.ShowRewardPopup(player.CurrentQuest, QuestData.QuestDialogueState.End, player, this);

	private void DuringDialogue(QuestData quest, PlayerScript player)
	{
		QuestDialogueEntry dialogue = quest.QuestDialogue.FirstOrDefault(obj => obj.State == QuestData.QuestDialogueState.During);
		DialogueManager.Instance.ShowDialoguePopup(quest.NpcName, dialogue, player, this);
	}
}