namespace Valor;

/// <summary>
/// this class handles the input for Moving
/// </summary>
public class MoveCommand : ICommand
{
	public CommandManager CommandManager { get; set; }

	public MoveCommand(CommandManager manager)
	{
		this.CommandManager = manager;
	}

	public void OnClick(CommandContext context) =>
		this.CommandManager.SendCommand(
			context, EInputType.Click, HandlerId.Movement);

	public void OnHold(CommandContext context) =>
		this.CommandManager.SendCommand(
			context, EInputType.Hold, HandlerId.Movement);

	public void OnRelease(CommandContext context) =>
		this.CommandManager.SendCommand(
			context, EInputType.Release, HandlerId.Movement);
}