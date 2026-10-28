using Godot;

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
	public float MoveSpeed { get; set; } = 200f;

	// Attacks
	public float AttackDamage { get; set; } = 20f;
	public float ChargedDamageMultiplier { get; set; } = 2.5f;
	public float ChargeTime { get; set; } = 1.0f;
	public int MaxComboHits { get; set; } = 3;
	public float ComboResetTime { get; set; } = 0.8f;

	// Dodge
	public float DodgeDistance { get; set; } = 100f;
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

	public CharacterBody2D Player { get; set; }

	public bool CanHeal => CurrentFlasks > 0 && CurrentHealth > 0f && CurrentHealth < MaxHealth;
	public bool CanDodge => !IsDodging && DodgeCooldownTimer <= 0f;

	public void SetHealth(float value)
	{
		CurrentHealth = Mathf.Clamp(value, 0f, Mathf.Max(0f, MaxHealth));
	}

	public void SetFlasks(int value)
	{
		CurrentFlasks = Mathf.Clamp(value, 0, Mathf.Max(0, MaxFlasks));
	}
}
