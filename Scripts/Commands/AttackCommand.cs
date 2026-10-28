using Godot;
using System;

/// <summary>
/// this class handles the input for Attacking
/// </summary>
public class AttackCommand : ICommand
{
	public CommandManager CommandManager { get; set; }

	public AttackCommand(CommandManager manager)
	{
		this.CommandManager = manager;
	}

	public void OnClick(CommandContext context) =>
		CommandManager.SendCommand(
			context, EInputType.Click, HandlerId.Attack);

	public void OnHold(CommandContext context) =>
		CommandManager.SendCommand(
			context, EInputType.Hold, HandlerId.Attack);

	public void OnRelease(CommandContext context) =>
		CommandManager.SendCommand(
			context, EInputType.Release, HandlerId.Attack);
}
