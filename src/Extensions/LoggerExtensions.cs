using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace YA.TenantWorker.Extensions
{
    public static class LoggerExtensions
    {
        ///<summary>Обрамляет контекст логирования дополнительными параметрами.</summary>
        /// <param name="logger">Логер.</param>
        ///<param name="paramsAndValues">Параметры и их значения, которые нужно добавить в контекст.</param>
        public static IDisposable BeginScopeWith(this ILogger logger, params (string key, object value)[] paramsAndValues)
        {
            return logger.BeginScope(paramsAndValues.ToDictionary(x => x.key, x => x.value));
        }

        ///<summary>Логирует исключение, добвляя в контекст дополнительные параметры.</summary>
        /// <param name="logger">Логер.</param>
        /// <param name="ex">Исключение.</param>
        ///<param name="stateKeys">Параметры и их значения, которые нужно добавить в контекст.</param>
        public static bool LogException(this ILogger logger, Exception ex, params (string key, object value)[] stateKeys)
        {
            logger.Log(LogLevel.Error, 0, stateKeys.ToDictionary(x => x.key, x => x.value), ex.Demystify(), (s, e) => "Unhandled exception occured.");
            return true;
        }
    }
}
