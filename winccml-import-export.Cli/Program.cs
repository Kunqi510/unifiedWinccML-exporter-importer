using System;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Services;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;
using Siemens.Engineering.HW;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.Cli
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 2 ||
                !string.Equals(args[0], "inspect", StringComparison.OrdinalIgnoreCase))
            {
                PrintUsage();
                return 2;
            }

            string projectPath = args[1];

            try
            {
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
            catch (Exception ex)
            {
                Console.Error.WriteLine("Inspection failed:");
                Console.Error.WriteLine(ex.ToString());

                Exception innerException = ex.InnerException;
                while (innerException != null)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Inner exception:");
                    Console.Error.WriteLine(innerException.ToString());
                    innerException = innerException.InnerException;
                }

                return 1;
            }
        } 

        private static void PrintUsage()
        {
            Console.WriteLine("WinCCML CLI");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine(
                "  winccml-import-export.Cli inspect <TIA Portal project path>");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine(
                "  winccml-import-export.Cli inspect \"C:\\Projects\\Demo.ap21\"");
        }
    }
}