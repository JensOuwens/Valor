using Godot;

namespace Valor;

/// <summary>
/// this class provides context for some commands wich need directional data.
/// </summary>
public partial class CommandContext
{
	public Vector3 Direction { get; set; } = Vector3.Zero;
	public PlayerBlackboard Blackboard { get; set; }
}