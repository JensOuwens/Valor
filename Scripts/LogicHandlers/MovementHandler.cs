using Godot;
using System;

public partial class MovementHandler : Node, IHandler
{
	public void OnCLick(CommandContext context)
	{
		GD.Print("CLICK");
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
