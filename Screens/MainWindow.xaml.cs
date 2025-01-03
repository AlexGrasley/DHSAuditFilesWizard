using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using NFSUAuditFilesWizard.Interfaces;
using NFSUAuditFilesWizard.Services;

namespace NFSUAuditFilesWizard.Screens;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly IPdfCombinerService _pdfCombinerService;
    private ObservableCollection<FileSystemItemViewModel> _rootItems;
    private const string LoadingString = "Loading...";
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IncludeMaster { get; set; } = false;
    public ObservableCollection<FileSystemItemViewModel> RootItems
    {
        get => _rootItems;
        set
        {
            _rootItems = value;
            OnPropertyChanged();
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


    public ObservableCollection<string> CombinedPdfs { get; set; } = [];
    public SaveLocationModel SaveLocationModel { get; set; } = new SaveLocationModel();

    #region default constructor

    public MainWindow() : this(new DefaultPdfCombinerService())
    {
    }

    #endregion

    public MainWindow(IPdfCombinerService pdfCombinerService)
    {
        InitializeComponent();
        _pdfCombinerService = pdfCombinerService;
        DataContext = this;
        RootItems = [];
        OpenFolderDialog();
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
            SaveLocationModel.SaveLocation = dialog.FolderName;
        }
    }

    private void OpenFolderDialog()
    {
        var dialog = new OpenFolderDialog
        {
            Multiselect = false,
            Title = "Open Folder"
        };

        var result = dialog.ShowDialog();

        if (result == true)
        {
            RootItems = new ObservableCollection<FileSystemItemViewModel>
            {
                FileSystemService.GetFileSystemItems(dialog.FolderName)
            };
            SaveLocationModel.SaveLocation = dialog.FolderName;
        }
    }

    private async void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (!InputsAreValid())
            return;

        StartButton.IsEnabled = false;
        SelectSaveFolderButton.IsEnabled = false;
        FileTreeView.IsEnabled = false;

        CombinedPdfs.Add(LoadingString);

        await RunCombinePdfs();

        CombinedPdfs.Remove(LoadingString);

        StartButton.IsEnabled = true;
        SelectSaveFolderButton.IsEnabled = true;
        FileTreeView.IsEnabled = true;
    }

    private async Task RunCombinePdfs()
    {
        var fullPath = Path.GetFullPath(SaveLocationModel.SaveLocation);

        var progress = new Progress<string>(combinedPdf =>
        {
            CombinedPdfs.Remove(LoadingString);
            CombinedPdfs.Add(combinedPdf);
            CombinedPdfs.Add(LoadingString);
            AllowUIToUpdate();
        });

        try
        {
            await Task.Run(async () =>
            {
                await foreach (var combinedPdf in _pdfCombinerService.CombinePdfsInFolders(RootItems, SaveLocationModel.SaveLocation))
                {
                    ((IProgress<string>)progress).Report(combinedPdf);
                }
            });
        }
        catch (Exception ex)
        {
            ShowMessageBox(ex.Message, "Error", MessageBoxImage.Error);
            return;
        }

        OpenFileExplorer(SaveLocationModel.SaveLocation);
    }

    private bool InputsAreValid()
    {
        if (!SaveLocationModel.IsSet)
        {
            ShowMessageBox(
                "Please select at least one folder",
                "No folders selected",
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void ShowMessageBox(string message, string caption, MessageBoxImage icon)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, icon);

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

    private static void AllowUIToUpdate()
    {
        DispatcherFrame frame = new();
        // DispatcherPriority set to Input, the highest priority
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate (object parameter)
        {
            frame.Continue = false;
            Thread.Sleep(20); // Stop all processes to make sure the UI update is perform
            return null;
        }), null);
        Dispatcher.PushFrame(frame);
        // DispatcherPriority set to Input, the highest priority
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Input, new Action(delegate { }));
    }
}