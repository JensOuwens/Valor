using Godot;
using System;
using System.Collections.Generic;

public partial class AnimationFunctionBank : Node
{
    private Dictionary<string, Action> functions = new();

    public void RegisterFunction(string name, Action callback)
    {
        if (!this.functions.TryAdd(name, callback)) this.functions[name] = callback;
    }
    
    public void CallFunction(string name)
    {
        this.functions.TryGetValue(name, out Action callback);
        callback?.Invoke();
    }
}
