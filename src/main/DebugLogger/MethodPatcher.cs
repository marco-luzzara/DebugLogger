namespace Logging;

using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

internal class MethodPatcher
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

    private static void PrintContext(object instance, 
        MethodBase originalMethod, 
        params object[] parameters)
    {
        var writer = GetWriter(instance);
        writer.WriteLine($"{originalMethod.DeclaringType.Name} - {originalMethod.Name}");

        var paramsInfo = originalMethod.GetParameters();
        for (int i = 0; i < paramsInfo.Length; i++)
        {
            writer.WriteLine($"{paramsInfo[i].Name}: {parameters[i]}");
        }
    }

    internal static void ContextLog_0(object __instance, MethodBase __originalMethod)
    {
        PrintContext(__instance, __originalMethod);
    }

    internal static void ContextLog_1(object __instance, MethodBase __originalMethod, 
        object __0)
    {
        PrintContext(__instance, __originalMethod, __0);
    }

    internal static void ContextLog_2(object __instance, MethodBase __originalMethod,
        object __0, object __1)
    {
        PrintContext(__instance, __originalMethod, __0, __1);
    }

    internal static void ContextLog_3(object __instance, MethodBase __originalMethod, 
        object __0, object __1, object __2)
    {
        PrintContext(__instance, __originalMethod, __0, __1, __2);
    }

    internal static void ContextLog_4(object __instance, MethodBase __originalMethod,
        object __0, object __1, object __2, object __3)
    {
        PrintContext(__instance, __originalMethod, __0, __1, __2, __3);
    }

    internal static void ContextLog_5(object __instance, MethodBase __originalMethod, 
        object __0, object __1, object __2, object __3, object __4)
    {
        PrintContext(__instance, __originalMethod, __0, __1, __2, __3, __4);
    }

    internal static void ContextLog_6(object __instance, MethodBase __originalMethod, 
        object __0, object __1, object __2, object __3, object __4, object __5)
    {
        PrintContext(__instance, __originalMethod, __0, __1, __2, __3, __4, __5);
    }

    internal static void ContextLog_7(object __instance, MethodBase __originalMethod, 
        object __0, object __1, object __2, object __3, object __4, object __5, object __6)
    {
        PrintContext(__instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6);
    }
}