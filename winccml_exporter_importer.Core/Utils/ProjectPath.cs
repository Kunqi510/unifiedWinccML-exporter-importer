using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Utils
{
    /// <summary>
    /// Parses a TIA Portal project file path and extracts version and type information.
    /// Supports .apXX, .alsXX, .zapXX extensions where XX is V16-V21.
    /// </summary>
    public class ProjectPath
    {
        public readonly string FilePath;
        public readonly int Version;
        public readonly ProjectType Type;

        /// <summary>
        /// Returns the project file name without extension.
        /// </summary>
        public string ProjectName => Path.GetFileNameWithoutExtension(FilePath);

        /// <summary>
        /// Returns the root directory of the project.
        /// </summary>
        public string ProjectRoot
        {
            get
            {
                var root = Path.GetDirectoryName(FilePath);
                if (string.IsNullOrEmpty(root))
                    throw new ArgumentException(
                        "Root path of the project is null or empty because no directory information could be found");
                return root;
            }
        }

        /// <summary>
        /// Parses a TIA Portal project file path into a structure containing version and type.
        /// </summary>
        /// <param name="path">TIA Portal project file path, must be rooted.</param>
        /// <exception cref="ArgumentException">Thrown when the file does not exist or extension is invalid.</exception>
        public ProjectPath(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException(
                    $"Given project file for {path} does not exist on the filesystem");
            }

            this.FilePath = path;
            var rex = new Regex(@"\.(z?a(?>p|ls))(\d{2})", RegexOptions.Compiled);

            var match = rex.Match(Path.GetExtension(FilePath));

            if (!match.Success)
                throw new ArgumentException(
                    $"Given file extension {path} does not match expected format (.apXX/.alsXX/.zapXX)");

            this.Type = match.Groups[1].Value switch
            {
                "ap" => ProjectType.LocalProject,
                "als" => ProjectType.LocalSession,
                "zap" => ProjectType.ArchivedLocalProject,
                _ => throw new ArgumentException(
                    $"Unrecognized file extension, expected .apXX/.zapXX/.alsXX got {match.Groups[0].Value}")
            };

            Version = int.Parse(match.Groups[2].Value);
            if (Version < 16 || Version > 21)
            {
                throw new ArgumentException(
                    $"Version {Version} set for the project is not within the version range V16-V21");
            }
        }
    }
}
