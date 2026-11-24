#nullable enable
using System;
using System.IO;
using System.Linq;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;
using Siemens.Collaboration.Net;
using Siemens.Engineering;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Utils
{
    /// <summary>
    /// Provides TIA Portal process attachment and project opening capabilities.
    /// </summary>
    internal static class ProjectHelper
    {
        /// <summary>
        /// Attaches to an existing TIA Portal process with the matching project, or opens a new one.
        /// </summary>
        /// <exception cref="EngineeringException">Thrown when unable to open the project.</exception>
        public static (ProjectBase Project, Engineering.TiaPortal? Portal) AttachOrOpen(ProjectPath projectPath, TiaPortalMode mode, ILoggingService? logger = null)
        {
            var attached = TryAttach(projectPath, logger);
            if (attached.Project != null) return attached;
            logger?.LogInfo("No existing TIA Portal process with matching project found");
            var opened = TryOpen(projectPath, mode, logger);

            if (opened.Project == null)
                throw new EngineeringException(
                    $"Unable to retrieve TIA Portal instance of Project {projectPath.FilePath}");

            logger?.LogInfo("Started new TIA Portal with project");
            return opened;
        }

        private static (ProjectBase? Project, Engineering.TiaPortal? Portal) TryAttach(ProjectPath projectPath, ILoggingService? logger)
        {
            var tiaProcesses = Api.Global.Openness().GetTiaPortalProcesses();
            logger?.LogInfo("Trying to get running TIA Portal Processes");
            if (tiaProcesses == null)
            {
                logger?.LogInfo("No running TIA Portal Processes found");
                return (null, null);
            }

            foreach (var tiaPortalProcess in tiaProcesses)
            {
                logger?.LogInfo($"Found TIA Portal Process (PID: {tiaPortalProcess.ProcessId}) with project {tiaPortalProcess.ProjectPath}");

                switch (projectPath.Type)
                {
                    case ProjectType.ArchivedLocalProject:
                        return (null, null);
                    case ProjectType.LocalProject:
                        if (tiaPortalProcess.ProjectPath == projectPath.FilePath)
                        {
                            var portal = tiaPortalProcess.Attach();
                            return (portal.Projects.First(project => project.IsPrimary), portal);
                        }
                        break;
                    case ProjectType.LocalSession:
                        var procPath = tiaPortalProcess.ProjectPath;
                        if (Path.GetFileNameWithoutExtension(procPath.ToString()) == projectPath.ProjectName &&
                            Path.Combine(Path.GetDirectoryName(procPath) ?? "", "..") == projectPath.ProjectRoot)
                        {
                            logger?.LogInfo($"Attaching to matching TIA Portal Process with PID {tiaPortalProcess.ProcessId}");
                            var portal = tiaPortalProcess.Attach();
                            logger?.LogInfo($"Attaching to matching TIA Portal Process with PID {tiaPortalProcess.ProcessId}");
                            return (portal.Projects.First(project => project.IsPrimary), portal);
                        }
                        break;
                }
            }
            return (null, null);
        }

        private static (ProjectBase? Project, Engineering.TiaPortal? Portal) TryOpen(ProjectPath projectPath, TiaPortalMode mode, ILoggingService? logger)
        {
            logger?.LogInfo($"Starting new TIA Portal Process with mode {mode}");
            var portal = new Engineering.TiaPortal(mode);

            ProjectBase? project = projectPath.Type switch
            {
                ProjectType.ArchivedLocalProject => RetrieveArchivedProject(portal, projectPath, logger),
                ProjectType.LocalProject => portal.Projects.Open(new FileInfo(projectPath.FilePath)),
                ProjectType.LocalSession => portal.LocalSessions.Open(new FileInfo(projectPath.FilePath)).Project,
                _ => null
            };

            return (project, portal);
        }

        private static Project RetrieveArchivedProject(Engineering.TiaPortal portal, ProjectPath projectPath, ILoggingService? logger)
        {
            var archiveFile = new FileInfo(projectPath.FilePath);
            var targetDir = archiveFile.Directory
                ?? throw new EngineeringException($"Cannot determine parent directory of {projectPath.FilePath}");

            logger?.LogInfo($"Retrieving archived project to {targetDir.FullName}");
            return portal.Projects.Retrieve(archiveFile, targetDir);
        }
    }
}
