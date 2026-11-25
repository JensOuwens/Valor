using System;
using Godot;

namespace Valor;

public class SpeakCommand : ICommand
{
    public readonly Action npcDialogueEntered;
    public CommandManager CommandManager { get; set; }

    public SpeakCommand(CommandManager manager, Action npcDialogueEntered)
    {
        this.CommandManager = manager;
        this.npcDialogueEntered = npcDialogueEntered;
    }
    
    public void OnClick(CommandContext context)
    {
        GD.Print("[Input] Speak command triggered.");
        this.npcDialogueEntered.Invoke();
        this.CommandManager.SendCommand(context, EInputType.Click, HandlerId.Speak);
    }

    public void OnHold(CommandContext context) =>
        this.CommandManager.SendCommand(context, EInputType.Hold, HandlerId.Speak);

    public void OnRelease(CommandContext context) =>
        this.CommandManager.SendCommand(context, EInputType.Release, HandlerId.Speak);
}