#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.UI.Screens;
using Siemens.Engineering.HmiUnified.HmiTags;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Services
{
    /// <summary>
    /// Handles export and import of WinCCML (WinCC Unified Markup Language) files for
    /// individual screens or HMI tag tables of WinCC Unified HMI devices in a TIA Portal
    /// project, using the Openness <see cref="HmiSoftware"/> service.
    /// </summary>
    public class WinccmlService : IWinccmlService
    {
        private readonly ILoggingService? _logger;

        /// <summary>
        /// Initializes a service for transferring WinCCML files between TIA Portal
        /// WinCC Unified HMI devices and the file system.
        /// </summary>
        /// <param name="logger">Receives progress, warning, and failure messages produced by operations.</param>
        public WinccmlService(ILoggingService? logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Exports the given screen of the WinCC Unified HMI device to the destination folder.
        /// </summary>
        public void ExportScreen(Device device, string screenName, string destinationFolder)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrWhiteSpace(screenName))
            {
                throw new ArgumentException("A screen name is required.", nameof(screenName));
            }

            if (string.IsNullOrWhiteSpace(destinationFolder))
            {
                throw new ArgumentException("An export destination folder is required.", nameof(destinationFolder));
            }

            HmiSoftware software = GetHmiSoftware(device);
            HmiScreen screen = software.Screens.FirstOrDefault(s => s.Name == screenName)
                ?? throw new InvalidOperationException($"Screen '{screenName}' was not found on device '{device.Name}'.");

            string destinationPath = Path.GetFullPath(destinationFolder);
            Directory.CreateDirectory(destinationPath);

            try
            {
                _logger?.LogInfo($"Exporting screen '{screenName}' of HMI device '{device.Name}' to '{destinationPath}'.");
                screen.Export(new DirectoryInfo(destinationPath), screenName);
                _logger?.LogInfo($"Screen export completed for '{screenName}'.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Screen export failed: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Imports a WinCCML file into the screens of the given WinCC Unified HMI device.
        /// </summary>
        public void ImportScreen(Device device, string sourceFile)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            string sourcePath = ValidateSourceFile(sourceFile);
            HmiSoftware software = GetHmiSoftware(device);

            try
            {
                _logger?.LogInfo($"Importing screen WinCCML '{sourcePath}' into HMI device '{device.Name}'.");
                var sourceFileInfo = new FileInfo(sourcePath);
                software.Screens.Import(sourceFileInfo.Directory, sourceFileInfo.Name);
                _logger?.LogInfo($"Screen import completed for HMI device '{device.Name}'.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Screen import failed: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Exports the given HMI tag table of the WinCC Unified HMI device to the destination folder.
        /// </summary>
        public void ExportTagTable(Device device, string tagTableName, string destinationFolder)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrWhiteSpace(tagTableName))
            {
                throw new ArgumentException("A tag table name is required.", nameof(tagTableName));
            }

            if (string.IsNullOrWhiteSpace(destinationFolder))
            {
                throw new ArgumentException("An export destination folder is required.", nameof(destinationFolder));
            }

            HmiSoftware software = GetHmiSoftware(device);
            HmiTagTable tagTable = software.TagTables.Find(tagTableName)
                ?? throw new InvalidOperationException($"Tag table '{tagTableName}' was not found on device '{device.Name}'.");

            string destinationPath = Path.GetFullPath(destinationFolder);
            Directory.CreateDirectory(destinationPath);

            try
            {
                _logger?.LogInfo($"Exporting HMI tag table '{tagTableName}' of device '{device.Name}' to '{destinationPath}'.");
                tagTable.Tags.Export(new DirectoryInfo(destinationPath), tagTableName);
                _logger?.LogInfo($"HMI tag table export completed for '{tagTableName}'.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"HMI tag table export failed: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Imports a WinCCML file into the given HMI tag table of the WinCC Unified HMI device.
        /// </summary>
        public void ImportTagTable(Device device, string tagTableName, string sourceFile)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrWhiteSpace(tagTableName))
            {
                throw new ArgumentException("A tag table name is required.", nameof(tagTableName));
            }

            string sourcePath = ValidateSourceFile(sourceFile);
            HmiSoftware software = GetHmiSoftware(device);
            HmiTagTable tagTable = software.TagTables.Find(tagTableName)
                ?? throw new InvalidOperationException($"Tag table '{tagTableName}' was not found on device '{device.Name}'.");

            try
            {
                _logger?.LogInfo($"Importing HMI tag table WinCCML '{sourcePath}' into tag table '{tagTableName}' of HMI device '{device.Name}'.");
                var sourceFileInfo = new FileInfo(sourcePath);
                tagTable.Tags.Import(sourceFileInfo.Directory, sourceFileInfo.Name);
                _logger?.LogInfo($"HMI tag table import completed for '{tagTableName}'.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"HMI tag table import failed: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Locates the first WinCC Unified HMI device contained in the project.
        /// </summary>
        public Device? FindFirstWinCCUnifiedDevice(ProjectBase project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            return project.Devices.FirstOrDefault(d => TryGetHmiSoftware(d, out _));
        }

        /// <summary>
        /// Lists the names of the screens contained in the WinCC Unified HMI device.
        /// </summary>
        public IEnumerable<string> GetScreenNames(Device device)
        {
            HmiSoftware software = GetHmiSoftware(device);
            return software.Screens.Select(s => s.Name).ToList();
        }

        /// <summary>
        /// Lists the names of the HMI tag tables contained in the WinCC Unified HMI device.
        /// </summary>
        public IEnumerable<string> GetTagTableNames(Device device)
        {
            HmiSoftware software = GetHmiSoftware(device);
            return software.TagTables.Select(t => t.Name).ToList();
        }

        /// <summary>
        /// Resolves the <see cref="HmiSoftware"/> exposed by a WinCC Unified HMI device.
        /// </summary>
        /// <exception cref="InvalidOperationException">No device item in the device exposes WinCC Unified HMI software.</exception>
        private static HmiSoftware GetHmiSoftware(Device device)
        {
            if (TryGetHmiSoftware(device, out HmiSoftware? software) && software != null)
            {
                return software;
            }

            throw new InvalidOperationException(
                $"Device '{device.Name}' does not contain WinCC Unified HMI software.");
        }

        /// <summary>
        /// Attempts to find an <see cref="HmiSoftware"/> instance on any device item of the given device.
        /// </summary>
        private static bool TryGetHmiSoftware(Device device, out HmiSoftware? software)
        {
            foreach (DeviceItem item in device.DeviceItems)
            {
                SoftwareContainer? container = item.GetService<SoftwareContainer>();
                if (container?.Software is HmiSoftware hmiSoftware)
                {
                    software = hmiSoftware;
                    return true;
                }
            }

            software = null;
            return false;
        }

        private static string ValidateSourceFile(string sourceFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new ArgumentException("An import source file is required.", nameof(sourceFile));
            }

            string sourcePath = Path.GetFullPath(sourceFile);
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"The WinCCML import file does not exist: '{sourcePath}'.", sourcePath);
            }

            return sourcePath;
        }
    }
}