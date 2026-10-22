using Godot;
using System;

public interface ICommand
{
	public CommandManager CommandManager { get; set; }
	
	public void OnClick(CommandContext context)
	{
		//call command manager function
	}
	
	public void OnHold()
	{
		//call command manager function
	}

	public void OnRelease()
	{
		//call command manager function
	}
}
