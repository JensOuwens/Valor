using Godot;
using System;

public interface IHandler
{
    public void OnCLick(CommandContext context);

    public void OnHold(CommandContext context);

    public void OnRelease(CommandContext context);
}
