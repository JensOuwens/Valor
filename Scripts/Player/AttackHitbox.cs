using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Valor;

/// <summary>
/// Temporary melee Area3D. It damages each damageable body at most once per swing.
/// </summary>
public partial class AttackHitbox : Area3D
{
	[Export] public float ActiveDuration { get; set; } = 0.12f;
	[Export] public float ForwardOffset { get; set; } = 22f;
	[Export] public Vector3 BoxSize { get; set; } = new Vector3(32f, 32f, 24f);

	private PlayerBlackboard blackboard;
	private CollisionShape3D collisionShape;
	private readonly HashSet<ulong> hitBodyIds = [];
	private float damage;
	private float remaining;
	private bool active;
	private string attackName = "Attack";

	public override void _Ready()
	{
		this.collisionShape = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
		if (this.collisionShape == null)
		{
			this.collisionShape = new CollisionShape3D { Name = "CollisionShape3D" };
			AddChild(this.collisionShape);
			GD.Print("[Hitbox] CollisionShape2D was missing; created it at runtime.");
		}

		if (this.collisionShape.Shape == null)
		{
			this.collisionShape.Shape = new BoxShape3D() { Size = this.BoxSize };
			GD.Print($"[Hitbox] No shape resource assigned; created RectangleShape3D size={this.BoxSize}.");
		}
		else if (this.collisionShape.Shape is BoxShape3D rectangle) 
			rectangle.Size = this.BoxSize;

		this.collisionShape.Disabled = false;
		this.CollisionLayer = 0;
		this.CollisionMask = 0;
		SetCollisionMaskValue(2, true); // Enemy bodies should be on physics layer 2.
		this.Monitoring = false;
		this.Monitorable = false;
		this.BodyEntered += OnBodyEntered;
		SetPhysicsProcess(true);
		GD.Print("[Hitbox] Ready. Monitoring is off until an attack fires. Collision mask targets layer 2.");
	}

	public void Initialize(PlayerBlackboard blackboard) => this.blackboard = blackboard;

	public void PerformAttack(float damage, Vector2 direction, string attackName)
	{
		if (direction == Vector2.Zero)
			direction = Vector2.Down;
		direction = direction.Normalized();

		this.damage = Mathf.Max(0f, damage);
		this.attackName = attackName;
		this.hitBodyIds.Clear();
		this.remaining = Mathf.Max(0.01f, this.ActiveDuration);
		this.active = true;

		this.Position = new Vector3(direction.X, 0f, direction.Y) * this.ForwardOffset;
		this.Rotation = new Vector3(0f, -direction.Angle(), 0f);
		this.Monitoring = true;
		if (this.blackboard != null) this.blackboard.IsAttacking = true;

		GD.Print($"[Hitbox] {this.attackName} activated. Damage={this.damage:0.##}, duration={this.remaining:0.##}s, direction={direction}.");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!this.active)
			return;

		Array<Node3D> bodies = GetOverlappingBodies();

		GD.Print(
			$"[Hitbox DEBUG] Active={this.active}, " +
			$"Overlapping bodies={bodies.Count}"
		);

		foreach (Node3D body in bodies)
			TryDamageBody(body);
		
		// BodyEntered is the normal path. Scanning overlap results also catches
		// a body that was already overlapping when monitoring became active.

		this.remaining -= (float)delta;
		if (this.remaining <= 0f)
			EndAttack();
	}

	private void OnBodyEntered(Node3D body)
	{
		GD.Print(
			$"[Hitbox DEBUG] BodyEntered: {body.Name}, " +
			$"Active={this.active}, Monitoring={this.Monitoring}, " +
			$"IsDamageable={body is IDamageable}"
		);

		if (this.active)
			TryDamageBody(body);
	}

	private void TryDamageBody(Node3D body)
	{
		if (body == null || body == this.blackboard?.Player)
			return;

		ulong id = body.GetInstanceId();
		if (!this.hitBodyIds.Add(id))
			return;

		if (body is IDamageable damageable)
		{
			GD.Print($"[Hitbox] {this.attackName} hit '{body.Name}' for {this.damage:0.##} damage. Target HP before hit={damageable.Health:0.##}.");
			damageable.Hurt(this.damage);
		}
		else
			GD.Print($"[Hitbox] Overlapped '{body.Name}', but it does not implement IDamageable.");
	}

	private void EndAttack()
	{
		this.active = false;
		this.Monitoring = false;
		this.Position = Vector3.Zero;
		this.Rotation = Vector3.Zero;
		if (this.blackboard != null) this.blackboard.IsAttacking = false;
		GD.Print($"[Hitbox] {this.attackName} window ended.");
	}
}