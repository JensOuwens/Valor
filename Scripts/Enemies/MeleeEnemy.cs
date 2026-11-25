using Godot;
using Valor;

public partial class MeleeEnemy : BaseEnemy
{
	private enum EnemyState
	{
		Idle,
		Chasing,
		WindingUp,
		Lunging,
		Returning
	}
	
	[Export] private float AttackRange { get; set; } = 1.5f;
	[Export] private float AttackWindup { get; set; } = 0.6f;
	[Export] private float LungeSpeed { get; set; } = 7.0f;
	[Export] private float LungeDuration { get; set; } = 0.2f;
	
	[Export] private float Gravity { get; set; } = 9.8f;
	[Export] private float ReturnStopDistance { get; set; } = 0.2f;

	private Node3D _player;
	private IDamageable _playerDamageable;

	private GpuParticles3D _attackTelegraph;

	private Vector3 _homePosition;
	private Vector3 _lungeDirection;

	private EnemyState _state = EnemyState.Idle;

	private float _stateTimer;
	private float _attackCooldown;

	private bool _hasHit;
	private bool _hasNavigationTarget;


	public override void _Ready()
	{
		base._Ready();

		_player = GetTree().GetFirstNodeInGroup("Player") as Node3D;

		if (_player == null)
		{
			GD.PushWarning("MeleeEnemy: Could not find Player!");
			return;
		}

		_playerDamageable = _player as IDamageable;

		if (_playerDamageable == null)
			GD.PushWarning("MeleeEnemy: Player does not implement IDamageable!");
		
		_homePosition = Body.GlobalPosition;
		
		_attackTelegraph = Body.GetNodeOrNull<GpuParticles3D>("AttackTelegraph");
		
		NavAgent.TargetDesiredDistance = 0.15f;
	}


	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_player == null || !GodotObject.IsInstanceValid(_player))
		{
			StopAndApplyGravity(dt);
			return;
		}
		
		_attackCooldown = Mathf.Max(0f, _attackCooldown - dt);
		
		switch (_state)
		{
			case EnemyState.WindingUp:
				UpdateWindup(dt);
				return;

			case EnemyState.Lunging:
				UpdateLunge(dt);
				return;
		}
		
		float distanceFromHome = FlatDistance(
			_homePosition,
			_player.GlobalPosition
		);

		if (distanceFromHome <= AggroRange)
		{
			UpdateChase(dt);
		}
		else
		{
			UpdateReturn(dt);
		}
	}

	private void UpdateChase(float delta)
	{
		_state = EnemyState.Chasing;

		float distanceToPlayer = FlatDistance(
			Body.GlobalPosition,
			_player.GlobalPosition
		);
		
		if (distanceToPlayer <= AttackRange)
		{
			FacePosition(_player.GlobalPosition);
			StopAndApplyGravity(delta);
			
			if (_attackCooldown <= 0f)
				BeginWindup();

			return;
		}
		
		MoveUsingNavigation(_player.GlobalPosition, delta);
		FacePosition(_player.GlobalPosition);
	}

	

	private void BeginWindup()
	{
		_state = EnemyState.WindingUp;
		_stateTimer = AttackWindup;
		
		if (_attackTelegraph != null)
			_attackTelegraph.Restart();
	}


	private void UpdateWindup(float delta)
	{
		if (FlatDistance(_homePosition, _player.GlobalPosition) > AggroRange)
		{
			if (_attackTelegraph != null)
				_attackTelegraph.Emitting = false;

			_state = EnemyState.Returning;
			StopAndApplyGravity(delta);
			return;
		}

		FacePosition(_player.GlobalPosition);
		StopAndApplyGravity(delta);

		_stateTimer -= delta;

		if (_stateTimer <= 0f)
			BeginLunge();
	}

	private void BeginLunge()
	{
		_state = EnemyState.Lunging;
		_stateTimer = LungeDuration;

		_hasHit = false;
		
		_lungeDirection = GetFlatDirection(
			Body.GlobalPosition,
			_player.GlobalPosition
		);
		
		if (_lungeDirection.LengthSquared() < 0.001f)
		{
			_lungeDirection = -Body.GlobalBasis.Z;
			_lungeDirection.Y = 0f;
			_lungeDirection = _lungeDirection.Normalized();
		}
		
		_attackCooldown = 1f / Mathf.Max(Firerate, 0.01f);

		if (_attackTelegraph != null)
			_attackTelegraph.Emitting = false;
	}


	private void UpdateLunge(float delta)
	{
		FacePosition(Body.GlobalPosition + _lungeDirection);
		
		Vector3 velocity = _lungeDirection * LungeSpeed;
		velocity.Y = Body.Velocity.Y;

		Body.Velocity = velocity;

		ApplyGravity(delta);
		Body.MoveAndSlide();
		
		if (!_hasHit)
		{
			float distanceToPlayer = FlatDistance(
				Body.GlobalPosition,
				_player.GlobalPosition
			);

			if (distanceToPlayer <= AttackRange)
			{
				_hasHit = true;
				DamagePlayer();
			}
		}

		_stateTimer -= delta;

		if (_stateTimer <= 0f)
		{
			_state = EnemyState.Chasing;
			
			Body.Velocity = new Vector3(
				0f,
				Body.Velocity.Y,
				0f
			);
		}
	}


	private void DamagePlayer()
	{
		if (_playerDamageable == null)
			return;

		Vector3 hitDirection = GetFlatDirection(
			Body.GlobalPosition,
			_player.GlobalPosition
		);

		_playerDamageable.Hurt(Damage, hitDirection);
	}

	private void UpdateReturn(float delta)
	{
		_state = EnemyState.Returning;

		float distanceFromHome = FlatDistance(
			Body.GlobalPosition,
			_homePosition
		);
		
		if (distanceFromHome <= ReturnStopDistance)
		{
			_state = EnemyState.Idle;
			StopAndApplyGravity(delta);
			return;
		}

		MoveUsingNavigation(_homePosition, delta);
		FacePosition(_homePosition);
	}

	private void MoveUsingNavigation(Vector3 target, float delta)
	{
		if (!_hasNavigationTarget ||
			NavAgent.TargetPosition.DistanceTo(target) > 0.25f)
		{
			NavAgent.TargetPosition = target;
			_hasNavigationTarget = true;
		}

		Vector3 direction = Vector3.Zero;
		
		if (!NavAgent.IsNavigationFinished())
		{
			Vector3 nextPosition = NavAgent.GetNextPathPosition();

			direction = nextPosition - Body.GlobalPosition;
			direction.Y = 0f;

			if (direction.LengthSquared() > 0.001f)
				direction = direction.Normalized();
		}

		Body.Velocity = new Vector3(
			direction.X * Movespeed,
			Body.Velocity.Y,
			direction.Z * Movespeed
		);

		ApplyGravity(delta);
		Body.MoveAndSlide();
	}

	private void StopAndApplyGravity(float delta)
	{
		Body.Velocity = new Vector3(
			0f,
			Body.Velocity.Y,
			0f
		);

		ApplyGravity(delta);
		Body.MoveAndSlide();
	}


	private void ApplyGravity(float delta)
	{
		if (!Body.IsOnFloor())
		{
			Body.Velocity = new Vector3(
				Body.Velocity.X,
				Body.Velocity.Y - Gravity * delta,
				Body.Velocity.Z
			);
		}
		else if (Body.Velocity.Y < 0f)
		{
			Body.Velocity = new Vector3(
				Body.Velocity.X,
				0f,
				Body.Velocity.Z
			);
		}
	}


	private void FacePosition(Vector3 target)
	{
		Vector3 direction = target - Body.GlobalPosition;
		direction.Y = 0f;

		if (direction.LengthSquared() > 0.001f)
			Body.LookAt(Body.GlobalPosition + direction, Vector3.Up);
	}


	private Vector3 GetFlatDirection(Vector3 from, Vector3 to)
	{
		Vector3 direction = to - from;
		direction.Y = 0f;

		return direction.Normalized();
	}


	private float FlatDistance(Vector3 from, Vector3 to)
	{
		Vector3 difference = to - from;
		difference.Y = 0f;

		return difference.Length();
	}
}
