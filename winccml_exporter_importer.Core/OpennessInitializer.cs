using System;
using System.IO;
using System.Reflection;
using Siemens.Collaboration.Net;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core
{
    /// <summary>
    /// Initializes Openness assembly resolution automatically when first accessed.
    /// The static constructor registers both the NuGet-based resolver and a fallback
    /// <see cref="AppDomain.AssemblyResolve"/> handler for self-contained deployments.
    /// </summary>
    public static class OpennessInitializer
    {
        /// <summary>
        /// Path to the Openness V21.1 DLL directory.
        /// </summary>
        private static readonly string OpennessDllDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Siemens\Automation\Portal V21.1\PublicAPI\V21.1\net48");

        /// <summary>
        /// Static constructor — runs automatically the first time any member is accessed.
        /// Registers assembly resolution before any Openness types are needed.
        /// </summary>
        static OpennessInitializer()
        {
            // Register a fallback resolver that probes the app directory and PublicAPI directory.
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            // Run the NuGet-based resolver (handles Openness DLL resolution on .NET Framework 4.8)
            Api.Global.Openness().Initialize();
        }

        /// <summary>
        /// Explicit initialization entry point. Accessing this method triggers the static
        /// constructor. CLI and UI apps may call this at startup for clarity.
        /// </summary>
        public static void Initialize()
        {
            // No-op: the static constructor does all the work.
        }

        /// <summary>
        /// Resolves Siemens.Engineering assemblies by probing the application base directory
        /// (self-contained delivery) and the TIA Portal PublicAPI directory.
        /// </summary>
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            if (!assemblyName.Name.StartsWith("Siemens.Engineering", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // First: application base directory (self-contained delivery)
            string localPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                assemblyName.Name + ".dll");
            if (File.Exists(localPath))
            {
                return Assembly.LoadFrom(localPath);
            }

            // Second: TIA Portal PublicAPI directory
            string tiaPath = Path.Combine(OpennessDllDir, assemblyName.Name + ".dll");
            if (File.Exists(tiaPath))
            {
                return Assembly.LoadFrom(tiaPath);
            }

            return null;
        }
    }
}
