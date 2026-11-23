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
    private Node3D questGiver;

    public override void _Ready()
    {
        base._Ready();
        this.camera3D = GetViewport()?.GetCamera3D();
        if (this.QuestPopup == null) return;
        this.QuestPopup.Visible = false;
    }

    public void ShowQuestPopup(QuestData playerCurrentQuest, QuestData.QuestDialogueState dialogueState, Node3D questGiver)
    {
        if (playerCurrentQuest == null || this.QuestPopup == null || this.camera3D  == null) return;

        this.QuestPopup.Visible = true;
        this.questGiver = questGiver;

        this.QuestPopup.SetQuest(playerCurrentQuest, dialogueState);
    }

    public void HideQuestPopup(Node3D questNpc)
    {
        if (this.questGiver != questNpc) return;
        this.QuestPopup.Visible = false;
        this.questGiver = null;
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (this.QuestPopup == null || this.camera3D == null || this.questGiver == null) return;
        this.QuestPopup.Position = this.camera3D.UnprojectPosition(this.questGiver.GlobalPosition) - new Vector2(this.QuestPopup.Size.X / 2, this.QuestPopup.Size.Y);
    }
}