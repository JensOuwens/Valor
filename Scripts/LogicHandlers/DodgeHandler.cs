using Godot;
using System;

/// <summary>
/// this class handles dodging logic
/// </summary>

public class DodgeHandler : IHandler
{
	private readonly PlayerBlackboard _blackboard;
	private Vector2 _dodgeDirection;
	private bool _iFramesEndedLogged;

	public DodgeHandler(PlayerBlackboard blackboard) => _blackboard = blackboard;

	public void OnCLick(CommandContext context)
	{
		if (_blackboard.Player == null)
		{
			GD.Print("[Dodge] Rejected: player reference is missing.");
			return;
		}

		if (!_blackboard.CanDodge)
		{
			GD.Print($"[Dodge] Rejected: dodge active or cooldown remaining ({_blackboard.DodgeCooldownTimer:0.##}s).");
			return;
		}

		_dodgeDirection = context.Direction.Normalized();
		if (_dodgeDirection == Vector2.Zero)
			_dodgeDirection = Vector2.Down;

		float duration = Mathf.Max(0.01f, _blackboard.DodgeDuration);
		_blackboard.IsDodging = true;
		_blackboard.IsInvincible = true;
		_blackboard.DodgeTimer = duration;
		_blackboard.DodgeCooldownTimer = Mathf.Max(0f, _blackboard.DodgeCooldown);
		_iFramesEndedLogged = false;

		ApplyDodgeVelocity(duration);
		GD.Print($"[Dodge] Started. Direction={_dodgeDirection}, distance={_blackboard.DodgeDistance:0.##}, duration={duration:0.##}s, invincible for {duration * Mathf.Clamp(_blackboard.DodgeIFrameFraction, 0f, 1f):0.##}s.");
	}

	private void ApplyDodgeVelocity(float duration)
	{
		if (_blackboard.Player == null)
			return;

		_blackboard.Player.Velocity = _dodgeDirection * (_blackboard.DodgeDistance / duration);
	}

	public void OnHold(CommandContext context) { }
	public void OnRelease(CommandContext context) { }

	public void Tick(double delta)
	{
		float dt = (float)delta;

		if (_blackboard.DodgeCooldownTimer > 0f)
		{
			_blackboard.DodgeCooldownTimer = Mathf.Max(0f, _blackboard.DodgeCooldownTimer - dt);
		}

		if (!_blackboard.IsDodging)
			return;

		_blackboard.DodgeTimer -= dt;
		float duration = Mathf.Max(0.01f, _blackboard.DodgeDuration);
		float elapsed = duration - Mathf.Max(0f, _blackboard.DodgeTimer);
		float iFrameDuration = duration * Mathf.Clamp(_blackboard.DodgeIFrameFraction, 0f, 1f);

		if (!_iFramesEndedLogged && elapsed >= iFrameDuration)
		{
			_blackboard.IsInvincible = false;
			_iFramesEndedLogged = true;
			GD.Print("[Dodge] Invincibility frames ended; remaining dodge movement is vulnerable.");
		}

		if (_blackboard.DodgeTimer <= 0f)
		{
			_blackboard.DodgeTimer = 0f;
			_blackboard.IsDodging = false;
			_blackboard.IsInvincible = false;
			if (_blackboard.Player != null)
				_blackboard.Player.Velocity = Vector2.Zero;
			GD.Print($"[Dodge] Finished. Cooldown remaining={_blackboard.DodgeCooldownTimer:0.##}s.");
		}
		else
		{
			ApplyDodgeVelocity(duration);
		}
	}
}
