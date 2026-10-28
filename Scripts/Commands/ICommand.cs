namespace Valor;

/// <summary>
/// this interface is used by all commands in the command pattern
/// </summary>
public interface ICommand
{
	public CommandManager CommandManager { get; set; }

	public void OnClick(CommandContext context);
	public void OnHold(CommandContext context);
	public void OnRelease(CommandContext context);
}