#nullable enable
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Utils;
using Siemens.Engineering;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core
{
    /// <summary>
    /// Connection facade for TIA Portal projects.
    /// Handles process attachment, project opening, and assembly resolution.
    /// Feature logic lives in dedicated services under <c>Services/</c> — each service
    /// receives <see cref="Project"/> and operates independently.
    /// </summary>
    public class ProjectAdapter
    {
        private readonly ILoggingService? _logger;

        /// <summary>
        /// The underlying TIA Portal project instance.
        /// Pass this to service classes (e.g. <c>DeviceService</c>) to execute features.
        /// </summary>
        public ProjectBase Project { get; }
        /// <summary>
        /// The TIA Portal instance that hosts <see cref="Project"/>, when available.
        /// </summary>
        public Siemens.Engineering.TiaPortal? Portal { get; }

        /// <summary>
        /// Opens or attaches to a TIA Portal project.
        /// </summary>
        /// <param name="path">TIA Portal project file path (.apXX, .alsXX, .zapXX).</param>
        /// <param name="withUserInterface">If true, opens TIA Portal with UI visible.</param>
        /// <param name="logger">Optional logging service for diagnostics.</param>
        /// <exception cref="System.ArgumentException">Thrown when the path is invalid.</exception>
        /// <exception cref="EngineeringException">Thrown when the project cannot be opened.</exception>
        public ProjectAdapter(string path, bool withUserInterface = false, ILoggingService? logger = null)
        {
            _logger = logger;

            // Ensure Openness assembly resolution is active (standalone apps only)
            OpennessInitializer.Initialize();

            var mode = withUserInterface
                ? TiaPortalMode.WithUserInterface
                : TiaPortalMode.WithoutUserInterface;
            var projectPath = new ProjectPath(path);
            var result = ProjectHelper.AttachOrOpen(projectPath, mode, _logger);
            Project = result.Project;
            Portal = result.Portal;
        }

        /// <summary>
        /// Creates an adapter from an already-open project reference.
        /// Used by Add-Ins where the project is provided by TIA Portal.
        /// </summary>
        /// <param name="project">An open TIA Portal project.</param>
        /// <param name="logger">Optional logging service for diagnostics.</param>
        public ProjectAdapter(ProjectBase project, ILoggingService? logger = null)
        {
            Project = project;
            _logger = logger;
        }
    }
}
