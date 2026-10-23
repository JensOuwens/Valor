using Godot;
using System;

public class HealCommand : ICommand
{
    public CommandManager CommandManager { get; set; }
    
    public void OnClick(CommandContext context)
    {
        CommandManager.SendCommand(context, EInputType.Click, 0);
    }

    public void OnHold(CommandContext context)
    {
        CommandManager.SendCommand(context, EInputType.Hold, 0);
    }

    public void OnRelease(CommandContext context)
    {
        CommandManager.SendCommand(context, EInputType.Release, 0);
    }
}
