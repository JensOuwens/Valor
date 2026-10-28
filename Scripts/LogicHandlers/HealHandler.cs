using Godot;
using System;

/// <summary>
/// this class handles healing logic
/// </summary>
public class HealHandler : IHandler
{
	private readonly PlayerBlackboard _blackboard;

	public HealHandler(PlayerBlackboard blackboard) => _blackboard = blackboard;

	public void OnCLick(CommandContext context)
	{
		if (_blackboard.CurrentHealth <= 0f)
		{
			GD.Print("[Heal] Rejected: player is dead.");
			return;
		}

		if (_blackboard.CurrentFlasks <= 0)
		{
			GD.Print("[Heal] Rejected: no flasks remaining.");
			return;
		}

		if (_blackboard.CurrentHealth >= _blackboard.MaxHealth)
		{
			GD.Print("[Heal] Rejected: health is already full. No flask consumed.");
			return;
		}

		float before = _blackboard.CurrentHealth;
		_blackboard.SetHealth(before + _blackboard.HealAmount);
		_blackboard.SetFlasks(_blackboard.CurrentFlasks - 1);

		GD.Print($"[Heal] Flask used. HP {before:0.##} -> {_blackboard.CurrentHealth:0.##}/{_blackboard.MaxHealth:0.##}. Flasks={_blackboard.CurrentFlasks}/{_blackboard.MaxFlasks}.");
	}

	public void OnHold(CommandContext context) { }
	public void OnRelease(CommandContext context) { }
	public void Tick(double delta) { }
}
