using System;
using System.Runtime.CompilerServices;
using VaettirNet.Btleplug;

namespace VaettirNet.PixelsDice.Net;

public class Logger
{
    public static Logger Instance { get; } = new();

    private static PixelsLogLevel _logLevel =
#if DEBUG
        PixelsLogLevel.Error;
#else
        PixelsLogLevel.None;
#endif

    public void Log(PixelsLogLevel level,
        FormattableString message,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = null)
    {
        if (!ShouldLog(level)) return;

        // ReSharper disable once ExplicitCallerInfoArgument
        Log(level, message.ToString(), file, line, member);
    }

    public void Log(PixelsLogLevel level,
        string message,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = null)
    {
        if (!ShouldLog(level)) return;

        Console.WriteLine($"[{level}] {file}:{line} in {member}: {message}");
    }

    public bool ShouldLog(PixelsLogLevel level)
    {
        return level <= _logLevel;
    }

    public static void SetLogLevel(PixelsLogLevel logLevel)
    {
        _logLevel = logLevel;
        BtleManager.SetLogLevel((BtleLogLevel)logLevel);
    }
}