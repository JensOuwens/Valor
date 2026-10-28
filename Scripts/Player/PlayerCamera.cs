using Godot;

namespace Valor;

public partial class PlayerCamera : Camera3D
{
    [Export] private Node3D player;
    [Export] private float speed = 10.0f;

    private Vector3 offset;

    public override void _Ready()
    {
        if (this.player != null)
            this.offset = this.GlobalPosition - this.player.GlobalPosition;
        else
            GD.PrintErr("Player node is not assigned in the Camera Inspector");
    }

    public override void _Process(double delta)
    {
        if (this.player == null) return;

        Vector3 targetPosition = this.player.GlobalPosition + this.offset;

        // Smoothly lerp towards the target position.
        // Using Mathf.Exp ensures the camera movement is smooth and frame-rate independent.
        float weight = 1.0f - Mathf.Exp(-this.speed * (float)delta);
        this.GlobalPosition = this.GlobalPosition.Lerp(targetPosition, weight);
    }
}