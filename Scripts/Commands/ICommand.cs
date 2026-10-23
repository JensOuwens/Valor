using Godot;
using System;

public interface ICommand
{
	public CommandManager CommandManager { get; set; }

	public void OnClick(CommandContext context);
	public void OnHold(CommandContext context);
	public void OnRelease(CommandContext context);
}
