using Godot;
using System;

/// <summary>
/// this class handles dodging logic
/// </summary>
public partial class DodgeHandler : Node, IHandler
{
	public void OnCLick(CommandContext context)
	{
		GD.Print("Dodge");
	}

	public void OnHold(CommandContext context)
	{
		throw new NotImplementedException();
	}

	public void OnRelease(CommandContext context)
	{
		throw new NotImplementedException();
	}
}
