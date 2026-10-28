using System;
using Godot;
/// <summary>
/// this class handles attack logic
/// </summary>

namespace Valor;

public class AttackHandler : IHandler
{
	private readonly PlayerBlackboard blackboard;

	public AttackMode Mode { get; set; } = AttackMode.Combo;
	public event Action<int, float> AttackTriggered;
	public event Action<float> ChargedAttackTriggered;

	private int comboStep;
	private float comboTimer;
	private float chargeTimer;
	private bool attackHeld;
	private bool chargeReadyLogged;

	public AttackHandler(PlayerBlackboard blackboard) => this.blackboard = blackboard;

	public void OnCLick(CommandContext context)
	{
		if (this.blackboard.IsDodging)
		{
			GD.Print("[Attack] Input ignored: player is dodging.");
			return;
		}
		

		if (this.Mode == AttackMode.Combo)
		{
			this.blackboard.PlayerVisual.PlayAnim(new PlayerVisual.PlayerAnimation
			{
				name = PlayerVisual.EPlayerAnimNames.Attack, 
				priority = 1, 
				callback = ExecuteComboHit
			});
			return;
		}

		if (this.attackHeld)
		{
			GD.Print("[Attack] Charge input ignored: attack is already being held.");
			return;
		}

		this.attackHeld = true;
		this.chargeTimer = 0f;
		this.chargeReadyLogged = false;
		this.blackboard.IsChargingAttack = true;
		GD.Print($"[Attack] Charge started. Hold for at least {this.blackboard.ChargeTime:0.##} seconds.");
	}

	public void OnHold(CommandContext context)
	{
		// Charge timing is accumulated once per physics tick in Tick().
	}

	public void OnRelease(CommandContext context)
	{
		if (this.Mode != AttackMode.Charge || !this.attackHeld)
			return;

		this.attackHeld = false;
		this.blackboard.IsChargingAttack = false;

		if (this.blackboard.IsDodging)
		{
			GD.Print("[Attack] Charge canceled because the player started dodging.");
			this.chargeTimer = 0f;
			return;
		}

		if (this.chargeTimer >= Mathf.Max(0f, this.blackboard.ChargeTime))
		{
			float damage = this.blackboard.AttackDamage * this.blackboard.ChargedDamageMultiplier;
			GD.Print($"[Attack] Charged attack released after {this.chargeTimer:0.##}s. Damage={damage:0.##}.");
			this.ChargedAttackTriggered?.Invoke(damage);
		}
		else
		{
			GD.Print($"[Attack] Charge canceled: held {this.chargeTimer:0.##}s; required {this.blackboard.ChargeTime:0.##}s. No attack fired.");
		}

		this.chargeTimer = 0f;
	}

	private void ExecuteComboHit()
	{
		int maxHits = Mathf.Max(1, this.blackboard.MaxComboHits);
		this.comboStep++;
		float damage = this.blackboard.AttackDamage;

		GD.Print($"[Attack] Combo hit {this.comboStep}/{maxHits}. Damage={damage:0.##}.");
		this.AttackTriggered?.Invoke(this.comboStep, damage);

		if (this.comboStep >= maxHits)
		{
			GD.Print("[Attack] Full combo completed.");
			this.comboStep = 0;
			this.comboTimer = 0f;
		}
		else
		{
			this.comboTimer = Mathf.Max(0f, this.blackboard.ComboResetTime);
		}
	}

	public void Tick(double delta)
	{
		float dt = (float)delta;

		if (this.comboTimer > 0f)
		{
			this.comboTimer -= dt;
			if (this.comboTimer <= 0f && this.comboStep > 0)
			{
				this.comboTimer = 0f;
				this.comboStep = 0;
				GD.Print("[Attack] Combo window expired; next click starts a new combo.");
			}
		}

		if (!this.attackHeld)
			return;

		if (this.blackboard.IsDodging)
		{
			this.attackHeld = false;
			this.blackboard.IsChargingAttack = false;
			this.chargeTimer = 0f;
			GD.Print("[Attack] Charge canceled because the player is dodging.");
			return;
		}

		this.chargeTimer += dt;
		if (!this.chargeReadyLogged && this.chargeTimer >= Mathf.Max(0f, this.blackboard.ChargeTime))
		{
			this.chargeReadyLogged = true;
			GD.Print("[Attack] Charge threshold reached. Release Attack to execute.");
		}
	}
	
	
}