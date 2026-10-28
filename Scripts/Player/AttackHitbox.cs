using System.Collections.Generic;
using Godot;

/// <summary>
/// Temporary melee Area2D. It damages each damageable body at most once per swing.
/// </summary>
public partial class AttackHitbox : Area2D
{
	[Export] public float ActiveDuration { get; set; } = 0.12f;
	[Export] public float ForwardOffset { get; set; } = 22f;
	[Export] public Vector2 BoxSize { get; set; } = new Vector2(32f, 24f);

	private PlayerBlackboard _blackboard;
	private CollisionShape2D _collisionShape;
	private readonly HashSet<ulong> _hitBodyIds = new();
	private float _damage;
	private float _remaining;
	private bool _active;
	private string _attackName = "Attack";

	public override void _Ready()
	{
		_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (_collisionShape == null)
		{
			_collisionShape = new CollisionShape2D { Name = "CollisionShape2D" };
			AddChild(_collisionShape);
			GD.Print("[Hitbox] CollisionShape2D was missing; created it at runtime.");
		}

		if (_collisionShape.Shape == null)
		{
			_collisionShape.Shape = new RectangleShape2D { Size = BoxSize };
			GD.Print($"[Hitbox] No shape resource assigned; created RectangleShape2D size={BoxSize}.");
		}
		else if (_collisionShape.Shape is RectangleShape2D rectangle)
		{
			rectangle.Size = BoxSize;
		}

		_collisionShape.Disabled = false;
		CollisionLayer = 0;
		CollisionMask = 0;
		SetCollisionMaskValue(2, true); // Enemy bodies should be on physics layer 2.
		Monitoring = false;
		Monitorable = false;
		BodyEntered += OnBodyEntered;
		SetPhysicsProcess(true);
		GD.Print("[Hitbox] Ready. Monitoring is off until an attack fires. Collision mask targets layer 2.");
	}

	public void Initialize(PlayerBlackboard blackboard)
	{
		_blackboard = blackboard;
	}

	public void PerformAttack(float damage, Vector2 direction, string attackName)
	{
		if (direction == Vector2.Zero)
			direction = Vector2.Down;
		direction = direction.Normalized();

		_damage = Mathf.Max(0f, damage);
		_attackName = attackName;
		_hitBodyIds.Clear();
		_remaining = Mathf.Max(0.01f, ActiveDuration);
		_active = true;

		// The local +X axis is the forward axis. Rotation turns the box toward direction.
		Rotation = direction.Angle();
		Position = new Vector2(ForwardOffset, 0f);
		Monitoring = true;
		if (_blackboard != null)
			_blackboard.IsAttacking = true;

		GD.Print($"[Hitbox] {_attackName} activated. Damage={_damage:0.##}, duration={_remaining:0.##}s, direction={direction}.");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_active)
			return;

		// BodyEntered is the normal path. Scanning overlap results also catches
		// a body that was already overlapping when monitoring became active.
		foreach (Node2D body in GetOverlappingBodies())
			TryDamageBody(body);

		_remaining -= (float)delta;
		if (_remaining <= 0f)
			EndAttack();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_active)
			TryDamageBody(body);
	}

	private void TryDamageBody(Node2D body)
	{
		if (body == null || body == _blackboard?.Player)
			return;

		ulong id = body.GetInstanceId();
		if (!_hitBodyIds.Add(id))
			return;

		if (body is IDamageable damageable)
		{
			GD.Print($"[Hitbox] {_attackName} hit '{body.Name}' for {_damage:0.##} damage. Target HP before hit={damageable.Health:0.##}.");
			damageable.Hurt(_damage);
		}
		else
		{
			GD.Print($"[Hitbox] Overlapped '{body.Name}', but it does not implement IDamageable.");
		}
	}

	private void EndAttack()
	{
		_active = false;
		Monitoring = false;
		Position = Vector2.Zero;
		Rotation = 0f;
		if (_blackboard != null)
			_blackboard.IsAttacking = false;
		GD.Print($"[Hitbox] {_attackName} window ended.");
	}
}
