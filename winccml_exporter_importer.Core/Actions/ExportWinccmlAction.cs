using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Services;
using Siemens.Engineering;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Actions
{
    /// <summary>
    /// Exports the WinCCML representation of a screen or HMI tag table of a WinCC Unified
    /// HMI device from a TIA Portal project.
    /// </summary>
    public class ExportWinccmlAction
    {
        private readonly ProjectBase _project;
        private readonly ILoggingService _logger;

        /// <summary>
        /// Creates the action for the given project.
        /// </summary>
        public ExportWinccmlAction(ProjectBase project, ILoggingService logger)
        {
            _project = project;
            _logger = logger;
        }

        /// <summary>
        /// Exports the WinCCML file for the given screen of the first WinCC Unified HMI device
        /// found in the project to the specified destination folder.
        /// </summary>
        public void ExportScreen(string screenName, string destFolder)
        {
            var service = new WinccmlService(_logger);
            var device = GetDevice(service);
            service.ExportScreen(device, screenName, destFolder);
        }

        /// <summary>
        /// Exports the WinCCML file for the given HMI tag table of the first WinCC Unified HMI
        /// device found in the project to the specified destination folder.
        /// </summary>
        public void ExportTagTable(string tagTableName, string destFolder)
        {
            var service = new WinccmlService(_logger);
            var device = GetDevice(service);
            service.ExportTagTable(device, tagTableName, destFolder);
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