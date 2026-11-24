using System;
using System.IO;
using System.Reflection;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Utils
{
    /// <summary>
    /// Manages opening of embedded documentation resources (PDF manuals, OSS info).
    /// Matches CopyPLC ManualManager pattern: direct call to sandbox-safe Process.Start,
    /// per-step logging, silent catch in OpenFile.
    /// </summary>
    public static class ManualOssManager
    {
        /// <summary>
        /// Open the PDF manual file.
        /// </summary>
        public static void OpenManual(ILoggingService? logger = null)
        {
            logger?.LogInfo("Open the PDF manual of the application");
            OpenFile("Manual_en.pdf", "manual", logger);
        }

        /// <summary>
        /// Open the OSS info file.
        /// </summary>
        public static void OpenOss(ILoggingService? logger = null)
        {
            logger?.LogInfo("Open the OSS information of the application");
            OpenFile("Multilingual_ReadMe_OSS.html", "OSS-file", logger);
        }

        /// <summary>
        /// Extracts an embedded resource to the temp folder and opens it.
        /// Uses <see cref="Siemens.Engineering.AddIn.Utilities.Process.Start"/> directly
        /// (sandbox-safe in AddIn context, works like System.Diagnostics.Process.Start outside).
        /// </summary>
        private static void OpenFile(string fileName, string fileDescription, ILoggingService? logger)
        {
            try
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                using (Stream resourceStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Resources." + fileName))
                {
                    if (resourceStream == null)
                    {
                        logger?.LogError($"Could not receive the {fileDescription} ({fileName}) from the assembly.");
                        return;
                    }

                    using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write)) resourceStream.CopyTo(fileStream);
                }

                Siemens.Engineering.AddIn.Utilities.Process.Start(tempFilePath);
            }
            catch (Exception ex)
            {
                logger?.LogError($"Error opening {fileDescription}: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
