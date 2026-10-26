using Godot;
using System;

public partial class PlayerScript : Node
{
	ICommand AttackCommand;
	ICommand DodgeCommand;
	ICommand HealCommand;
	ICommand MoveCommand;
	
	CommandManager comandmanager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		IntializeCommands();
	}

	private void IntializeCommands()
	{
		comandmanager = new CommandManager();
		comandmanager.InitializeHandlers();
		
		AttackCommand = new AttackCommand(comandmanager);
		DodgeCommand = new DodgeCommand(comandmanager);
		HealCommand = new HealCommand(comandmanager);
		MoveCommand = new MoveCommand(comandmanager);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("Jump"))
		{
			
		}
	}
}
