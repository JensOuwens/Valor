using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

public partial class QuestPopUp : Control
{
    [Export]
    public Godot.Collections.Array<QuestBackboardList> Backboards { get; set; }
    
    [Export]
    public TextureRect Background { get; set; }
    
    [Export]
    public VBoxContainer Content { get; set; }
    
    [Export]
    public Label NameQuestNpc { get; set; }
    [Export]
    public Label NameQuest { get; set; }
    [Export]
    public TextBoxWithItemIcons QuestText { get; set; }
    
    [Export]
    public TextureButton AcceptButton { get; set; }
    [Export]
    public TextureButton DeclineButton { get; set; }
    

    public void SetQuest(QuestData playerCurrentQuest, QuestData.QuestDialogueState dialogueState)
    {
        this.NameQuestNpc.Text = playerCurrentQuest.NpcName;
        this.NameQuest.Text = playerCurrentQuest.QuestName;

        string text = playerCurrentQuest.QuestDialogue.FirstOrDefault(obj => obj.State == dialogueState)?.Text;
        _ = TryGetCorrectBlackboard(text);
        
        // TODO: accept and decline
        
        // TODO: Should only be activated if player accepts
        // player.CurrentQuest = this.Quests.Last();
        // player.IsQuestActive = true;
    }
    
    private async Task TryGetCorrectBlackboard(string text)
    {
        bool found = false;
        
        foreach (QuestBackboardList backboardList in this.Backboards)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            foreach (QuestBackboard backboard in backboardList.Backboards)
            {
                this.QuestText.Text = text;
                this.QuestText.Rebuild();
                ShrinkToFit();

                Vector2 size = this.Content.GetCombinedMinimumSize();
                GD.Print($"[UI: size of current quest popup] {size}");
                GD.Print($"[UI: checking size against] {backboard.ContentConstraints}");
                
                if (size.X > backboard.ContentConstraints.X || size.Y > backboard.ContentConstraints.Y) continue;
                this.Background.Texture = backboard.Texture;
                this.CustomMaximumSize = backboard.MaxSize;
                this.CustomMinimumSize = backboard.MinSize;
                found = true;
                GD.Print($"[UI: size found] {backboard.MaxSize}");
                break;
            }
            
            if (found) break;
        }
        
        if (!found) 
            GD.PushWarning("[UI] Backboard cannot be found for size current Quest");
    }
    
    /// <summary>
    /// Shrinks the MarginContainer to fit its current visible children + margin theme constants.
    /// </summary>
    private void ShrinkToFit()
    {
        Vector2 minSize = GetCombinedMinimumSize();
        this.Size = minSize;
    }
}