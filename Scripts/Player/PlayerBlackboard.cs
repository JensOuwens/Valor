using Godot;

namespace Valor;

/// <summary>
/// Shared, runtime-only player state. Charm systems can modify the stat properties.
/// </summary>
public sealed class PlayerBlackboard
{
	// Health and healing
	public float MaxHealth { get; set; } = 100f;
	public float CurrentHealth { get; set; } = 100f;
	public float HealAmount { get; set; } = 40f;
	public int MaxFlasks { get; set; } = 3;
	public int CurrentFlasks { get; set; } = 3;

	// Movement
	public float MoveSpeed { get; set; } = 5f;

	// Attacks
	public float AttackDamage { get; set; } = 20f;
	public float ChargedDamageMultiplier { get; set; } = 2.5f;
	public float ChargeTime { get; set; } = 1.0f;
	public int MaxComboHits { get; set; } = 3;
	public float ComboResetTime { get; set; } = 0.8f;

	// Dodge
	public float DodgeDistance { get; set; } = 2f;
	public float DodgeDuration { get; set; } = 0.2f;
	public float DodgeCooldown { get; set; } = 0.5f;
	public float DodgeIFrameFraction { get; set; } = 0.65f;

	// Runtime state (not upgrade stats)
	public bool IsDodging { get; set; }
	public bool IsInvincible { get; set; }
	public bool IsAttacking { get; set; }
	public bool IsChargingAttack { get; set; }
	public float DodgeTimer { get; set; }
	public float DodgeCooldownTimer { get; set; }

	public CharacterBody3D Player { get; set; }
	public PlayerVisual PlayerVisual { get; set; }

	public bool CanHeal => this.CurrentFlasks > 0 && this.CurrentHealth > 0f && this.CurrentHealth < this.MaxHealth;
	public bool CanDodge => !this.IsDodging && this.DodgeCooldownTimer <= 0f;

	public void SetHealth(float value) => 
		this.CurrentHealth = Mathf.Clamp(value, 0f, Mathf.Max(0f, this.MaxHealth));

	public void SetFlasks(int value) => 
		this.CurrentFlasks = Mathf.Clamp(value, 0, Mathf.Max(0, this.MaxFlasks));
}