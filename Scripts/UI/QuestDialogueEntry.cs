using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

[GlobalClass]
public partial class QuestDialogueEntry : Resource
{
    [Export]
    public QuestData.QuestDialogueState State { get; set; }

    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = string.Empty;
}