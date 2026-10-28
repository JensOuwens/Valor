using Godot;

namespace Valor;

/// <summary>
/// this class handles dodging logic
/// </summary>
public class DodgeHandler : IHandler
{
	private readonly PlayerBlackboard blackboard;
	private Vector3 dodgeDirection;
	private bool iFramesEndedLogged;

	public DodgeHandler(PlayerBlackboard blackboard) => this.blackboard = blackboard;

	public void OnCLick(CommandContext context)
	{
		if (this.blackboard.Player == null)
		{
			GD.Print("[Dodge] Rejected: player reference is missing.");
			return;
		}

		if (!this.blackboard.CanDodge)
		{
			GD.Print($"[Dodge] Rejected: dodge active or cooldown remaining ({this.blackboard.DodgeCooldownTimer:0.##}s).");
			return;
		}
		
		this.dodgeDirection = context.Direction.Normalized();
		if (this.dodgeDirection == Vector3.Zero)
			this.dodgeDirection = Vector3.Forward;

		float duration = Mathf.Max(0.01f, this.blackboard.DodgeDuration);
		this.blackboard.PlayerVisual.PlayAnim(new PlayerVisual.PlayerAnimation{name = PlayerVisual.EPlayerAnimNames.Dodge, priority = 1, duration = duration});
		this.blackboard.IsDodging = true;
		this.blackboard.IsInvincible = true;
		this.blackboard.DodgeTimer = duration;
		this.blackboard.DodgeCooldownTimer = Mathf.Max(0f, this.blackboard.DodgeCooldown);
		this.iFramesEndedLogged = false;

		ApplyDodgeVelocity(duration);
		GD.Print($"[Dodge] Started. Direction={this.dodgeDirection}, distance={this.blackboard.DodgeDistance:0.##}, duration={duration:0.##}s, invincible for {duration * Mathf.Clamp(this.blackboard.DodgeIFrameFraction, 0f, 1f):0.##}s.");
	}

	private void ApplyDodgeVelocity(float duration)
	{
		if (this.blackboard.Player == null)
			return;

		this.blackboard.Player.Velocity = this.dodgeDirection * (this.blackboard.DodgeDistance / duration);
	}

	public void OnHold(CommandContext context) { }
	public void OnRelease(CommandContext context) { }

	public void Tick(double delta)
	{
		float dt = (float)delta;

		if (this.blackboard.DodgeCooldownTimer > 0f) 
			this.blackboard.DodgeCooldownTimer = Mathf.Max(0f, this.blackboard.DodgeCooldownTimer - dt);

		if (!this.blackboard.IsDodging) return;

		this.blackboard.DodgeTimer -= dt;
		float duration = Mathf.Max(0.01f, this.blackboard.DodgeDuration);
		float elapsed = duration - Mathf.Max(0f, this.blackboard.DodgeTimer);
		float iFrameDuration = duration * Mathf.Clamp(this.blackboard.DodgeIFrameFraction, 0f, 1f);

		if (!this.iFramesEndedLogged && elapsed >= iFrameDuration)
		{
			this.blackboard.IsInvincible = false;
			this.iFramesEndedLogged = true;
			GD.Print("[Dodge] Invincibility frames ended; remaining dodge movement is vulnerable.");
		}

		if (this.blackboard.DodgeTimer <= 0f)
		{
			this.blackboard.DodgeTimer = 0f;
			this.blackboard.IsDodging = false;
			this.blackboard.IsInvincible = false;
			if (this.blackboard.Player != null)
				this.blackboard.Player.Velocity = Vector3.Zero;
			GD.Print($"[Dodge] Finished. Cooldown remaining={this.blackboard.DodgeCooldownTimer:0.##}s.");
		}
		else
			ApplyDodgeVelocity(duration);
	}
}