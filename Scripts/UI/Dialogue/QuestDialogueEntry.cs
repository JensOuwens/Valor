using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

[GlobalClass]
public partial class QuestDialogueEntry : DialogueEntry
{
    [Export]
    public QuestData.QuestDialogueState State { get; set; }
}