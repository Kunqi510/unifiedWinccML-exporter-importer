using System.Collections.Generic;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces
{
    /// <summary>
    /// Provides centralized logging services for the application.
    /// All components receive an <see cref="ILoggingService"/> via constructor injection
    /// instead of creating static NLog loggers directly.
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Logs a general informational message.
        /// </summary>
        void LogInfo(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Logs a general error message.
        /// </summary>
        void LogError(string message);

        /// <summary>
        /// Returns all recorded log messages (info, warning, and error combined).
        /// </summary>
        IReadOnlyList<string> GetLogMessages();

        /// <summary>
        /// Clears all recorded log messages.
        /// </summary>
        void ClearLogs();
    }
}
