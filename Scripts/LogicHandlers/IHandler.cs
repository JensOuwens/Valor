using Godot;
using System;

public partial class IHandler
{
    public CommandContext thisCommandContext;
    public void OnCLick(CommandContext context)
    {
        thisCommandContext = context;
    }

    public void OnHold()
    {
        
    }

    public void OnRelease()
    {
        
    }
}
