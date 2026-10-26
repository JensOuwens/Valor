using Godot;
using System;

/// <summary>
/// this class handles player input
/// </summary>
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
		// ATTACK: press, hold, release
		if (Input.IsActionJustPressed("Attack"))
			AttackCommand.OnClick(new CommandContext());

		if (Input.IsActionPressed("Attack"))
			AttackCommand.OnHold(new CommandContext());

		if (Input.IsActionJustReleased("Attack"))
			AttackCommand.OnRelease(new CommandContext());


		// HEAL: execute once on press
		if (Input.IsActionJustPressed("Heal"))
			HealCommand.OnClick(new CommandContext());


		// DODGE: execute once on press
		if (Input.IsActionJustPressed("Dodge"))
			DodgeCommand.OnClick(new CommandContext());


		// MOVEMENT: direction and continued movement
		Vector2 direction = Input.GetVector(
			"MoveLeft", "MoveRight", "MoveUp", "MoveDown"
		);

		CommandContext moveContext = new CommandContext
		{
			Direction = direction
		};

		if (direction != Vector2.Zero)
		{
			if (!_wasMoving)
				MoveCommand.OnClick(moveContext);

			MoveCommand.OnHold(moveContext);
			_wasMoving = true;
		}
		else if (_wasMoving)
		{
			MoveCommand.OnRelease(moveContext);
			_wasMoving = false;
		}
	}

	private bool _wasMoving = false;
}
