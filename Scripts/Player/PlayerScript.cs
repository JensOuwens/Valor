using Godot;
using System;

/// <summary>
/// this class handles player input
/// </summary>
using Godot;

public partial class PlayerScript : CharacterBody2D, IDamageable
{
    [Export] public AttackMode StartingAttackMode { get; set; } = AttackMode.Combo;

    private ICommand _attackCommand;
    private ICommand _dodgeCommand;
    private ICommand _healCommand;
    private ICommand _moveCommand;
    private CommandManager _commandManager;
    private PlayerBlackboard _blackboard;
    private AttackHitbox _attackHitbox;
    private bool _wasMoving;
    private Vector2 _lastMovementDirection = Vector2.Down;

    public PlayerBlackboard Blackboard => _blackboard;
    public float Health => _blackboard?.CurrentHealth ?? 0f;

    public override void _Ready()
    {
        // Player collides with world layer 1 and enemy layer 2.
        CollisionLayer = 1;
        CollisionMask = 3;

        _blackboard = new PlayerBlackboard { Player = this };
        _commandManager = new CommandManager();
        _commandManager.InitializeHandlers(_blackboard);

        _attackCommand = new AttackCommand(_commandManager);
        _dodgeCommand = new DodgeCommand(_commandManager);
        _healCommand = new HealCommand(_commandManager);
        _moveCommand = new MoveCommand(_commandManager);

        _attackHitbox = GetNode<AttackHitbox>("AttackHitbox");
        _attackHitbox.Initialize(_blackboard);

        AttackHandler attackHandler = _commandManager.GetHandler<AttackHandler>(HandlerId.Attack);
        attackHandler.Mode = StartingAttackMode;
        attackHandler.AttackTriggered += OnComboAttackTriggered;
        attackHandler.ChargedAttackTriggered += OnChargedAttackTriggered;

        GD.Print($"[Player] Ready. HP={_blackboard.CurrentHealth:0.##}/{_blackboard.MaxHealth:0.##}, flasks={_blackboard.CurrentFlasks}/{_blackboard.MaxFlasks}, move speed={_blackboard.MoveSpeed:0.##}, attack mode={attackHandler.Mode}.");
        GD.Print("[Player] Controls expected: MoveUp/Down/Left/Right, Attack, Dodge, Heal.");
    }

    public override void _Process(double delta)
    {
        Vector2 direction = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
        if (direction != Vector2.Zero)
            _lastMovementDirection = direction.Normalized();

        CommandContext context = new CommandContext
        {
            Direction = direction,
            Blackboard = _blackboard
        };

        if (Input.IsActionJustPressed("Attack"))
        {
            GD.Print("[Input] Attack pressed.");
            _attackCommand.OnClick(context);
        }
        if (Input.IsActionPressed("Attack"))
            _attackCommand.OnHold(context);
        if (Input.IsActionJustReleased("Attack"))
        {
            GD.Print("[Input] Attack released.");
            _attackCommand.OnRelease(context);
        }

        if (Input.IsActionJustPressed("Heal"))
        {
            GD.Print("[Input] Heal pressed.");
            _healCommand.OnClick(context);
        }

        // Test-only input: lets you verify health and dodge i-frames without enemy AI.
        if (Input.IsActionJustPressed("DebugDamage"))
        {
            GD.Print("[Input] DebugDamage pressed; attempting to apply 15 damage to player.");
            Hurt(15f);
        }

        if (Input.IsActionJustPressed("Dodge"))
        {
            GD.Print("[Input] Dodge pressed.");
            context.Direction = direction != Vector2.Zero ? direction.Normalized() : _lastMovementDirection;
            _dodgeCommand.OnClick(context);
        }

        if (direction != Vector2.Zero)
        {
            if (!_wasMoving)
                _moveCommand.OnClick(context);
            else
                _moveCommand.OnHold(context);
            _wasMoving = true;
        }
        else if (_wasMoving)
        {
            _moveCommand.OnRelease(context);
            _wasMoving = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Move first, then advance timers. This avoids ending a dodge before its final movement step.
        MoveAndSlide();
        _commandManager.Tick(delta);
    }

    private void OnComboAttackTriggered(int comboStep, float damage)
    {
        _attackHitbox.PerformAttack(damage, _lastMovementDirection, $"Combo hit {comboStep}");
    }

    private void OnChargedAttackTriggered(float damage)
    {
        _attackHitbox.PerformAttack(damage, _lastMovementDirection, "Charged attack");
    }

    public void SetAttackMode(AttackMode mode)
    {
        AttackHandler handler = _commandManager.GetHandler<AttackHandler>(HandlerId.Attack);
        handler.Mode = mode;
        GD.Print($"[Player] Attack mode changed to {mode}.");
    }

    public void Hurt(float damage)
    {
        if (_blackboard == null)
        {
            GD.Print("[Player] Hurt ignored: blackboard is not initialized.");
            return;
        }
        if (damage <= 0f)
        {
            GD.Print($"[Player] Hurt ignored: invalid damage {damage:0.##}.");
            return;
        }
        if (_blackboard.CurrentHealth <= 0f)
        {
            GD.Print("[Player] Hurt ignored: player is already at 0 HP.");
            return;
        }
        if (_blackboard.IsInvincible)
        {
            GD.Print($"[Player] Damage {damage:0.##} prevented by dodge invincibility frames.");
            return;
        }

        float before = _blackboard.CurrentHealth;
        _blackboard.SetHealth(before - damage);
        GD.Print($"[Player] Took {damage:0.##} damage. HP {before:0.##} -> {_blackboard.CurrentHealth:0.##}/{_blackboard.MaxHealth:0.##}.");
        if (_blackboard.CurrentHealth <= 0f)
            GD.Print("[Player] HP reached 0. Add your death/respawn handling here.");
    }

    // Example upgrade hook. Call this from your upgrade system.
    public void ApplyExampleUpgrade()
    {
        _blackboard.AttackDamage += 5f;
        _blackboard.MaxHealth += 20f;
        _blackboard.SetHealth(_blackboard.CurrentHealth + 20f);
        GD.Print($"[Upgrade] Applied example upgrade. AttackDamage={_blackboard.AttackDamage:0.##}, MaxHealth={_blackboard.MaxHealth:0.##}, HP={_blackboard.CurrentHealth:0.##}.");
    }
}
