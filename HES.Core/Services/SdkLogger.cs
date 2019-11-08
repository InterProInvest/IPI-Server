using System;
using Hideez.SDK.Communication.Log;
using Microsoft.Extensions.Logging;

namespace HES.Core.Services
{
    public class SdkLogger<T> : ILog
    {
        private readonly ILogger<T> _logger;

        public SdkLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Shutdown()
        {
        }

        public void WriteDebugLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Debug)
        {
            Log($"{source} # {message}", severity);
        }

        public void WriteDebugLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Debug)
        {
            Log($"{source} # {ex.Message}", severity);
        }

        public void WriteDebugLine(string source, string message, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
            Log($"{source} # {message} # {ex.Message}", severity);
        }

        public void WriteLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Information, string stackTrace = null)
        {
            Log($"{source} # {message}", severity);
        }

        public void WriteLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
            Log($"{source} # {ex.Message}", severity);
        }

        public void WriteLine(string source, string message, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
            Log($"{source} # {message} # {ex.Message}", severity);
        }

        void Log(string message, LogErrorSeverity severity)
        {
            switch (severity)
            {
                case LogErrorSeverity.Fatal:
                    _logger.LogCritical(message);
                    break;
                case LogErrorSeverity.Error:
                    _logger.LogError(message);
                    break;
                case LogErrorSeverity.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogErrorSeverity.Information:
                    _logger.LogInformation(message);
                    break;
                case LogErrorSeverity.Debug:
                    _logger.LogDebug(message);
                    break;
                default:
                    break;
            }
        }
    }
}
