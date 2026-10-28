namespace Valor;

/// <summary>
/// this is an interface that all logic handlers use
/// </summary>
public interface IHandler
{
    public void OnCLick(CommandContext context);

    public void OnHold(CommandContext context);

    public void OnRelease(CommandContext context);
    
    void Tick(double delta);
}