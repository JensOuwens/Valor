using Godot;

namespace Valor.UI;

[GlobalClass]
public partial class QuestBackboardList : Resource
{
    [Export] public int HeightKey { get; set; }
    [Export] public Godot.Collections.Array<QuestBackboard> Backboards { get; set; }
    [Export] public int MarginLeft  { get; set; }
    [Export] public int MarginTop { get; set; }
    [Export] public int MarginRight { get; set; }
    [Export] public int MarginBottom { get; set; }
}