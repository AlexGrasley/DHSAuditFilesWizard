using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NFSUAuditFilesWizard.Interfaces;
using NFSUAuditFilesWizard.Services;

namespace NFSUAuditFilesWizard.Screens;

public partial class MainWindow : Window
{
    private readonly IPDFCombinerService _pdfCombinerService;

    private bool _includeMaster = false;
    private string _saveLocation = "";
    private List<string> _combinedPds = [];

    private const int MaxProgress = 100;

    public ObservableCollection<FileSystemItemViewModel> RootItems { get; set; }

    #region default constructor

    public MainWindow() : this(new DefaultPDFCombinerService())
    {
    }

    #endregion

    public MainWindow(IPDFCombinerService pdfCombinerService)
    {
        InitializeComponent();
        _pdfCombinerService = pdfCombinerService;
        DataContext = new SaveLocationModel();
        RootItems = new ObservableCollection<FileSystemItemViewModel>
        {
            FileSystemService.GetFileSystemItems(@$"C:\Programming Projects\Test Data")
        };
    }

    private void OnSelectFoldersClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Multiselect = false,
            Title = "Select folder"
        };

        var result = dialog.ShowDialog();

        if (result == true)
        {
            if (DataContext is SaveLocationModel location)
                location.SaveLocation = dialog.FolderName;
        }
    }

    private async void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (!InputsAreValid())
            return;

        var saveLocation = ((SaveLocationModel)DataContext).SaveLocation;

        var fullPath = Path.GetFullPath(saveLocation);
        var parentDirectory = Path.GetDirectoryName(fullPath);
        var masterFileName = MasterFileNameTextBox.Text;

        ShowProgress();

        try
        {
            await foreach (var combinedPdf in _pdfCombinerService.CombinePdfsInFolders(RootItems))
            {
                _combinedPds.Add(combinedPdf);
                UpdateProgressBar(_combinedPds.Count);
            }

            if (_includeMaster)
                await _pdfCombinerService.CreateMasterPdf(_combinedPds, masterFileName, parentDirectory);
        }
        catch (Exception ex)
        {
            ShowMessageBox(ex.Message, "Error", MessageBoxImage.Error, true);
            return;
        }

        OpenFileExplorer(saveLocation);
    }

    private void HideProgress()
    {
        // ProgressBar.Visibility = Visibility.Collapsed;
        // SelectFoldersButton.IsEnabled = true;
        // StartButton.IsEnabled = true;
        // MasterFileNameTextBox.IsEnabled = true;
    }

    private void ShowProgress()
    {
        // ProgressBar.Visibility = Visibility.Visible;
        // ProgressBar.IsIndeterminate = false;
        // ProgressBar.Value = 0;
        // SelectFoldersButton.IsEnabled = false;
        // StartButton.IsEnabled = false;
        // MasterFileNameTextBox.IsEnabled = false;
    }

    private bool InputsAreValid()
    {
        if (string.IsNullOrWhiteSpace(_saveLocation))
        {
            ShowMessageBox(
                "Please select at least one folder",
                "No folders selected",
                MessageBoxImage.Warning);
            return false;
        }

        if (_includeMaster && string.IsNullOrWhiteSpace(MasterFileNameTextBox.Text))
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
        // ProgressBar.Value = (count + 1) * MaxProgress / (double)_saveLocation.Count;
    }

    private void ShowMessageBox(string message, string caption, MessageBoxImage icon, bool hideProgressBar = false)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, icon);

        if (hideProgressBar)
            HideProgress();
    }

    private void TreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        TreeViewScrollViewer.ScrollToVerticalOffset(TreeViewScrollViewer.VerticalOffset - e.Delta / 3);
        e.Handled = true;
    }

    private void OpenFileExplorer(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }
}