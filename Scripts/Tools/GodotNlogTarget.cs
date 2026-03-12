using Godot;
using NLog;
using NLog.Common;
using NLog.Targets;

namespace TheBlockWar.Scripts.Tools;
[Target("Godot")]
public class GodotNlogTarget : TargetWithLayout
{
    public GodotNlogTarget()
    {
        Layout = "${message}";
    }

    protected override void Write(LogEventInfo logEvent)
    {
        string logMessage = RenderLogEvent(Layout, logEvent);
        string coloredMessage;
        if (logEvent.Level == LogLevel.Trace)
            coloredMessage = $"[color=gray]{logMessage}[/color]";
        else if (logEvent.Level == LogLevel.Debug)
            coloredMessage = $"[color=green]{logMessage}[/color]";
        else if (logEvent.Level == LogLevel.Info)
            coloredMessage = $"[color=white]{logMessage}[/color]";
        else if (logEvent.Level == LogLevel.Warn)
            coloredMessage = $"[color=yellow]{logMessage}[/color]";
        else if (logEvent.Level == LogLevel.Error)
            coloredMessage = $"[color=red]{logMessage}[/color]";
        else if (logEvent.Level == LogLevel.Fatal)
            coloredMessage = $"[color=red][b]{logMessage}[/b][/color]";
        else
            coloredMessage = logMessage;

        // 根据级别使用不同的 Godot 输出方法
        if (logEvent.Level == LogLevel.Trace || logEvent.Level == LogLevel.Debug || logEvent.Level == LogLevel.Info)
            GD.Print(logMessage);
        else if (logEvent.Level == LogLevel.Warn)
            GD.PushWarning(logMessage);
        else if (logEvent.Level == LogLevel.Error || logEvent.Level == LogLevel.Fatal)
            GD.PushError(logMessage);
        else
            GD.Print(logMessage);
    }

    protected override void Write(AsyncLogEventInfo logEvent)
    {
        Write(logEvent.LogEvent);
        logEvent.Continuation(null);
    }
}