using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

public partial class QuestPopUp : DialoguePopUp
{
    private QuestData currentQuest;
    
    [Export] public Label CurrentQuestLabel { get; set; }
    [Export] public Label NameQuest { get; set; }
    [Export] public TextureButton AcceptButton { get; set; }
    [Export] public TextureButton DeclineButton { get; set; }

    public void SetQuest(QuestData playerCurrentQuest, QuestData.QuestDialogueState dialogueState, PlayerScript player)
    {
        if (this.currentQuest == playerCurrentQuest) return;
        this.NameQuest.Text = playerCurrentQuest.QuestName;
        this.currentQuest = playerCurrentQuest;
        
        QuestDialogueEntry dialogue =
            playerCurrentQuest.QuestDialogue.FirstOrDefault(obj => obj.State == dialogueState);
        SetData(playerCurrentQuest.NpcName, dialogue, player);

        this.AcceptButton.Pressed += OnAcceptQuestButtonPressed;
        this.AcceptButton.MouseFilter = MouseFilterEnum.Stop;
        this.DeclineButton.Pressed += OnDeclineButtonPressed;
        this.DeclineButton.MouseFilter = MouseFilterEnum.Stop;
    }
    
    public void SetReward(QuestData playerCurrentQuest, QuestData.QuestDialogueState dialogueState, PlayerScript player)
    {
        if (this.currentQuest == playerCurrentQuest) return;
        this.NameQuest.Text = playerCurrentQuest.QuestName;
        this.currentQuest = playerCurrentQuest;
        
        QuestDialogueEntry dialogue =
            playerCurrentQuest.QuestDialogue.FirstOrDefault(obj => obj.State == dialogueState);
        SetData(playerCurrentQuest.NpcName, dialogue, player);

        this.AcceptButton.Pressed += OnAcceptRewardButtonPressed;
        this.AcceptButton.MouseFilter = MouseFilterEnum.Stop;
        this.DeclineButton.Pressed += OnDeclineButtonPressed;
        this.DeclineButton.MouseFilter = MouseFilterEnum.Stop;
    }

    private void OnAcceptRewardButtonPressed()
    {
        this.AcceptButton.Pressed -= OnAcceptQuestButtonPressed;
        
        //TODO:  this.CurrentQuestLabel.Text = "";
        
        foreach (ItemData questItem in this.playerRef.CurrentQuest.QuestItems)
        {
            if (this.playerRef.inventory.IsInInvetory(questItem.Item_Name))
                this.playerRef.inventory.RemoveFromInventory(questItem.Item_Name);
        }

        switch (this.playerRef.CurrentQuest.questrewardType)
        {
            case QuestRewardTypes.Item:
                foreach (ItemData questReward in this.playerRef.CurrentQuest.RewardItems)
                {
                    this.playerRef.inventory.AddToInventory(questReward);
                    _ = this.playerRef.TryPlayerPickup(questReward.Item_Name);
                }
                break;
            case QuestRewardTypes.Land: //TODO: unlock area from list
                break;
            case QuestRewardTypes.ItemAndLand: 
                foreach (ItemData questReward in this.playerRef.CurrentQuest.RewardItems)
                {
                    this.playerRef.inventory.AddToInventory(questReward);
                    _ = this.playerRef.TryPlayerPickup(questReward.Item_Name);
                }
                //TODO: unlock area from list
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
		
        this.playerRef.CurrentQuest = null;
        this.playerRef.IsQuestActive = false;
        
        this.playerRef.CurrentQuest = this.currentQuest;
        this.playerRef.IsQuestActive = true;
        
        this.playerRef.ResetAttackCommand();
        DialogueManager.Instance.HideQuestPopup();
        this.currentQuest = null;
    }

    private void OnDeclineButtonPressed()
    {
        this.DeclineButton.Pressed -= OnDeclineButtonPressed;
        this.playerRef.ResetAttackCommand();
        DialogueManager.Instance.HideQuestPopup();
        this.currentQuest = null;
    }

    private void OnAcceptQuestButtonPressed()
    {
        this.AcceptButton.Pressed -= OnAcceptQuestButtonPressed;
        
        //TODO:  this.CurrentQuestLabel.Text = this.playerRef.CurrentQuest.QuestName + " - give me: " + this.playerRef.CurrentQuest.QuestItems[0].Item_Name;

        this.playerRef.CurrentQuest = this.currentQuest;
        this.CurrentQuestLabel.Text = this.playerRef.CurrentQuest.QuestName + " - give me: " + this.playerRef.CurrentQuest.QuestItems[0].Item_Name;
        this.playerRef.IsQuestActive = true;
        
        this.playerRef.ResetAttackCommand();
        DialogueManager.Instance.HideQuestPopup();
        this.currentQuest = null;
    }
}