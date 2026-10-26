using Godot;
using System;

public partial class AttackHandler : Node, IHandler
{
	/// <summary>
	/// this class handles attack logic
	/// </summary>
	/// <param name="context"></param>
	public void OnCLick(CommandContext context)
	{
		GD.Print("Attack");
	}

	public void OnHold(CommandContext context)
	{
		GD.Print("AttackHold");
	}

	public void OnRelease(CommandContext context)
	{
		GD.Print("SuperAttack");
	}
}
