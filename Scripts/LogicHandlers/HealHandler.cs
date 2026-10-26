using Godot;
using System;

public partial class HealHandler : Node, IHandler
{
	/// <summary>
	/// this class handles healing logic
	/// </summary>
	/// <param name="context"></param>
	public void OnCLick(CommandContext context)
	{
		GD.Print("Heal");
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
