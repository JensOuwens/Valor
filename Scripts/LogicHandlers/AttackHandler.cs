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

		if (this.attackHeld)
			return;

		this.attackHeld = true;
		this.chargeTimer = 0f;
		this.chargeReadyLogged = false;

		this.blackboard.IsChargingAttack = true;

		GD.Print("[Attack] Attack button held.");
	}

	public void OnHold(CommandContext context)
	{
		// Charge timing is accumulated once per physics tick in Tick().
	}

	public void OnRelease(CommandContext context)
	{
		if (!this.attackHeld)
			return;

		this.attackHeld = false;
		this.blackboard.IsChargingAttack = false;

		if (this.blackboard.IsDodging)
		{
			this.chargeTimer = 0f;
			return;
		}

		float requiredTime = Mathf.Max(0f, this.blackboard.ChargeTime);

		if (this.chargeTimer >= requiredTime)
		{
			float damage = this.blackboard.AttackDamage *
			               this.blackboard.ChargedDamageMultiplier;

			GD.Print($"[Attack] Charged attack executed! Damage: {damage:0.##}");

			this.ChargedAttackTriggered?.Invoke(damage);
		}
		else
		{
			if (this.Mode == AttackMode.Combo)
			{
				this.blackboard.PlayerVisual.PlayAnim(
					new PlayerVisual.PlayerAnimation
					{
						name = PlayerVisual.EPlayerAnimNames.Attack,
						priority = 1,
						callback = ExecuteComboHit
					});
			}

			GD.Print("[Attack] Normal attack executed.");
		}

		this.chargeTimer = 0f;
		this.chargeReadyLogged = false;
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