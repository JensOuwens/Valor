using Godot;

namespace Valor;

/// <summary>
/// this class handles player movement logic
/// </summary>
public class MovementHandler : IHandler
{
	private readonly PlayerBlackboard blackboard;
	private Vector3 lastLoggedDirection = Vector3.Zero;

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

		Vector3 direction = context.Direction.Normalized();
		this.blackboard.Player.Velocity = direction * this.blackboard.MoveSpeed;
		
		PlayerVisual visual = this.blackboard.PlayerVisual;
		if (visual != null) visual.Rotation = new Vector3(0f, new Vector2(direction.Z, direction.X).Angle(), 0f);;

		if (direction == this.lastLoggedDirection) return;
		this.lastLoggedDirection = direction;
		GD.Print($"[Movement] Direction={direction}, speed={this.blackboard.MoveSpeed:0.##} px/s, velocity={this.blackboard.Player.Velocity}.");
	}

	public void OnRelease(CommandContext context)
	{
		if (this.blackboard.Player == null || this.blackboard.IsDodging) return;

		this.blackboard.Player.Velocity = Vector3.Zero;
		this.lastLoggedDirection = Vector3.Zero;
		GD.Print("[Movement] Movement stopped.");
	}

	public void Tick(double delta) => this.blackboard.Player?.MoveAndSlide();
}