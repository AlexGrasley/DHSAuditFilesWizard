
using System.IO;
using System.Windows;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Microsoft.Win32;

namespace NFSUAuditFilesWizard;

public partial class MainWindow : Window
{
    private List<string> _folderNames = [];

    public MainWindow()
    {
        InitializeComponent();
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
            _folderNames = dialog.FolderNames.ToList();
        }
    }

    private void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (!_folderNames.Any())
            return;

        foreach (var folder in _folderNames)
        {
            var pdfFiles = Directory.GetFiles(folder, "*.pdf");
            if (pdfFiles.Length == 0)
                continue;

            var outputFilePath = Path.Combine(folder, $"{Path.GetFileName(folder)}.pdf");

            using (var pdfWriter = new PdfWriter(outputFilePath))
            using (var pdfDocument = new PdfDocument(pdfWriter))
            {
                var pdfMerger = new PdfMerger(pdfDocument);
                foreach (var pdfFile in pdfFiles)
                {
                    using (var inputPdf = new PdfDocument(new PdfReader(pdfFile)))
                    {
                        pdfMerger.Merge(inputPdf, 1, inputPdf.GetNumberOfPages());
                    }
                }
            }
        }
    }
}