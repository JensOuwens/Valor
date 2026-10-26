using Godot;
using System;

/// <summary>
/// this class handles the input for healing
/// </summary>
public class HealCommand : ICommand
{
	public CommandManager CommandManager { get; set; }
	
	public HealCommand (CommandManager manager)
	{
		this.CommandManager = manager;
	}
	
	public void OnClick(CommandContext context)
	{
		CommandManager.SendCommand(context, EInputType.Click, 2);
	}

	public void OnHold(CommandContext context)
	{
		CommandManager.SendCommand(context, EInputType.Hold, 2);
	}

	public void OnRelease(CommandContext context)
	{
		CommandManager.SendCommand(context, EInputType.Release, 2);
	}
}
