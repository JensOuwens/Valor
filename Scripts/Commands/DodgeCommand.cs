namespace Valor;

/// <summary>
/// this class handles the input for Dodging
/// </summary>
public class DodgeCommand : ICommand
{
	public CommandManager CommandManager { get; set; }

	public DodgeCommand (CommandManager manager)
	{
		this.CommandManager = manager;
	}
	
	public void OnClick(CommandContext context) =>
		this.CommandManager.SendCommand(
			context, EInputType.Click, HandlerId.Dodge);

	public void OnHold(CommandContext context) =>
		this.CommandManager.SendCommand(
			context, EInputType.Hold, HandlerId.Dodge);

	public void OnRelease(CommandContext context) =>
		this.CommandManager.SendCommand(
			context, EInputType.Release, HandlerId.Dodge);
}