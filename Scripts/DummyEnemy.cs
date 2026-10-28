using Godot;

namespace Valor;

/// <summary>
/// Temporary enemy for verifying the hitbox and damage logs without art or animations.
/// Attach to a CharacterBody2D with a CollisionShape2D child.
/// </summary>
public partial class DummyEnemy : CharacterBody3D, IDamageable
{
    [Export] private float flinchStrengthDegrees = 20f;
    [Export] private Node3D joint;
    [Export] public float MaxHealth { get; set; } = 100f;
    public float Health { get; private set; } = 100f;
    
    private bool isFlipping = false;
    private Vector3 knockbackVelocity = Vector3.Zero;

    public override void _Ready()
    {
        this.Health = Mathf.Max(0f, this.MaxHealth);
        this.CollisionLayer = 2; // Physics layer 2: attack hitbox target layer.
        this.CollisionMask = 1;  // Collide with player/world layer 1.
        GD.Print($"[DummyEnemy:{this.Name}] Ready. HP={this.Health:0.##}/{this.MaxHealth:0.##}, collision layer=2.");
    }

    public void Hurt(float damage, Vector3 hitDirection)
    {
        if (this.isFlipping) return;
        this.isFlipping = true;

        if (hitDirection == Vector3.Zero) hitDirection = -this.Transform.Basis.Z;
        this.knockbackVelocity = hitDirection.Normalized() * 6.0f + Vector3.Up * 3.0f;

        // Transform world hit direction into local space relative to the enemy
        Vector3 localHitDir = (this.Transform.Basis.Inverse() * hitDirection).Normalized();

        // Calculate flinch angles for X (pitch) and Z (roll) using the exported strength
        float maxFlinchRad = Mathf.DegToRad(this.flinchStrengthDegrees);
        float flinchAngleX = (localHitDir.Z > 0 ? 1f : -1f) * maxFlinchRad * Mathf.Abs(localHitDir.Z);
        float flinchAngleZ = -localHitDir.X * maxFlinchRad;

        Tween tween = CreateTween();
        tween.SetParallel();

        // Flinch on X-axis (forward/backward)
        tween.TweenProperty(this.joint, "rotation:x", this.joint.Rotation.X + flinchAngleX, 0.15f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        // Flinch on Z-axis (left/right)
        tween.TweenProperty(this.joint, "rotation:z", this.joint.Rotation.Z + flinchAngleZ, 0.15f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        // Return both axes back to original rotation smoothly after the tap
        Tween returnTween = CreateTween();
        returnTween.SetParallel();
        returnTween.TweenInterval(0.15f);
        returnTween.TweenProperty(this.joint, "rotation:x", this.joint.Rotation.X, 0.25f)
            .SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        returnTween.TweenProperty(this.joint, "rotation:z", this.joint.Rotation.Z, 0.25f)
            .SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        GetTree().CreateTimer(0.4f).Timeout += () => 
        {
            this.isFlipping = false;
            this.knockbackVelocity = Vector3.Zero;
        };
        
        // if (damage <= 0f || this.Health <= 0f)
        // {
        //     GD.Print($"[TestEnemy:{this.Name}] Ignored damage {damage:0.##}; HP={this.Health:0.##}.");
        //     return;
        // }
        //
        // float before = this.Health;
        // this.Health = Mathf.Max(0f, this.Health - damage);
        // GD.Print($"[TestEnemy:{this.Name}] Took {damage:0.##} damage. HP {before:0.##} -> {this.Health:0.##}/{this.MaxHealth:0.##}.");
        //
        // if (!(this.Health <= 0f)) return;
        // GD.Print($"[TestEnemy:{this.Name}] Defeated. Disabling collision layer for test purposes.");
        // this.CollisionLayer = 0;
        // this.CollisionMask = 0;
    }
}