using Godot;

/// <summary>
/// this class handles player movement logic
/// </summary>
public class MovementHandler : IHandler
{
	private readonly PlayerBlackboard _blackboard;
	private Vector2 _lastLoggedDirection = Vector2.Zero;

	public MovementHandler(PlayerBlackboard blackboard) => _blackboard = blackboard;

	public void OnCLick(CommandContext context)
	{
		GD.Print("[Movement] Movement started.");
		OnHold(context);
	}

	public void OnHold(CommandContext context)
	{
		if (_blackboard.Player == null || _blackboard.IsDodging)
			return;

		Vector2 direction = context.Direction.Normalized();
		_blackboard.Player.Velocity = direction * _blackboard.MoveSpeed;

		if (direction != _lastLoggedDirection)
		{
			_lastLoggedDirection = direction;
			GD.Print($"[Movement] Direction={direction}, speed={_blackboard.MoveSpeed:0.##} px/s, velocity={_blackboard.Player.Velocity}.");
		}
	}

	public void OnRelease(CommandContext context)
	{
		if (_blackboard.Player == null || _blackboard.IsDodging)
			return;

		_blackboard.Player.Velocity = Vector2.Zero;
		_lastLoggedDirection = Vector2.Zero;
		GD.Print("[Movement] Movement stopped.");
	}

	public void Tick(double delta) { }
}
