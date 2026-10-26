using Godot;
using System;

/// <summary>
/// this class handles player movement logic
/// </summary>
public partial class MovementHandler : Node, IHandler
{
	public void OnCLick(CommandContext context)
	{
		GD.Print("Movement started: ", context.Direction);
	}

	public void OnHold(CommandContext context)
	{
		GD.Print("Moving in direction: ", context.Direction);
	}

	public void OnRelease(CommandContext context)
	{
		GD.Print("Movement stopped");
	}
}
