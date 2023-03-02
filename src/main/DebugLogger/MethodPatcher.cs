namespace Logging;

using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

internal static class MethodPatcher
{
    private static TextWriter GetWriter(object instance)
    {
        if (instance == null)
            return Console.Out;
            
        var clsFields = instance.GetType().GetFields(BindingFlags.FlattenHierarchy |
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Static);
        var loggerField = clsFields.FirstOrDefault(f => f.FieldType == typeof(DebugLogger));
        var logger = (DebugLogger?)loggerField?.GetValue(instance);

        return logger?.Configs.OutWriter ?? Console.Out;
    }

    private static void PrintContext(object __instance, 
        MethodBase __originalMethod, 
        params object[] __args)
    {
        var writer = GetWriter(__instance);
        writer.WriteLine($"{__originalMethod.DeclaringType.Name} - {__originalMethod.Name}");

        var paramsInfo = __originalMethod.GetParameters();
        for (int i = 0; i < paramsInfo.Length; i++)
        {
            writer.WriteLine($"{paramsInfo[i].Name}: {__args[i]}");
        }
    }
}