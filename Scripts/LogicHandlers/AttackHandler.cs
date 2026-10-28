using Godot;
using System;

/// <summary>
/// this class handles attack logic
/// </summary>
using System;

public class AttackHandler : IHandler
{
	private readonly PlayerBlackboard _blackboard;

	public AttackMode Mode { get; set; } = AttackMode.Combo;
	public event Action<int, float> AttackTriggered;
	public event Action<float> ChargedAttackTriggered;

	private int _comboStep;
	private float _comboTimer;
	private float _chargeTimer;
	private bool _attackHeld;
	private bool _chargeReadyLogged;

	public AttackHandler(PlayerBlackboard blackboard) => _blackboard = blackboard;

	public void OnCLick(CommandContext context)
	{
		if (_blackboard.IsDodging)
		{
			GD.Print("[Attack] Input ignored: player is dodging.");
			return;
		}

		if (Mode == AttackMode.Combo)
		{
			ExecuteComboHit();
			return;
		}

		if (_attackHeld)
		{
			GD.Print("[Attack] Charge input ignored: attack is already being held.");
			return;
		}

		_attackHeld = true;
		_chargeTimer = 0f;
		_chargeReadyLogged = false;
		_blackboard.IsChargingAttack = true;
		GD.Print($"[Attack] Charge started. Hold for at least {_blackboard.ChargeTime:0.##} seconds.");
	}

	public void OnHold(CommandContext context)
	{
		// Charge timing is accumulated once per physics tick in Tick().
	}

	public void OnRelease(CommandContext context)
	{
		if (Mode != AttackMode.Charge || !_attackHeld)
			return;

		_attackHeld = false;
		_blackboard.IsChargingAttack = false;

		if (_blackboard.IsDodging)
		{
			GD.Print("[Attack] Charge canceled because the player started dodging.");
			_chargeTimer = 0f;
			return;
		}

		if (_chargeTimer >= Mathf.Max(0f, _blackboard.ChargeTime))
		{
			float damage = _blackboard.AttackDamage * _blackboard.ChargedDamageMultiplier;
			GD.Print($"[Attack] Charged attack released after {_chargeTimer:0.##}s. Damage={damage:0.##}.");
			ChargedAttackTriggered?.Invoke(damage);
		}
		else
		{
			GD.Print($"[Attack] Charge canceled: held {_chargeTimer:0.##}s; required {_blackboard.ChargeTime:0.##}s. No attack fired.");
		}

		_chargeTimer = 0f;
	}

	private void ExecuteComboHit()
	{
		int maxHits = Mathf.Max(1, _blackboard.MaxComboHits);
		_comboStep++;
		float damage = _blackboard.AttackDamage;

		GD.Print($"[Attack] Combo hit {_comboStep}/{maxHits}. Damage={damage:0.##}.");
		AttackTriggered?.Invoke(_comboStep, damage);

		if (_comboStep >= maxHits)
		{
			GD.Print("[Attack] Full combo completed.");
			_comboStep = 0;
			_comboTimer = 0f;
		}
		else
		{
			_comboTimer = Mathf.Max(0f, _blackboard.ComboResetTime);
		}
	}

	public void Tick(double delta)
	{
		float dt = (float)delta;

		if (_comboTimer > 0f)
		{
			_comboTimer -= dt;
			if (_comboTimer <= 0f && _comboStep > 0)
			{
				_comboTimer = 0f;
				_comboStep = 0;
				GD.Print("[Attack] Combo window expired; next click starts a new combo.");
			}
		}

		if (!_attackHeld)
			return;

		if (_blackboard.IsDodging)
		{
			_attackHeld = false;
			_blackboard.IsChargingAttack = false;
			_chargeTimer = 0f;
			GD.Print("[Attack] Charge canceled because the player is dodging.");
			return;
		}

		_chargeTimer += dt;
		if (!_chargeReadyLogged && _chargeTimer >= Mathf.Max(0f, _blackboard.ChargeTime))
		{
			_chargeReadyLogged = true;
			GD.Print("[Attack] Charge threshold reached. Release Attack to execute.");
		}
	}
}
