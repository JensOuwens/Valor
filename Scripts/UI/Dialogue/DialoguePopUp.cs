using System.Linq;
using System.Threading.Tasks;
using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

public partial class DialoguePopUp : Control
{
    protected PlayerScript playerRef;

    [Export] public VBoxContainer Content { get; set; }
    [Export] public Label NameNpc { get; set; }
    [Export] public TextBoxWithItemIcons MainText { get; set; }
    [Export] public TextureButton CloseButton { get; set; }


    public void SetData(string npcName, DialogueEntry dialogue, PlayerScript player)
    {
        this.playerRef = player;
        this.NameNpc.Text = npcName;
        this.CloseButton.Pressed += OnCloseButtonPressed;
        this.CloseButton.MouseFilter = MouseFilterEnum.Stop;

        SetDialogue(dialogue);

        this.MainText.Rebuild();
        _ = ShrinkToFit();
    }

    private void OnCloseButtonPressed()
    {
        this.CloseButton.Pressed -= OnCloseButtonPressed;
        this.playerRef.ResetAttackCommand();
        DialogueManager.Instance.HideDialoguePopup();
    }

    private void SetDialogue(DialogueEntry dialogue)
    {
        if (dialogue == null) return;
        this.MainText.Text = dialogue.Text;
        this.CustomMinimumSize = new Vector2(dialogue.BoxWidth, dialogue.BoxHeight);
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