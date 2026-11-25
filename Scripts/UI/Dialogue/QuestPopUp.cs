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
        GD.Print("[Quest debug] Setting quest");
        if (this.currentQuest == playerCurrentQuest) return;
        this.NameQuest.Text = playerCurrentQuest.QuestName;
        this.currentQuest = playerCurrentQuest;
        
        QuestDialogueEntry dialogue =
            playerCurrentQuest.QuestDialogue.FirstOrDefault(obj => obj.State == dialogueState);
        SetData(playerCurrentQuest.NpcName, dialogue, player);

        this.AcceptButton.Pressed += OnAcceptButtonPressed;
        this.AcceptButton.MouseFilter = MouseFilterEnum.Stop;
        this.DeclineButton.Pressed += OnDeclineButtonPressed;
        this.DeclineButton.MouseFilter = MouseFilterEnum.Stop;
    }

    private void OnDeclineButtonPressed()
    {
        this.DeclineButton.Pressed -= OnDeclineButtonPressed;
        this.playerRef.ResetAttackCommand();
        DialogueManager.Instance.HideQuestPopup();
        this.currentQuest = null;
    }

    private void OnAcceptButtonPressed()
    {
        this.AcceptButton.Pressed -= OnAcceptButtonPressed;
        
        // this.CurrentQuestLabel.Text = this.playerRef.CurrentQuest.QuestName + " - give me: " + this.playerRef.CurrentQuest.QuestItems[0].Item_Name;
        
        this.playerRef.CurrentQuest = this.currentQuest;
        this.playerRef.IsQuestActive = true;
        
        this.playerRef.ResetAttackCommand();
        DialogueManager.Instance.HideQuestPopup();
        this.currentQuest = null;
    }
}