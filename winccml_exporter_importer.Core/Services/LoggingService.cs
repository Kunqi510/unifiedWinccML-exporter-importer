using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NLog;
using NLog.Config;
using NLog.Targets;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Services
{
    /// <summary>
    /// NLog-backed logging service. Instance-based — no static Logger fields.
    /// Creates a file target at <c>%AppData%\Siemens\{AppName}\{Version}\{AppName}.log</c>.
    /// Console targets are added only for standalone apps (<paramref name="isAddIn"/> = false).
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly List<string> _logMessages = new List<string>();
        private readonly Logger _logger;

        /// <summary>
        /// Full path to the directory containing the log file.
        /// </summary>
        public string LogDirectory { get; }

        /// <summary>
        /// Full path to the log file.
        /// </summary>
        public string LogFilePath { get; }

        /// <summary>
        /// Creates a new logging service instance.
        /// </summary>
        /// <param name="isAddIn">True when running inside TIA Portal sandbox (suppresses console targets).</param>
        /// <param name="minLevel">Minimum NLog level. Defaults to <see cref="LogLevel.Info"/>.</param>
        public LoggingService(bool isAddIn = false, LogLevel minLevel = null)
        {
            minLevel = minLevel ?? LogLevel.Info;

            var asmName = Assembly.GetExecutingAssembly().GetName();
            string cleanedName = asmName.Name
                .Replace(".Utilities", "")
                .Replace(".Core", "");

            LogDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Siemens",
                cleanedName,
                asmName.Version.ToString());

            string logFileName = cleanedName + ".log";
            LogFilePath = Path.Combine(LogDirectory, logFileName);

            var logFactory = BuildLogFactory(LogDirectory, logFileName, isAddIn, minLevel);
            _logger = logFactory.GetLogger(cleanedName);
        }

        /// <inheritdoc />
        public void LogInfo(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _logMessages.Add($"[INFO] {message}");
            _logger.Info(message);
        }

        /// <inheritdoc />
        public void LogWarning(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _logMessages.Add($"[WARN] {message}");
            _logger.Warn(message);
        }

        /// <inheritdoc />
        public void LogError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _logMessages.Add($"[ERROR] {message}");
            _logger.Error(message);
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetLogMessages()
        {
            return _logMessages.AsReadOnly();
        }

        /// <inheritdoc />
        public void ClearLogs()
        {
            _logMessages.Clear();
        }

        private static LogFactory BuildLogFactory(
            string logDir, string logFileName, bool isAddIn, LogLevel minLevel)
        {
            var logFactory = new LogFactory();

            try
            {
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                var config = new LoggingConfiguration();

                var fileTarget = new FileTarget("logfile")
                {
                    FileName = Path.Combine(logDir, logFileName),
                    AutoFlush = true,
                    KeepFileOpen = false,
                    Layout = "${longdate} | ${level:uppercase=true} | ${message} ${exception:format=toString}"
                };

                var fileLogLevel = minLevel < LogLevel.Info ? minLevel : LogLevel.Info;
                config.AddRule(fileLogLevel, LogLevel.Fatal, fileTarget);

                if (!isAddIn)
                {
                    var stderrTarget = new ColoredConsoleTarget("stderr")
                    {
                        Layout = "${level} | ${message} | ${exception}",
                        StdErr = true
                    };
                    var stdoutTarget = new ColoredConsoleTarget("stdout")
                    {
                        Layout = "${level} | ${message}",
                        StdErr = false
                    };

                    config.AddRule(LogLevel.Error, LogLevel.Fatal, stderrTarget);
                    config.AddRule(minLevel, LogLevel.Warn, stdoutTarget);
                }

                logFactory.Configuration = config;
            }
            catch (Exception)
            {
                // If logging setup fails (e.g., sandbox restrictions), return a no-op factory
                // so callers never crash due to logging infrastructure.
            }

            return logFactory;
        }
    }
}
