using Godot;

namespace Valor.UI;

[GlobalClass, Tool]
public partial class HealthBarItem : Resource
{
    [Export] public Color modulateColor = Colors.White;
    [Export] public Texture2D texture;
    [Export] public Vector2I size;
}