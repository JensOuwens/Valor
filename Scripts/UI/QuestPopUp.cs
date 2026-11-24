using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

public partial class QuestPopUp : Control
{
    [Export]
    public VBoxContainer Content { get; set; }
    [Export]
    public MarginContainer ContentMargin { get; set; }

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

        QuestDialogueEntry dialogue =
            playerCurrentQuest.QuestDialogue.FirstOrDefault(obj => obj.State == dialogueState);

        if (dialogue != null)
        {
            this.QuestText.Text = dialogue.Text;
            this.CustomMinimumSize = new Vector2(dialogue.BoxWidth, dialogue.BoxHeight);
        }

        this.QuestText.Rebuild();
        _ = ShrinkToFit();

        // TODO setup buttons
    }

    /// <summary>
    /// Shrinks the MarginContainer to fit its current visible children + margin theme constants.
    /// </summary>
    private async Task ShrinkToFit()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Vector2 minSize = GetCombinedMinimumSize();
        this.Size = minSize;
    }
}