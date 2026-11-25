using Godot;

namespace Valor.UI;

[GlobalClass]
public partial class HealthBarItem : Resource
{
    [Export] public Color modulateColor = Colors.White;
    [Export] public Texture texture;
}