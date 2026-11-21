using Godot;

namespace Valor.AnimationUtil;

/// <summary>
/// Makes a target bob up and down
/// </summary>
public partial class SineBobber : Node
{
    [Export]
    public Node3D Target { get; set; }

    [Export]
    public float Speed { get; set; } = 3.0f;

    [Export]
    public float Distance { get; set; } = 0.25f;

    private float baseY;
    private float time;

    public override void _Ready()
    {
        if (this.Target != null) this.baseY = this.Target.Position.Y;
    }

    public override void _Process(double delta)
    {
        if (this.Target == null)
            return;

        this.time += (float)delta * this.Speed;
        float offset = Mathf.Sin(this.time) * this.Distance;

        Vector3 pos = this.Target.Position;
        pos.Y = this.baseY + offset;
        this.Target.Position = pos;
    }
}