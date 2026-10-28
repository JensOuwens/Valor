using Godot;
using System;

/// <summary>
/// this class handles the input for healing
/// </summary>
public class HealCommand : ICommand
{
	public CommandManager CommandManager { get; set; }
	
	public HealCommand(CommandManager manager)
	{
		this.CommandManager = manager;
	}
	
	public void OnClick(CommandContext context) =>
		CommandManager.SendCommand(
			context, EInputType.Click, HandlerId.Heal);

	public void OnHold(CommandContext context) =>
		CommandManager.SendCommand(
			context, EInputType.Hold, HandlerId.Heal);

	public void OnRelease(CommandContext context) =>
		CommandManager.SendCommand(
			context, EInputType.Release, HandlerId.Heal);
}
