namespace ApacheZooKeeper.Logging;

internal static class Extensions
{
    public static void LogTrace(this ILogger? logger, string? message) => Write(logger, LogLevel.Trace, ex: null, message: message);
    public static void LogTrace<T1>(this ILogger? logger, string? message, T1 p1) => Write(logger, LogLevel.Trace, ex: null, message: message, p1);
    public static void LogTrace<T1, T2>(this ILogger? logger, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2);
    public static void LogTrace<T1, T2, T3>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3);
    public static void LogTrace<T1, T2, T3, T4>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4);
    public static void LogTrace<T1, T2, T3, T4, T5>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void LogTrace<T1, T2, T3, T4, T5, T6>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogTrace<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogTrace<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Trace, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);
    public static void LogTrace(this ILogger? logger, Exception? ex) => Write(logger, LogLevel.Trace, ex: ex, message: null);
    public static void LogTrace(this ILogger? logger, Exception? ex, string? message) => Write(logger, LogLevel.Trace, ex: ex, message: message);
    public static void LogTrace<T1>(this ILogger? logger, Exception? ex, string? message, T1 p1) => Write(logger, LogLevel.Trace, ex: ex, message: message, p1);
    public static void LogTrace<T1, T2>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Trace, ex: ex, message: message, p1, p2);
    public static void LogTrace<T1, T2, T3>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Trace, ex: ex, message: message, p1, p2, p3);
    public static void LogTrace<T1, T2, T3, T4>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Trace, ex: ex, message: message, p1, p2, p3, p4);
    public static void LogTrace<T1, T2, T3, T4, T5>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Trace, ex: ex, message: message, p1, p2, p3, p4, p5);
    public static void LogTrace<T1, T2, T3, T4, T5, T6>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Trace, ex: ex, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogTrace<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Trace, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogTrace<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Trace, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void LogDebug(this ILogger? logger, string? message) => Write(logger, LogLevel.Debug, ex: null, message: message);
    public static void LogDebug<T1>(this ILogger? logger, string? message, T1 p1) => Write(logger, LogLevel.Debug, ex: null, message: message, p1);
    public static void LogDebug<T1, T2>(this ILogger? logger, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2);
    public static void LogDebug<T1, T2, T3>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3);
    public static void LogDebug<T1, T2, T3, T4>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4);
    public static void LogDebug<T1, T2, T3, T4, T5>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void LogDebug<T1, T2, T3, T4, T5, T6>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogDebug<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogDebug<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Debug, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);
    public static void LogDebug(this ILogger? logger, Exception? ex) => Write(logger, LogLevel.Debug, ex: ex, message: null);
    public static void LogDebug(this ILogger? logger, Exception? ex, string? message) => Write(logger, LogLevel.Debug, ex: ex, message: message);
    public static void LogDebug<T1>(this ILogger? logger, Exception? ex, string? message, T1 p1) => Write(logger, LogLevel.Debug, ex: ex, message: message, p1);
    public static void LogDebug<T1, T2>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Debug, ex: ex, message: message, p1, p2);
    public static void LogDebug<T1, T2, T3>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Debug, ex: ex, message: message, p1, p2, p3);
    public static void LogDebug<T1, T2, T3, T4>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Debug, ex: ex, message: message, p1, p2, p3, p4);
    public static void LogDebug<T1, T2, T3, T4, T5>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Debug, ex: ex, message: message, p1, p2, p3, p4, p5);
    public static void LogDebug<T1, T2, T3, T4, T5, T6>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Debug, ex: ex, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogDebug<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Debug, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogDebug<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Debug, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void LogInformation(this ILogger? logger, string? message) => Write(logger, LogLevel.Information, ex: null, message: message);
    public static void LogInformation<T1>(this ILogger? logger, string? message, T1 p1) => Write(logger, LogLevel.Information, ex: null, message: message, p1);
    public static void LogInformation<T1, T2>(this ILogger? logger, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2);
    public static void LogInformation<T1, T2, T3>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3);
    public static void LogInformation<T1, T2, T3, T4>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4);
    public static void LogInformation<T1, T2, T3, T4, T5>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void LogInformation<T1, T2, T3, T4, T5, T6>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogInformation<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogInformation<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Information, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);
    public static void LogInformation(this ILogger? logger, Exception? ex) => Write(logger, LogLevel.Information, ex: ex, message: null);
    public static void LogInformation(this ILogger? logger, Exception? ex, string? message) => Write(logger, LogLevel.Information, ex: ex, message: message);
    public static void LogInformation<T1>(this ILogger? logger, Exception? ex, string? message, T1 p1) => Write(logger, LogLevel.Information, ex: ex, message: message, p1);
    public static void LogInformation<T1, T2>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Information, ex: ex, message: message, p1, p2);
    public static void LogInformation<T1, T2, T3>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Information, ex: ex, message: message, p1, p2, p3);
    public static void LogInformation<T1, T2, T3, T4>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Information, ex: ex, message: message, p1, p2, p3, p4);
    public static void LogInformation<T1, T2, T3, T4, T5>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Information, ex: ex, message: message, p1, p2, p3, p4, p5);
    public static void LogInformation<T1, T2, T3, T4, T5, T6>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Information, ex: ex, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogInformation<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Information, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogInformation<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Information, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void LogWarning(this ILogger? logger, string? message) => Write(logger, LogLevel.Warning, ex: null, message: message);
    public static void LogWarning<T1>(this ILogger? logger, string? message, T1 p1) => Write(logger, LogLevel.Warning, ex: null, message: message, p1);
    public static void LogWarning<T1, T2>(this ILogger? logger, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2);
    public static void LogWarning<T1, T2, T3>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3);
    public static void LogWarning<T1, T2, T3, T4>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4);
    public static void LogWarning<T1, T2, T3, T4, T5>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void LogWarning<T1, T2, T3, T4, T5, T6>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogWarning<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogWarning<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Warning, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);
    public static void LogWarning(this ILogger? logger, Exception? ex) => Write(logger, LogLevel.Warning, ex: ex, message: null);
    public static void LogWarning(this ILogger? logger, Exception? ex, string? message) => Write(logger, LogLevel.Warning, ex: ex, message: message);
    public static void LogWarning<T1>(this ILogger? logger, Exception? ex, string? message, T1 p1) => Write(logger, LogLevel.Warning, ex: ex, message: message, p1);
    public static void LogWarning<T1, T2>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Warning, ex: ex, message: message, p1, p2);
    public static void LogWarning<T1, T2, T3>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Warning, ex: ex, message: message, p1, p2, p3);
    public static void LogWarning<T1, T2, T3, T4>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Warning, ex: ex, message: message, p1, p2, p3, p4);
    public static void LogWarning<T1, T2, T3, T4, T5>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Warning, ex: ex, message: message, p1, p2, p3, p4, p5);
    public static void LogWarning<T1, T2, T3, T4, T5, T6>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Warning, ex: ex, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogWarning<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Warning, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogWarning<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Warning, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void LogError(this ILogger? logger, string? message) => Write(logger, LogLevel.Error, ex: null, message: message);
    public static void LogError<T1>(this ILogger? logger, string? message, T1 p1) => Write(logger, LogLevel.Error, ex: null, message: message, p1);
    public static void LogError<T1, T2>(this ILogger? logger, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2);
    public static void LogError<T1, T2, T3>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3);
    public static void LogError<T1, T2, T3, T4>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4);
    public static void LogError<T1, T2, T3, T4, T5>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void LogError<T1, T2, T3, T4, T5, T6>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogError<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogError<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Error, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);
    public static void LogError(this ILogger? logger, Exception? ex) => Write(logger, LogLevel.Error, ex: ex, message: null);
    public static void LogError(this ILogger? logger, Exception? ex, string? message) => Write(logger, LogLevel.Error, ex: ex, message: message);
    public static void LogError<T1>(this ILogger? logger, Exception? ex, string? message, T1 p1) => Write(logger, LogLevel.Error, ex: ex, message: message, p1);
    public static void LogError<T1, T2>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Error, ex: ex, message: message, p1, p2);
    public static void LogError<T1, T2, T3>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Error, ex: ex, message: message, p1, p2, p3);
    public static void LogError<T1, T2, T3, T4>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Error, ex: ex, message: message, p1, p2, p3, p4);
    public static void LogError<T1, T2, T3, T4, T5>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Error, ex: ex, message: message, p1, p2, p3, p4, p5);
    public static void LogError<T1, T2, T3, T4, T5, T6>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Error, ex: ex, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogError<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Error, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogError<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Error, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    public static void LogCritical(this ILogger? logger, string? message) => Write(logger, LogLevel.Critical, ex: null, message: message);
    public static void LogCritical<T1>(this ILogger? logger, string? message, T1 p1) => Write(logger, LogLevel.Critical, ex: null, message: message, p1);
    public static void LogCritical<T1, T2>(this ILogger? logger, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2);
    public static void LogCritical<T1, T2, T3>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3);
    public static void LogCritical<T1, T2, T3, T4>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4);
    public static void LogCritical<T1, T2, T3, T4, T5>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4, p5);
    public static void LogCritical<T1, T2, T3, T4, T5, T6>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogCritical<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogCritical<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Critical, ex: null, message: message, p1, p2, p3, p4, p5, p6, p7, p8);
    public static void LogCritical(this ILogger? logger, Exception? ex) => Write(logger, LogLevel.Critical, ex: ex, message: null);
    public static void LogCritical(this ILogger? logger, Exception? ex, string? message) => Write(logger, LogLevel.Critical, ex: ex, message: message);
    public static void LogCritical<T1>(this ILogger? logger, Exception? ex, string? message, T1 p1) => Write(logger, LogLevel.Critical, ex: ex, message: message, p1);
    public static void LogCritical<T1, T2>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2) => Write(logger, LogLevel.Critical, ex: ex, message: message, p1, p2);
    public static void LogCritical<T1, T2, T3>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3) => Write(logger, LogLevel.Critical, ex: ex, message: message, p1, p2, p3);
    public static void LogCritical<T1, T2, T3, T4>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4) => Write(logger, LogLevel.Critical, ex: ex, message: message, p1, p2, p3, p4);
    public static void LogCritical<T1, T2, T3, T4, T5>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => Write(logger, LogLevel.Critical, ex: ex, message: message, p1, p2, p3, p4, p5);
    public static void LogCritical<T1, T2, T3, T4, T5, T6>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => Write(logger, LogLevel.Critical, ex: ex, message: message, p1, p2, p3, p4, p5, p6);
    public static void LogCritical<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => Write(logger, LogLevel.Critical, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7);
    public static void LogCritical<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => Write(logger, LogLevel.Critical, ex: ex, message: message, p1, p2, p3, p4, p5, p6, p7, p8);

    private static void Write(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message);
        }
    }

    private static void Write<T1>(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message, T1 p1)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1);
        }
    }

    private static void Write<T1, T2>(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message, T1 p1, T2 p2)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2);
        }
    }

    private static void Write<T1, T2, T3>(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message, T1 p1, T2 p2, T3 p3)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3);
        }
    }

    private static void Write<T1, T2, T3, T4>(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4);
        }
    }

    private static void Write<T1, T2, T3, T4, T5>(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4, p5);
        }
    }

    private static void Write<T1, T2, T3, T4, T5, T6>(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4, p5, p6);
        }
    }

    private static void Write<T1, T2, T3, T4, T5, T6, T7>(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4, p5, p6, p7);
        }
    }

    private static void Write<T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger? logger, LogLevel logLevel, Exception? ex, string? message, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
    {
        if (logger != null && logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, exception: ex, message: message, p1, p2, p3, p4, p5, p6, p7, p8);
        }
    }
}
