using Godot;

namespace Valor;

/// <summary>
/// this class handles player movement logic
/// </summary>
public class MovementHandler : IHandler
{
	private readonly PlayerBlackboard blackboard;
	private Vector2 lastLoggedDirection = Vector2.Zero;

	public MovementHandler(PlayerBlackboard blackboard) => this.blackboard = blackboard;

	public void OnCLick(CommandContext context)
	{
		GD.Print("[Movement] Movement started.");
		OnHold(context);
	}

	public void OnHold(CommandContext context)
	{
		if (this.blackboard.Player == null || this.blackboard.IsDodging)
			return;

		Vector2 direction = context.Direction.Normalized();
		this.blackboard.Player.Velocity = new Vector3(direction.X, 0f, direction.Y) * this.blackboard.MoveSpeed;

		if (direction == this.lastLoggedDirection) return;
		this.lastLoggedDirection = direction;
		GD.Print($"[Movement] Direction={direction}, speed={this.blackboard.MoveSpeed:0.##} px/s, velocity={this.blackboard.Player.Velocity}.");
	}

	public void OnRelease(CommandContext context)
	{
		if (this.blackboard.Player == null || this.blackboard.IsDodging)
			return;

		this.blackboard.Player.Velocity = Vector3.Zero;
		this.lastLoggedDirection = Vector2.Zero;
		GD.Print("[Movement] Movement stopped.");
	}

	public void Tick(double delta)
	{
		this.blackboard.Player?.MoveAndSlide();
	}
}