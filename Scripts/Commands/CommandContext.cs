using Godot;
using System;

/// <summary>
/// this class provides context for some commands wich need directional data.
/// </summary>
public partial class CommandContext
{
	public Vector2 Direction { get; set; } = Vector2.Zero;
	public PlayerBlackboard Blackboard { get; set; }
}
