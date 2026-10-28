using Godot;

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
        Health = Mathf.Max(0f, MaxHealth);
        CollisionLayer = 2; // Physics layer 2: attack hitbox target layer.
        CollisionMask = 1;  // Collide with player/world layer 1.
        GD.Print($"[TestEnemy:{Name}] Ready. HP={Health:0.##}/{MaxHealth:0.##}, collision layer=2.");
    }

    public void Hurt(float damage)
    {
        if (damage <= 0f || Health <= 0f)
        {
            GD.Print($"[TestEnemy:{Name}] Ignored damage {damage:0.##}; HP={Health:0.##}.");
            return;
        }

        float before = Health;
        Health = Mathf.Max(0f, Health - damage);
        GD.Print($"[TestEnemy:{Name}] Took {damage:0.##} damage. HP {before:0.##} -> {Health:0.##}/{MaxHealth:0.##}.");

        if (Health <= 0f)
        {
            GD.Print($"[TestEnemy:{Name}] Defeated. Disabling collision layer for test purposes.");
            CollisionLayer = 0;
            CollisionMask = 0;
        }
    }
}
