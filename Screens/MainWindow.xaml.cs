using System.IO;
using System.Windows;
using Microsoft.Win32;
using NFSUAuditFilesWizard.Interfaces;
using NFSUAuditFilesWizard.Services;

namespace NFSUAuditFilesWizard.Screens;

public partial class MainWindow : Window
{
    private readonly IPDFCombinerService _pdfCombinerService;

    private List<string> _folderPaths = [];
    private List<string> _combinedPds = [];

    private const int MaxProgress = 100;

    #region default constructor

    public MainWindow() : this(new DefaultPDFCombinerService())
    {
    }

    #endregion

    public MainWindow(IPDFCombinerService pdfCombinerService)
    {
        InitializeComponent();
        _pdfCombinerService = pdfCombinerService;
    }

    private void OnSelectFoldersClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Multiselect = true,
            Title = "Select folders"
        };

        var result = dialog.ShowDialog();

        if (result == true)
        {
            _folderPaths = dialog.FolderNames.ToList();
        }
    }

    private async void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (!InputsAreValid())
            return;

        var fullPath = Path.GetFullPath(_folderPaths.First());
        var parentDirectory = Path.GetDirectoryName(fullPath);
        var masterFileName = MasterFileNameTextBox.Text;

        ShowProgress();

        try
        {
            await foreach (var combinedPdf in _pdfCombinerService.CombinePdfsInFolders(_folderPaths))
            {
                _combinedPds.Add(combinedPdf);
                UpdateProgressBar(_combinedPds.Count);
            }

            await _pdfCombinerService.CreateMasterPdf(_combinedPds, masterFileName, parentDirectory);
        }
        catch (Exception ex)
        {
            ShowMessageBox(ex.Message, "Error", MessageBoxImage.Error, true);
        }

        HideProgress();
    }

    private void HideProgress()
    {
        ProgressBar.Visibility = Visibility.Collapsed;
    }

    private void ShowProgress()
    {
        ProgressBar.Visibility = Visibility.Visible;
        ProgressBar.IsIndeterminate = false;
        ProgressBar.Value = 0;
    }

    private bool InputsAreValid()
    {
        if (!_folderPaths.Any())
        {
            ShowMessageBox(
                "Please select at least one folder",
                "No folders selected",
                MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(MasterFileNameTextBox.Text))
        {
            ShowMessageBox(
                "Please enter a Master File Name",
                "Missing Master File Name",
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void UpdateProgressBar(int count)
    {
        ProgressBar.Value = (count + 1) * MaxProgress / (double)_folderPaths.Count;
    }

    private void ShowMessageBox(string message, string caption, MessageBoxImage icon, bool hideProgressBar = false)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, icon);
        ProgressBar.Visibility = hideProgressBar
            ? Visibility.Collapsed
            : ProgressBar.Visibility;
    }
}