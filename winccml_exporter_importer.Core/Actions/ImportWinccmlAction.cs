using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Services;
using Siemens.Engineering;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Actions
{
    /// <summary>
    /// Imports a WinCCML file into a screen or HMI tag table of a WinCC Unified HMI device
    /// of a TIA Portal project.
    /// </summary>
    public class ImportWinccmlAction
    {
        private readonly ProjectBase _project;
        private readonly ILoggingService _logger;

        /// <summary>
        /// Creates the action for the given project.
        /// </summary>
        public ImportWinccmlAction(ProjectBase project, ILoggingService logger)
        {
            _project = project;
            _logger = logger;
        }

        /// <summary>
        /// Imports the WinCCML file from <paramref name="sourceFile"/> into the screens of the
        /// first WinCC Unified HMI device found in the project.
        /// </summary>
        /// <param name="sourceFile">Path to the WinCCML file to import.</param>
        public void ImportScreen(string sourceFile)
        {
            var service = new WinccmlService(_logger);
            var device = GetDevice(service);
            service.ImportScreen(device, sourceFile);
        }

        /// <summary>
        /// Imports the WinCCML file from <paramref name="sourceFile"/> into the given HMI tag
        /// table of the first WinCC Unified HMI device found in the project.
        /// </summary>
        /// <param name="tagTableName">Name of the HMI tag table to import into.</param>
        /// <param name="sourceFile">Path to the WinCCML file to import.</param>
        public void ImportTagTable(string tagTableName, string sourceFile)
        {
            var service = new WinccmlService(_logger);
            var device = GetDevice(service);
            service.ImportTagTable(device, tagTableName, sourceFile);
        }

        private Siemens.Engineering.HW.Device GetDevice(WinccmlService service)
        {
            var device = service.FindFirstWinCCUnifiedDevice(_project);
            if (device == null)
            {
                _logger.LogError("No WinCC Unified HMI device was found in the project.");
                throw new System.InvalidOperationException("No WinCC Unified HMI device was found in the project.");
            }

            return device;
        }
    }
}