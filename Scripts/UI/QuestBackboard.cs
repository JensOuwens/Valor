using Godot;

namespace Valor.UI;

[GlobalClass]
public partial class QuestBackboard : Resource
{
    [Export] public int WidthKey { get; set; }
    [Export] public Texture2D Texture { get; set; }
    [Export] public Vector2I MinSize { get; set; }
    [Export] public Vector2I MaxSize { get; set; } = new(-1, -1);
    [Export] public Vector2I ContentConstraints { get; set; } // width and height to check against when looking if content fits
}