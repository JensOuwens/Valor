using System.Collections.Generic;

namespace Valor;

/// <summary>
/// This class accepts commands from the command classes, and redirects them to the apropriate handlers
/// </summary>
public class CommandManager
{
	private readonly Dictionary<HandlerId, IHandler> handlers = new();

	public void InitializeHandlers(PlayerBlackboard blackboard)
	{
		this.handlers.Clear();

		this.handlers.Add(
			HandlerId.Movement, new MovementHandler(blackboard));

		this.handlers.Add(
			HandlerId.Attack, new AttackHandler(blackboard));

		this.handlers.Add(
			HandlerId.Heal, new HealHandler(blackboard));

		this.handlers.Add(
			HandlerId.Dodge, new DodgeHandler(blackboard));
	}

	public void SendCommand(
		CommandContext context,
		EInputType inputType,
		HandlerId handlerId)
	{
		if (!this.handlers.TryGetValue(handlerId, out IHandler handler))
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
		foreach (IHandler handler in this.handlers.Values)
			handler.Tick(delta);
	}
	
	public T GetHandler<T>(HandlerId id) where T : class, IHandler
	{
		if (this.handlers.TryGetValue(id, out IHandler handler))
			return handler as T;
		return null;
	}
}