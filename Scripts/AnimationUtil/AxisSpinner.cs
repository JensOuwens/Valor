using Godot;

namespace Valor.AnimationUtil;

/// <summary>
/// Spins a target along a normalized axis vector
/// </summary>
public partial class AxisSpinner : Node
{
    [Export]
    public Node3D Target { get; set; }

    [Export]
    public Vector3 Axis { get; set; } = Vector3.Up;

    [Export]
    public float Speed { get; set; } = 2.0f;

    public override void _Process(double delta)
    {
        if (this.Target == null || this.Axis.LengthSquared() < Mathf.Epsilon)
            return;

        Vector3 normalizedAxis = this.Axis.Normalized();
        float angle = this.Speed * (float)delta;

        this.Target.Rotate(normalizedAxis, angle);
    }
}