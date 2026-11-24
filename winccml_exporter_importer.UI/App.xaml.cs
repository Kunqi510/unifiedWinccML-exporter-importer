using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// Static constructor — runs before any WPF XAML is processed.
        /// Registers the Openness assembly resolver so that TiaPortalWindow
        /// and other Presentation Framework types can load Siemens.Engineering DLLs.
        /// </summary>
        static App()
        {
            OpennessInitializer.Initialize();
        }
    }
}
