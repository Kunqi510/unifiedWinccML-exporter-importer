using System;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Actions;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Services;
using Siemens.Engineering.HW;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Cli
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 2;
            }

            try
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "inspect":
                        return RunInspect(args);

                    case "export-screen":
                        return RunExportScreen(args);

                    default:
                        PrintUsage();
                        return 2;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("CLI operation failed:");
                Console.Error.WriteLine(ex);

                return 1;
            }
        }

        private static int RunInspect(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return 2;
            }

            string projectPath = args[1];
            ILoggingService logger = new LoggingService(isAddIn: false);

            var adapter = new ProjectAdapter(
                projectPath,
                withUserInterface: false,
                logger: logger);

            var service = new WinccmlService(logger);
            Device device = service.FindFirstWinCCUnifiedDevice(adapter.Project);

            if (device == null)
            {
                Console.Error.WriteLine(
                    "No WinCC Unified HMI device was found in the project.");

                return 1;
            }

            Console.WriteLine("Project opened successfully.");
            Console.WriteLine("HMI device: " + device.Name);
            Console.WriteLine();
            Console.WriteLine("Screens:");

            foreach (string screenName in service.GetScreenNames(device))
            {
                Console.WriteLine("  - " + screenName);
            }

            Console.WriteLine();
            Console.WriteLine("HMI tag tables:");

            foreach (string tagTableName in service.GetTagTableNames(device))
            {
                Console.WriteLine("  - " + tagTableName);
            }

            return 0;
        }

        private static int RunExportScreen(string[] args)
        {
            if (args.Length != 4)
            {
                PrintUsage();
                return 2;
            }

            string projectPath = args[1];
            string screenName = args[2];
            string destinationFolder = args[3];

            ILoggingService logger = new LoggingService(isAddIn: false);

            var adapter = new ProjectAdapter(
                projectPath,
                withUserInterface: false,
                logger: logger);

            var action = new ExportWinccmlAction(
                adapter.Project,
                logger);

            action.ExportScreen(
                screenName,
                destinationFolder);

            Console.WriteLine("Screen exported successfully.");
            Console.WriteLine("Screen: " + screenName);
            Console.WriteLine("Destination: " + destinationFolder);

            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("WinCCML CLI");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine();
            Console.WriteLine(
                "  inspect <TIA Portal project path>");
            Console.WriteLine(
                "  export-screen <project path> <screen name> <destination folder>");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine(
                "  inspect \"C:\\Projects\\Demo.ap21\"");
            Console.WriteLine(
                "  export-screen \"C:\\Projects\\Demo.ap21\" \"Overview\" \"C:\\WinCCML\"");
        }
    }
}