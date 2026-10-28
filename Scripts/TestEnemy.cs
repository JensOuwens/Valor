using Godot;

namespace Valor;

/// <summary>
/// Temporary enemy for verifying the hitbox and damage logs without art or animations.
/// Attach to a CharacterBody2D with a CollisionShape2D child.
/// </summary>
public partial class TestEnemy : CharacterBody2D, IDamageable
{
    [Export] public float MaxHealth { get; set; } = 100f;
    public float Health { get; private set; } = 100f;

    public override void _Ready()
    {
        this.Health = Mathf.Max(0f, this.MaxHealth);
        this.CollisionLayer = 2; // Physics layer 2: attack hitbox target layer.
        this.CollisionMask = 1;  // Collide with player/world layer 1.
        GD.Print($"[TestEnemy:{this.Name}] Ready. HP={this.Health:0.##}/{this.MaxHealth:0.##}, collision layer=2.");
    }

    public void Hurt(float damage)
    {
        if (damage <= 0f || this.Health <= 0f)
        {
            GD.Print($"[TestEnemy:{this.Name}] Ignored damage {damage:0.##}; HP={this.Health:0.##}.");
            return;
        }

        float before = this.Health;
        this.Health = Mathf.Max(0f, this.Health - damage);
        GD.Print($"[TestEnemy:{this.Name}] Took {damage:0.##} damage. HP {before:0.##} -> {this.Health:0.##}/{this.MaxHealth:0.##}.");

        if (this.Health <= 0f)
        {
            GD.Print($"[TestEnemy:{this.Name}] Defeated. Disabling collision layer for test purposes.");
            this.CollisionLayer = 0;
            this.CollisionMask = 0;
        }
    }
}