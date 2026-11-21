using Godot;

namespace Valor.UI;

public partial class DialogueManager : Node
{
    [Export]
    public Camera3D camera3D;
    
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
    
    [Export]
    public Godot.Collections.Dictionary<Vector2I, Texture2D> Backboards { get; set; } = new();
    
    public void ShowQuestPopup(QuestData playerCurrentQuest, Vector3 positionQuestGiver)
    {
        this.camera3D.UnprojectPosition(positionQuestGiver);
        
    }
}