using Siemens.Engineering;
#nullable enable

using System.Collections.Generic;
using Siemens.Engineering;
using Siemens.Engineering.HW;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces
{
    /// <summary>
    /// Exports and imports WinCCML (WinCC Unified Markup Language) files for individual
    /// screens or HMI tag tables of a WinCC Unified HMI device in a TIA Portal project.
    /// </summary>
    public interface IWinccmlService
    {
        /// <summary>
        /// Exports the given screen of the WinCC Unified HMI device to the specified destination folder.
        /// </summary>
        /// <param name="device">A WinCC Unified HMI device contained in the project.</param>
        /// <param name="screenName">Name of the screen to export.</param>
        /// <param name="destinationFolder">Folder that receives the exported WinCCML file.</param>
        void ExportScreen(Device device, string screenName, string destinationFolder);

        /// <summary>
        /// Imports a WinCCML file into the screens of the given WinCC Unified HMI device.
        /// </summary>
        /// <param name="device">A WinCC Unified HMI device contained in the project.</param>
        /// <param name="sourceFile">Path to the WinCCML file to import.</param>
        void ImportScreen(Device device, string sourceFile);

        /// <summary>
        /// Exports the given HMI tag table of the WinCC Unified HMI device to the specified destination folder.
        /// </summary>
        /// <param name="device">A WinCC Unified HMI device contained in the project.</param>
        /// <param name="tagTableName">Name of the HMI tag table to export.</param>
        /// <param name="destinationFolder">Folder that receives the exported WinCCML file.</param>
        void ExportTagTable(Device device, string tagTableName, string destinationFolder);

        /// <summary>
        /// Imports a WinCCML file into the given HMI tag table of the WinCC Unified HMI device.
        /// </summary>
        /// <param name="device">A WinCC Unified HMI device contained in the project.</param>
        /// <param name="tagTableName">Name of the HMI tag table to import into.</param>
        /// <param name="sourceFile">Path to the WinCCML file to import.</param>
        void ImportTagTable(Device device, string tagTableName, string sourceFile);

        /// <summary>
        /// Locates the first WinCC Unified HMI device contained in the project.
        /// </summary>
        /// <param name="project">An open TIA Portal project.</param>
        /// <returns>The first WinCC Unified HMI device found, or <see langword="null"/> if none exists.</returns>
        Device? FindFirstWinCCUnifiedDevice(ProjectBase project);

        /// <summary>
        /// Lists the names of the screens contained in the WinCC Unified HMI device.
        /// </summary>
        IEnumerable<string> GetScreenNames(Device device);

        /// <summary>
        /// Lists the names of the HMI tag tables contained in the WinCC Unified HMI device.
        /// </summary>
        IEnumerable<string> GetTagTableNames(Device device);
    }
}
