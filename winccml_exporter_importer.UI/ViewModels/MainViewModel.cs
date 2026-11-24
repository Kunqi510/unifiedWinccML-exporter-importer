using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Actions;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Interfaces;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Services;
using Siemens.Application.TiaPortal.WinccmlExporterImporter.Core.Utils;
using Siemens.Engineering;
using Siemens.Collaboration.Net;
using Siemens.Collaboration.Net.Presentation.ViewModels;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.UI.ViewModels;

/// <summary>
/// Main view model for the WinCCML exporter/importer UI.
/// </summary>
internal class MainViewModel : ViewModel<MainViewModel>
{
    private readonly ILoggingService _logger = new LoggingService(isAddIn: false);

    /// <summary>
    /// TIA Portal project file path entered by the user.
    /// </summary>
    public string ProjectPath
    {
        get => this.Get(() => string.Empty);
        set => this.Set(value);
    }

    /// <summary>
    /// Target path entered by the user (a folder for export, a WinCCML file for import).
    /// </summary>
    public string TargetPath
    {
        get => this.Get(() => string.Empty);
        set => this.Set(value);
    }

    /// <summary>
    /// Status/result text displayed in the main window.
    /// </summary>
    public string StatusText
    {
        get => this.Get(() => "Ready. Enter a TIA Portal project path and choose a WinCCML export or import operation.");
        set => this.Set(value);
    }

    /// <summary>
    /// Indicates whether a long-running operation is in progress.
    /// </summary>
    public bool IsBusy
    {
        get => this.Get(() => false);
        set => this.Set(value);
    }

    /// <summary>
    /// Command to browse for a TIA Portal project file.
    /// </summary>
    public ICommand BrowseProjectCommand { get; }

    /// <summary>
    /// Command to browse for the target folder (export) or WinCCML file (import).
    /// </summary>
    public ICommand BrowseTargetCommand { get; }

    /// <summary>
    /// Command that executes either export or import based on <see cref="IsImport"/>.
    /// </summary>
    public ICommand ActionCommand { get; }

    /// <summary>
    /// Whether the UI is in import mode. When false, UI is in export mode.
    /// </summary>
    public bool IsImport
    {
        get => this.Get(() => false);
        set
        {
            this.Set(value);
            UpdateModeTexts();
        }
    }

    /// <summary>
    /// Whether the WinCCML operation targets an HMI tag table. When false, it targets a screen.
    /// </summary>
    public bool IsTagTable
    {
        get => this.Get(() => false);
        set
        {
            this.Set(value);
            UpdateModeTexts();
        }
    }

    /// <summary>
    /// Name of the screen (export/import) or HMI tag table (export) to operate on.
    /// </summary>
    public string ElementName
    {
        get => this.Get(() => string.Empty);
        set => this.Set(value);
    }

    /// <summary>
    /// Label text for the element name row ("Screen name:" or "Tag table name:").
    /// </summary>
    public string ElementLabel
    {
        get => this.Get(() => "Screen name:");
        private set => this.Set(value);
    }

    /// <summary>
    /// Whether the element name field should be shown: always for export, and for import
    /// only when targeting a tag table (screen import works across the whole device).
    /// </summary>
    public bool IsElementNameVisible
    {
        get => this.Get(() => true);
        private set => this.Set(value);
    }

    /// <summary>
    /// Text shown on the main action button ("Export" or "Import").
    /// </summary>
    public string ActionText
    {
        get => this.Get(() => "Export");
        private set => this.Set(value);
    }

    /// <summary>
    /// Guidance shown for the currently selected operation.
    /// </summary>
    public string InformationText
    {
        get => this.Get(() => "Export the WinCC Unified HMI device of a TIA Portal project as a WinCCML file. Select a project and a target folder for the exported file.");
        private set => this.Set(value);
    }

    /// <summary>
    /// Label text for the target path row ("Target folder:" or "WinCCML file:").
    /// </summary>
    public string TargetLabel
    {
        get => this.Get(() => "Target folder:");
        private set => this.Set(value);
    }

    /// <summary>
    /// Command to open the application manual PDF.
    /// </summary>
    public ICommand OpenManualCommand { get; }

    /// <summary>
    /// Command to open the OSS information file.
    /// </summary>
    public ICommand OpenOssCommand { get; }

    public MainViewModel()
    {
        BrowseProjectCommand = new RelayCommand(ExecuteBrowseProject);
        BrowseTargetCommand = new RelayCommand(ExecuteBrowseTarget);
        ActionCommand = new AsyncRelayCommand(ExecuteActionAsync);
        OpenManualCommand = new RelayCommand(ExecuteOpenManual);
        OpenOssCommand = new RelayCommand(ExecuteOpenOss);

        UpdateModeTexts();
    }

