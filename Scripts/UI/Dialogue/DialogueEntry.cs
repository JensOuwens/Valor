using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

[GlobalClass]
public partial class DialogueEntry : Resource
{
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = string.Empty;

    [Export] public float BoxWidth { get; set; } = -1;
    [Export] public float BoxHeight { get; set; } = -1;
}