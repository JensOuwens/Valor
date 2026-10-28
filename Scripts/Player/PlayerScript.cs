using Godot;
/// <summary>
/// this class handles player input
/// </summary>

namespace Valor;

public partial class PlayerScript : CharacterBody3D, IDamageable
{
	[Export] private PlayerVisual playerVisual;
	
	[Export] public AttackMode StartingAttackMode { get; set; } = AttackMode.Combo;
	
	private ICommand attackCommand;
	private ICommand dodgeCommand;
	private ICommand healCommand;
	private ICommand moveCommand;
	private CommandManager commandManager;
	private PlayerBlackboard blackboard;
	private AttackHitbox attackHitbox;
	private bool wasMoving;
	private Vector3 lastMovementDirection = Vector3.Forward;
	

	public PlayerBlackboard Blackboard => this.blackboard;
	public float Health => this.blackboard?.CurrentHealth ?? 0f;

	public override void _Ready()
	{
		// Player collides with world layer 1 and enemy layer 2.
		this.CollisionLayer = 1;
		this.CollisionMask = 3;

		this.blackboard = new PlayerBlackboard { Player = this, PlayerVisual = this.playerVisual };
		this.commandManager = new CommandManager();
		this.commandManager.InitializeHandlers(this.blackboard);

		this.attackCommand = new AttackCommand(this.commandManager);
		this.dodgeCommand = new DodgeCommand(this.commandManager);
		this.healCommand = new HealCommand(this.commandManager);
		this.moveCommand = new MoveCommand(this.commandManager);

		this.attackHitbox = GetNode<AttackHitbox>("AttackHitbox");
		this.attackHitbox.Initialize(this.blackboard);

		AttackHandler attackHandler = this.commandManager.GetHandler<AttackHandler>(HandlerId.Attack);
		attackHandler.Mode = this.StartingAttackMode;
		attackHandler.AttackTriggered += OnComboAttackTriggered;
		attackHandler.ChargedAttackTriggered += OnChargedAttackTriggered;

		GD.Print($"[Player] Ready. HP={this.blackboard.CurrentHealth:0.##}/{this.blackboard.MaxHealth:0.##}, flasks={this.blackboard.CurrentFlasks}/{this.blackboard.MaxFlasks}, move speed={this.blackboard.MoveSpeed:0.##}, attack mode={attackHandler.Mode}.");
		GD.Print("[Player] Controls expected: MoveUp/Down/Left/Right, Attack, Dodge, Heal.");
	}

	public override void _Process(double delta)
	{
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		Vector3 direction = new (inputDir.X, 0f, inputDir.Y);
		if (inputDir != Vector2.Zero) this.lastMovementDirection = direction.Normalized();

		CommandContext context = new()
		{
			Direction = direction,
			Blackboard = this.blackboard
		};

		if (Input.IsActionJustPressed("Attack"))
		{
			GD.Print("[Input] Attack pressed.");
			this.attackCommand.OnClick(context);
		}
		if (Input.IsActionPressed("Attack")) this.attackCommand.OnHold(context);
		if (Input.IsActionJustReleased("Attack"))
		{
			GD.Print("[Input] Attack released.");
			this.attackCommand.OnRelease(context);
		}

		if (Input.IsActionJustPressed("Heal"))
		{
			GD.Print("[Input] Heal pressed.");
			this.healCommand.OnClick(context);
		}

		if (Input.IsActionJustPressed("Dodge"))
		{
			GD.Print("[Input] Dodge pressed.");
			context.Direction = direction != Vector3.Zero ? direction.Normalized() : this.lastMovementDirection;
			this.dodgeCommand.OnClick(context);
		}

		if (inputDir != Vector2.Zero)
		{
			if (!this.wasMoving)
				this.moveCommand.OnClick(context);
			else
				this.moveCommand.OnHold(context);
			this.wasMoving = true;
		}
		else if (this.wasMoving)
		{
			this.moveCommand.OnRelease(context);
			this.wasMoving = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsOnFloor())
		{
			// Get the default gravity from Godot's project settings
			Vector3 velocity = this.Velocity;
			velocity.Y -= ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * (float)delta;
			this.Velocity = velocity;
		}
		
		// Move first, then advance timers. This avoids ending a dodge before its final movement step.
		MoveAndSlide();
		this.commandManager.Tick(delta);

		this.playerVisual.PlayAnim(this.Velocity != Vector3.Zero
			? new PlayerVisual.PlayerAnimation { name = PlayerVisual.EPlayerAnimNames.Walk }
			: new PlayerVisual.PlayerAnimation { name = PlayerVisual.EPlayerAnimNames.Idle });
	}

	private void OnComboAttackTriggered(int comboStep, float damage) => 
		this.attackHitbox.PerformAttack(damage, this.lastMovementDirection, $"Combo hit {comboStep}");

	private void OnChargedAttackTriggered(float damage) => 
		this.attackHitbox.PerformAttack(damage, this.lastMovementDirection, "Charged attack");

	public void SetAttackMode(AttackMode mode)
	{
		AttackHandler handler = this.commandManager.GetHandler<AttackHandler>(HandlerId.Attack);
		handler.Mode = mode;
		GD.Print($"[Player] Attack mode changed to {mode}.");
	}

	public void Hurt(float damage, Vector3 hitDirection)
	{
		if (this.blackboard == null)
		{
			GD.Print("[Player] Hurt ignored: blackboard is not initialized.");
			return;
		}
		if (damage <= 0f)
		{
			GD.Print($"[Player] Hurt ignored: invalid damage {damage:0.##}.");
			return;
		}
		if (this.blackboard.CurrentHealth <= 0f)
		{
			GD.Print("[Player] Hurt ignored: player is already at 0 HP.");
			return;
		}
		if (this.blackboard.IsInvincible)
		{
			GD.Print($"[Player] Damage {damage:0.##} prevented by dodge invincibility frames.");
			return;
		}

		float before = this.blackboard.CurrentHealth;
		this.blackboard.SetHealth(before - damage);
		GD.Print($"[Player] Took {damage:0.##} damage. HP {before:0.##} -> {this.blackboard.CurrentHealth:0.##}/{this.blackboard.MaxHealth:0.##}.");
		if (this.blackboard.CurrentHealth <= 0f)
			GD.Print("[Player] HP reached 0. Add your death/respawn handling here.");
	}

	// Example upgrade hook. Call this from your upgrade system.
	public void ApplyExampleUpgrade()
	{
		this.blackboard.AttackDamage += 5f;
		this.blackboard.MaxHealth += 20f;
		this.blackboard.SetHealth(this.blackboard.CurrentHealth + 20f);
		GD.Print($"[Upgrade] Applied example upgrade. AttackDamage={this.blackboard.AttackDamage:0.##}, MaxHealth={this.blackboard.MaxHealth:0.##}, HP={this.blackboard.CurrentHealth:0.##}.");
	}
}