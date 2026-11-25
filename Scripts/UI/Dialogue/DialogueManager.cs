using System;
using System.Collections.Generic;
using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

public partial class DialogueManager : Node
{
    private Camera3D camera3D;
    
    #region Singleton Stuff
    public static DialogueManager Instance { get; private set; }
    
    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            GD.PushWarning($"{nameof(DialogueManager)} already exists. Removing duplicate on '{this.Name}'.");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }
    #endregion
    
    [Export] public QuestPopUp QuestPopup;
    [Export] public DialoguePopUp DialoguePopup;

    private List<Tuple<Control, Node3D>> activePopups = [];

    public override void _Ready()
    {
        base._Ready();
        this.camera3D = GetViewport()?.GetCamera3D();
        if (this.QuestPopup != null) this.QuestPopup.Visible = false;
        if (this.DialoguePopup != null) this.DialoguePopup.Visible = false;
    }

    public void ShowQuestPopup(QuestData playerCurrentQuest, QuestData.QuestDialogueState dialogueState,
        PlayerScript player, Node3D questGiver)
    {
        if (playerCurrentQuest == null || this.QuestPopup == null || this.camera3D  == null || player  == null) return;
        this.QuestPopup.Visible = true;

        this.QuestPopup.SetQuest(playerCurrentQuest, dialogueState, player);
        this.activePopups.Add(new Tuple<Control, Node3D>(this.QuestPopup, questGiver));
    }

    public void HideQuestPopup()
    {
        RemoveActivePopup(this.QuestPopup);
        this.QuestPopup.Visible = false;
        this.QuestPopup.MouseFilter = Control.MouseFilterEnum.Ignore;
        SetMouseFilterRecursive(this.QuestPopup, Control.MouseFilterEnum.Ignore);
    }
    
    public void ShowDialoguePopup(string questNpcName, QuestDialogueEntry dialogue, PlayerScript player, Node3D currentNpc)
    {
        GD.Print($"dialogue == null {dialogue == null} || this.DialoguePopup == null {this.DialoguePopup == null} || this.camera3D == null {this.camera3D == null} || player == null {player == null}");
        if (dialogue == null || this.DialoguePopup == null || this.camera3D == null || player == null) return;
        this.DialoguePopup.Visible = true;
        this.DialoguePopup.MouseFilter = Control.MouseFilterEnum.Stop;
        
        this.DialoguePopup.SetData(questNpcName, dialogue, player);
        this.activePopups.Add(new Tuple<Control, Node3D>(this.QuestPopup, currentNpc));
    }
    
    public void HideDialoguePopup()
    {
        RemoveActivePopup(this.DialoguePopup);
        this.DialoguePopup.Visible = false;
        this.DialoguePopup.MouseFilter = Control.MouseFilterEnum.Ignore;
        SetMouseFilterRecursive(this.DialoguePopup, Control.MouseFilterEnum.Ignore);
    }

    private void RemoveActivePopup(Control popup)
    {
        for (int i = 0; i < this.activePopups.Count; i++)
        {
            (Control popupOther, Node3D _) = this.activePopups[i];
            if (popupOther != popup) continue;
            this.activePopups.RemoveAt(i);
            return;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (this.camera3D == null) return;
        foreach ((Control popup, Node3D npc) in this.activePopups)
        {
            if (popup == null) continue;
            Vector2 offsetToMiddle = new(popup.Size.X / 2, popup.Size.Y);
            Vector2 extraOffset = Vector2.Zero;//new(0, popup.Size.Y / 8);
            popup.Position = this.camera3D.UnprojectPosition(npc.GlobalPosition) - offsetToMiddle - extraOffset;
        }
    }
    
    /// <summary>
    /// Recursively sets the MouseFilter on all descendants of a Control node.
    /// </summary>
    private static void SetMouseFilterRecursive(Node node, Control.MouseFilterEnum mouseFilter)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is Control controlChild) controlChild.MouseFilter = mouseFilter;
            SetMouseFilterRecursive(child, mouseFilter);
        }
    }
}