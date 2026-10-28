using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// This class accepts commands from the command classes, and redirects them to the apropriate handlers
/// </summary>

public class CommandManager
{
	private readonly Dictionary<HandlerId, IHandler> _handlers = new();

	public void InitializeHandlers(PlayerBlackboard blackboard)
	{
		_handlers.Clear();

		_handlers.Add(
			HandlerId.Movement, new MovementHandler(blackboard));

		_handlers.Add(
			HandlerId.Attack, new AttackHandler(blackboard));

		_handlers.Add(
			HandlerId.Heal, new HealHandler(blackboard));

		_handlers.Add(
			HandlerId.Dodge, new DodgeHandler(blackboard));
	}

	public void SendCommand(
		CommandContext context,
		EInputType inputType,
		HandlerId handlerId)
	{
		if (!_handlers.TryGetValue(handlerId, out IHandler handler))
			return;

		switch (inputType)
		{
			case EInputType.Click:
				handler.OnCLick(context);
				break;

			case EInputType.Hold:
				handler.OnHold(context);
				break;

			case EInputType.Release:
				handler.OnRelease(context);
				break;
		}
	}

	public void Tick(double delta)
	{
		foreach (IHandler handler in _handlers.Values)
			handler.Tick(delta);
	}
	
	public T GetHandler<T>(HandlerId id) where T : class, IHandler
	{
		if (_handlers.TryGetValue(id, out IHandler handler))
			return handler as T;
		return null;
	}
}
