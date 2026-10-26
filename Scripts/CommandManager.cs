using Godot;
using System;
using System.Collections.Generic;

public enum EInputType
{
	Click,
	Hold,
	Release
}
/// <summary>
/// This class accepts commands from the command classes, and redirects them to the apropriate handlers
/// </summary>
public  class CommandManager
{
	public List<IHandler> handlers = new List<IHandler>();

	public void InitializeHandlers()
	{
		IHandler movementHandler = new MovementHandler();
		IHandler attackHandler = new AttackHandler();
		IHandler healHandler = new HealHandler();
		IHandler dodgeHandler = new DodgeHandler();
		
		handlers.Add(movementHandler);
		handlers.Add(attackHandler);
		handlers.Add(healHandler);
		handlers.Add(dodgeHandler);
	}
	
	public void SendCommand(CommandContext context, EInputType inputType, int handlerID)
	{
		switch (inputType)
		{
			case EInputType.Click:
				handlers[handlerID].OnCLick(context);
				break;
			case EInputType.Hold:
				handlers[handlerID].OnHold(context);
				break;
			case EInputType.Release:
				handlers[handlerID].OnRelease(context);
				break;
		}
	}
}
