namespace ApacheZooKeeper.Logging;

internal static class Extensions
{
    public static bool isTraceEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Trace);
    public static bool isDebugEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Debug);
    public static bool isInfoEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Information);
    public static bool isWarnEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Warning);
    public static bool isErrorEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Error);

    public static void trace(this ILogger logger, string message) => Write(logger, LogLevel.Trace, ex: null, message: message);
    public static void trace(this ILogger logger, Exception ex) => Write(logger, LogLevel.Trace, ex: ex, message: null);

    public static void trace<T1>(this ILogger logger, string message, T1 p1)
    {
        if (p1 is Exception ex)
        {
            Write(logger, LogLevel.Trace, ex: ex, message: message);
        }
        else
        {
            Write(logger, LogLevel.Trace, ex: null, message: message, p1);
        }
    }

    public static void trace<T1, T2>(this ILogger logger, string message, T1 p1, T2 p2) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2);
    public static void trace<T1, T2, T3>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3);
    public static void trace<T1, T2, T3, T4>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4);
    public static void trace<T1, T2, T3, T4, T5>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void trace<T1, T2, T3, T4, T5, T6>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void trace<T1, T2, T3, T4, T5, T6, T7>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void trace<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void debug(this ILogger logger, string message) => Write(logger, LogLevel.Debug, ex: null, message: message);
    public static void debug(this ILogger logger, Exception ex) => Write(logger, LogLevel.Debug, ex: ex, message: null);

    public static void debug<T1>(this ILogger logger, string message, T1 p1)
    {
        if (p1 is Exception ex)
        {
            Write(logger, LogLevel.Debug, ex: ex, message: message);
        }
        else
        {
            Write(logger, LogLevel.Debug, ex: null, message: message, p1);
        }
    }

    public static void debug<T1, T2>(this ILogger logger, string message, T1 p1, T2 p2) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2);
    public static void debug<T1, T2, T3>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3);
    public static void debug<T1, T2, T3, T4>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4);
    public static void debug<T1, T2, T3, T4, T5>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void debug<T1, T2, T3, T4, T5, T6>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void debug<T1, T2, T3, T4, T5, T6, T7>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void debug<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void info(this ILogger logger, string message) => Write(logger, LogLevel.Information, ex: null, message: message);
    public static void info(this ILogger logger, Exception ex) => Write(logger, LogLevel.Information, ex: ex, message: null);

    public static void info<T1>(this ILogger logger, string message, T1 p1)
    {
        if (p1 is Exception ex)
        {
            Write(logger, LogLevel.Information, ex: ex, message: message);
        }
        else
        {
            Write(logger, LogLevel.Information, ex: null, message: message, p1);
        }
    }

    public static void info<T1, T2>(this ILogger logger, string message, T1 p1, T2 p2) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2);
    public static void info<T1, T2, T3>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3);
    public static void info<T1, T2, T3, T4>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4);
    public static void info<T1, T2, T3, T4, T5>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void info<T1, T2, T3, T4, T5, T6>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void info<T1, T2, T3, T4, T5, T6, T7>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void info<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void warn(this ILogger logger, string message) => Write(logger, LogLevel.Warning, ex: null, message: message);
    public static void warn(this ILogger logger, Exception ex) => Write(logger, LogLevel.Warning, ex: ex, message: null);

    public static void warn<T1>(this ILogger logger, string message, T1 p1)
    {
        if (p1 is Exception ex)
        {
            Write(logger, LogLevel.Warning, ex: ex, message: message);
        }
        else
        {
            Write(logger, LogLevel.Warning, ex: null, message: message, p1);
        }
    }

    public static void warn<T1, T2>(this ILogger logger, string message, T1 p1, T2 p2) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2);
    public static void warn<T1, T2, T3>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3);
    public static void warn<T1, T2, T3, T4>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4);
    public static void warn<T1, T2, T3, T4, T5>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void warn<T1, T2, T3, T4, T5, T6>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void warn<T1, T2, T3, T4, T5, T6, T7>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void warn<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void error(this ILogger logger, string message) => Write(logger, LogLevel.Error, ex: null, message: message);
    public static void error(this ILogger logger, Exception ex) => Write(logger, LogLevel.Error, ex: ex, message: null);

    public static void error<T1>(this ILogger logger, string message, T1 p1)
    {
        if (p1 is Exception ex)
        {
            Write(logger, LogLevel.Error, ex: ex, message: message);
        }
        else
        {
            Write(logger, LogLevel.Error, ex: null, message: message, p1);
        }
    }

    public static void error<T1, T2>(this ILogger logger, string message, T1 p1, T2 p2) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2);
    public static void error<T1, T2, T3>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3);
    public static void error<T1, T2, T3, T4>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4);
    public static void error<T1, T2, T3, T4, T5>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void error<T1, T2, T3, T4, T5, T6>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void error<T1, T2, T3, T4, T5, T6, T7>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void error<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void critical(this ILogger logger, string message) => Write(logger, LogLevel.Critical, ex: null, message: message);
    public static void critical(this ILogger logger, Exception ex) => Write(logger, LogLevel.Critical, ex: ex, message: null);

    public static void critical<T1>(this ILogger logger, string message, T1 p1)
    {
        if (p1 is Exception ex)
        {
            Write(logger, LogLevel.Critical, ex: ex, message: message);
        }
        else
        {
            Write(logger, LogLevel.Critical, ex: null, message: message, p1);
        }
    }

    public static void critical<T1, T2>(this ILogger logger, string message, T1 p1, T2 p2) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2);
    public static void critical<T1, T2, T3>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3);
    public static void critical<T1, T2, T3, T4>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4);
    public static void critical<T1, T2, T3, T4, T5>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void critical<T1, T2, T3, T4, T5, T6>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void critical<T1, T2, T3, T4, T5, T6, T7>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void critical<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger logger, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    private static void Write(this ILogger logger, LogLevel logLevel, Exception ex, string message)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message);
        }
    }

    private static void Write<T1>(this ILogger logger, LogLevel logLevel, Exception ex, string message, T1 p1)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1);
        }
    }

    private static void Write<T1, T2>(this ILogger logger, LogLevel logLevel, Exception ex, string message, T1 p1, T2 p2)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2);
        }
    }

    private static void Write<T1, T2, T3>(this ILogger logger, LogLevel logLevel, Exception ex, string message, T1 p1, T2 p2, T3 p3)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3);
        }
    }

    private static void Write<T1, T2, T3, T4>(this ILogger logger, LogLevel logLevel, Exception ex, string message, T1 p1, T2 p2, T3 p3, T4 p4)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4);
        }
    }

    private static void Write<T1, T2, T3, T4, T5>(this ILogger logger, LogLevel logLevel, Exception ex, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4, p5);
        }
    }

    private static void Write<T1, T2, T3, T4, T5, T6>(this ILogger logger, LogLevel logLevel, Exception ex, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4, p5, p6);
        }
    }

    private static void Write<T1, T2, T3, T4, T5, T6, T7>(this ILogger logger, LogLevel logLevel, Exception ex, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4, p5, p6, p7);
        }
    }

    private static void Write<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger logger, LogLevel logLevel, Exception ex, string message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4, p5, p6, p7, p8);
        }
    }
}
