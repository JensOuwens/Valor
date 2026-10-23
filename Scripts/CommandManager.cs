using Godot;
using System;
using System.Collections.Generic;

public enum EInputType
{
	Click,
	Hold,
	Release
}
public  class CommandManager
{
	public List<IHandler> handlers = new List<IHandler>();
	public void SendCommand(CommandContext context, EInputType inputType, int handlerID)
	{
		switch (inputType)
		{
			case EInputType.Click:
				handlers[handlerID].OnCLick(context);
				break;
			case EInputType.Hold:
				handlers[handlerID].OnHold();
				break;
			case EInputType.Release:
				handlers[handlerID].OnRelease();
				break;
		}
	}
}
