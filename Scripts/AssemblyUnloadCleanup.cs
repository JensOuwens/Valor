using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;

namespace Valor;

internal static class AssemblyUnloadCleanup
{
    [ModuleInitializer]
    internal static void Register()
    {
        AssemblyLoadContext? alc = AssemblyLoadContext.GetLoadContext(typeof(AssemblyUnloadCleanup).Assembly);
        if (alc == null)
            return;

        alc.Unloading += _ =>
        {
            try
            {
                Assembly jsonAssembly = typeof(JsonSerializerOptions).Assembly;
                Type? handlerType = jsonAssembly.GetType("System.Text.Json.JsonSerializerOptionsUpdateHandler");
                MethodInfo? clearCache = handlerType?.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
                clearCache?.Invoke(null, new object?[] { null });
            }
            catch
            {
                // Ignore if runtime internals differ across .NET versions.
            }
        };
    }
}