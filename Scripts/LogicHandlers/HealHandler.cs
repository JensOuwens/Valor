using Godot;

namespace Valor;

/// <summary>
/// this class handles healing logic
/// </summary>
public class HealHandler : IHandler
{
	private readonly PlayerBlackboard blackboard;

	public HealHandler(PlayerBlackboard blackboard) => this.blackboard = blackboard;

	public void OnCLick(CommandContext context)
	{
		if (this.blackboard.CurrentHealth <= 0f)
		{
			GD.Print("[Heal] Rejected: player is dead.");
			return;
		}

		if (this.blackboard.CurrentFlasks <= 0)
		{
			GD.Print("[Heal] Rejected: no flasks remaining.");
			return;
		}

		if (this.blackboard.CurrentHealth >= this.blackboard.MaxHealth)
		{
			GD.Print("[Heal] Rejected: health is already full. No flask consumed.");
			return;
		}

		float before = this.blackboard.CurrentHealth;
		this.blackboard.SetHealth(before + this.blackboard.HealAmount);
		this.blackboard.SetFlasks(this.blackboard.CurrentFlasks - 1);

		GD.Print($"[Heal] Flask used. HP {before:0.##} -> {this.blackboard.CurrentHealth:0.##}/{this.blackboard.MaxHealth:0.##}. Flasks={this.blackboard.CurrentFlasks}/{this.blackboard.MaxFlasks}.");
	}

	public void OnHold(CommandContext context) { }
	public void OnRelease(CommandContext context) { }
	public void Tick(double delta) { }
}