#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Utils
{
    internal class Configuration
    {

        #region Private Singletons

        public static readonly string ConfigurationPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Assembly.GetExecutingAssembly().GetName().Name,
            Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            "Configuration.json");

        #endregion

        #region Public Properties

        public string TargetDirectory { get; set; }

        #endregion Public Properties

        private Configuration()
        {

        }

        public Configuration(Configuration configuration)
        {
            TargetDirectory = configuration.TargetDirectory ?? throw new ArgumentNullException(nameof(TargetDirectory));
        }

        /// <summary>
        /// Reads the configuration from a well formed JSON file located in %AppData%\[AssemblyName]\[Version]\Configuration.json
        /// or throws one of many Exceptions.
        /// </summary>
        /// <returns>Deserialized configuration</returns>
        /// <exception cref="FileNotFoundException">File does not exist on the filesystem</exception>
        /// <exception cref="JsonException">Unable to parse the JSON File into the expected format</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="System.UnauthorizedAccessException"> Configuration.json is read-only.
        /// -or-
        /// This operation is not supported on the current platform.
        /// -or-
        /// The caller does not have the required permission.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        public static Configuration FromFile()
        {
            if (!File.Exists(ConfigurationPath))
            {
                throw new FileNotFoundException("Specified configuration file does not exist, please make sure to create the configuration file");
            }

            var errors = new List<string>();
            var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigurationPath));


            return configuration;

        }


        public void Write()
        {
            using var fs = File.OpenWrite(ConfigurationPath);
            fs.Write(JsonConvert.SerializeObject(this));
        }
    }
}