    private void ExecuteBrowseProject()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select TIA Portal Project",
            Filter = "TIA Portal Projects (*.ap*)|*.ap*|All Files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            ProjectPath = dialog.FileName;
        }
    }

    private void ExecuteOpenManual()
    {
        try
        {
            ManualOssManager.OpenManual(_logger);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Open Manual failed: {ex.Message}");
            StatusText = "Could not open manual. Check the application log for details.";
        }
    }

    private void ExecuteOpenOss()
    {
        try
        {
            ManualOssManager.OpenOss(_logger);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Open OSS failed: {ex.Message}");
            StatusText = "Could not open OSS info. Check the application log for details.";
        }
    }
    private void ExecuteBrowseTarget()
    {
        if (IsImport)
        {
            // Import mode: pick the WinCCML source file.
            var dialog = new OpenFileDialog
            {
                Title = "Select WinCCML File",
                Filter = "WinCCML Files (*.wincml;*.xml)|*.wincml;*.xml|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                TargetPath = dialog.FileName;
            }
        }
        else
        {
            // Export mode: pick the destination folder.
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = "Select export target folder";
                var result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    TargetPath = dlg.SelectedPath;
            }
        }
    }

    private void UpdateModeTexts()
    {
        ActionText = IsImport ? "Import" : "Export";
        string elementKind = IsTagTable ? "HMI tag table" : "screen";
        InformationText = IsImport
            ? $"Import a WinCCML file into a {elementKind} of the WinCC Unified HMI device of a TIA Portal project. Select a project and the WinCCML file to import."
            : $"Export a {elementKind} of the WinCC Unified HMI device of a TIA Portal project as a WinCCML file. Select a project, {elementKind} name, and a target folder for the exported file.";
        TargetLabel = IsImport ? "WinCCML file:" : "Target folder:";
        ElementLabel = IsTagTable ? "Tag table name:" : "Screen name:";
        IsElementNameVisible = !IsImport || IsTagTable;
    }

    private async Task ExecuteActionAsync()
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
        {
            StatusText = "Please enter a TIA Portal project path.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetPath))
        {
            StatusText = IsImport ? "Please enter a WinCCML file path." : "Please enter an export folder path.";
            return;
        }

        if (!IsImport && string.IsNullOrWhiteSpace(ElementName))
        {
            StatusText = IsTagTable ? "Please enter a tag table name." : "Please enter a screen name.";
            return;
        }

        if (IsImport && IsTagTable && string.IsNullOrWhiteSpace(ElementName))
        {
            StatusText = "Please enter a tag table name.";
            return;
        }

        IsBusy = true;
        StatusText = IsImport ? "Preparing WinCCML import..." : "Preparing WinCCML export...";

        bool isImport = IsImport;
        bool isTagTable = IsTagTable;
        string targetPath = TargetPath;
        string elementName = ElementName;

        try
        {
            await Task.Run(() =>
            {
                var adapter = new ProjectAdapter(ProjectPath, logger: _logger);
                if (isImport)
                {
                    void RunImport()
                    {
                        var action = new ImportWinccmlAction(adapter.Project, _logger);
                        if (isTagTable)
                        {
                            action.ImportTagTable(elementName, targetPath);
                        }
                        else
                        {
                            action.ImportScreen(targetPath);
                        }
                    }

                    // Prefer acquiring exclusive access on the portal instance when available.
                    if (adapter.Portal != null)
                    {
                        using (ExclusiveAccess exclusive = adapter.Portal.ExclusiveAccess("Import WinCCML"))
                        {
                            using (var transaction = exclusive.Transaction(adapter.Project, "Import WinCCML"))
                            {
                                RunImport();

                                // A transaction rolls back on disposal unless the commit is explicitly requested.
                                transaction.CommitOnDispose();
                            }

                            // The transaction must be disposed before saving, but the save should remain
                            // inside the same ExclusiveAccess scope.
                            adapter.Project.Save();
                        }
                    }
                    else
                    {
                        RunImport();
                    }
                }
                else
                {
                    var action = new ExportWinccmlAction(adapter.Project, _logger);
                    if (isTagTable)
                    {
                        action.ExportTagTable(elementName, targetPath);
                    }
                    else
                    {
                        action.ExportScreen(elementName, targetPath);
                    }
                }
            });

            StatusText = isImport
                ? $"WinCCML imported successfully. Source: {targetPath}"
                : $"WinCCML exported successfully. Destination: {targetPath}";
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Action failed: {ex.Message}");
            StatusText = isImport
                ? $"Could not import WinCCML: {ex.Message} Ensure the project path is correct and TIA Portal is available."
                : $"Could not export WinCCML: {ex.Message} Ensure the project path is correct and TIA Portal is available.";
        }
        finally
        {
            IsBusy = false;
        }
    }

}
