using System;
using System.Linq;
using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

public partial class QuestPopUp : AspectRatioContainer
{
    [Export]
    public Godot.Collections.Dictionary<Vector2I, Texture2D> Backboards { get; set; } = new();
    [Export]
    public TextureRect Background { get; set; } = new();
    
    [Export]
    public Label NameQuestNpc { get; set; } = new();
    [Export]
    public Label NameQuest { get; set; } = new();
    [Export]
    public TextBoxWithItemIcons QuestText { get; set; } = new();
    
    [Export]
    public TextureButton AcceptButton { get; set; } = new();
    [Export]
    public TextureButton DeclineButton { get; set; } = new();

    public void SetQuest(QuestData playerCurrentQuest, QuestData.QuestDialogueState dialogueState)
    {
        this.NameQuestNpc.Text = playerCurrentQuest.NpcName;
        this.NameQuest.Text = playerCurrentQuest.QuestName;

        try
        {
            this.QuestText.Text = playerCurrentQuest.QuestDialogue.FirstOrDefault(obj => obj.State == dialogueState)?.Text;
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
            throw;
        }
        
        // TODO: accept and decline
        
        // TODO: Should only be activated if player accepts
        // player.CurrentQuest = this.Quests.Last();
        // player.IsQuestActive = true;
    }
}