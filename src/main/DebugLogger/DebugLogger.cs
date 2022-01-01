namespace Logging;

using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using HarmonyLib;
using System.Reflection;

public class DebugLogger : ILogger
{
    private static readonly Harmony harmony = new Harmony(nameof(DebugLogger));
    private static readonly int MAX_PARAMETERS_SUPPORTED = 7;
    private static readonly string PREFIX_PATCHER = "ContextLog_";
    private static readonly MethodInfo[] prefixes = new MethodInfo[MAX_PARAMETERS_SUPPORTED + 1];

    public DebugLoggerConfig Configs { get; }

    static DebugLogger()
    {
        for (int i = 0; i <= MAX_PARAMETERS_SUPPORTED; i++)
        {
            var prefixMethodName = PREFIX_PATCHER + i;
            var prefixMethod = typeof(MethodPatcher).GetMethod(prefixMethodName, 
                        BindingFlags.NonPublic | BindingFlags.Static);
            if (prefixMethod == null)
                throw new NullReferenceException($"{nameof(MethodPatcher)}.{prefixMethodName} has not been found");

            prefixes[i] = prefixMethod;
        }
    }

    public DebugLogger(DebugLoggerConfig configs)
    {
        this.Configs = configs;
        PatchMethodsForContextLogging(configs.MethodsWithContextLogging);
    }

    private void PatchMethodsForContextLogging(ISet<MethodBase> methodsWithContextLogging)
    {
        if (!this.Configs.IsEnabled)
            return;
            
        foreach (var m in methodsWithContextLogging
            .Where(m => !harmony.GetPatchedMethods().Contains(m)))
        {
            var parametersCount = m.GetParameters().Length;
            var prefixMethod = prefixes[parametersCount];
            harmony.Patch(original: m, prefix: new HarmonyMethod(prefixMethod));
        }
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) => Configs.IsEnabled;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!this.Configs.IsEnabled)
            return;
        Configs.OutWriter.Write($"[level: {logLevel}][eId: {eventId}] - ");
        Configs.OutWriter.WriteLine(formatter(state, exception));
    }

    public void LogVariables(string separator, params (string vName, object vValue)[] variables)
    {
        if (variables.Length == 0 || !this.Configs.IsEnabled)
            return;

        var sb = new StringBuilder();
        foreach (var v in variables)
        {
            sb.Append(Configs.VariableFormatter.Invoke(v.vName, v.vValue))
                .Append(separator);
        }
        sb.Remove(sb.Length - separator.Length, separator.Length);

        Configs.OutWriter.WriteLine(sb.ToString());
    }

    public void LogVariables(params (string vName, object vValue)[] variables)
    {
        this.LogVariables(", ", variables);
    }

    public void Log(string message)
    {
        if (!this.Configs.IsEnabled)
            return;
        Configs.OutWriter.WriteLine(message);
    }

    public void Log(LogLevel level, EventId eventId, string message)
    {
        Log<string>(level, eventId, message, null, (message, exc) => message);
    }

    public class DebugLoggerConfig 
    {
        public bool IsEnabled { get; init; } = true;
        public TextWriter OutWriter { get; init; } = Console.Out;
        public Func<string, object, string> VariableFormatter { get; init; } = (name, value) => $"{name} = {value}";
        public ISet<MethodBase> MethodsWithContextLogging { get; init; } = new HashSet<MethodBase>();
    }
}