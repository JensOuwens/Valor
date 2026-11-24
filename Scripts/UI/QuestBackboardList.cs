using Godot;

namespace Valor.UI;

[GlobalClass]
public partial class QuestBackboardList : Resource
{
    [Export] public int HeightKey { get; set; }
    [Export] public Godot.Collections.Array<QuestBackboard> Backboards { get; set; }
}