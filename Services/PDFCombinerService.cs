using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows.Controls;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using NFSUAuditFilesWizard.Interfaces;

namespace NFSUAuditFilesWizard.Services;

public class PDFCombinerService : IPDFCombinerService
{
    public async IAsyncEnumerable<string> CombinePdfsInFolders(
        ObservableCollection<FileSystemItemViewModel> folderPaths)
    {
        var folders = FileSystemService.FlattenFileSystemItems(folderPaths);

        foreach (var folder in folders)
        {
            var pdfFiles = folder.Children;
            if (pdfFiles.Count == 0)
                continue;

            var outputFilePath = Path.Combine(folder.Path, $"{Path.GetFileName(folder.Name)}.pdf");

            //Don't want to delete existing files, just append date/time to create a new file
            if (Path.Exists(outputFilePath))
                outputFilePath = Path.Combine(folder.Path, $"{Path.GetFileName(folder.Name)}-{DateTime.Now:yyMMddThhmmss}.pdf");

            await using (var pdfWriter = new PdfWriter(outputFilePath))
            using (var pdfDocument = new PdfDocument(pdfWriter))
            {
                var pdfMerger = new PdfMerger(pdfDocument);
                foreach (var pdfFile in pdfFiles)
                {
                    using var inputPdf = new PdfDocument(new PdfReader(pdfFile.Path));
                    pdfMerger.Merge(inputPdf, 1, inputPdf.GetNumberOfPages());
                }
            }
            yield return outputFilePath;
        }
    }

    public async Task CreateMasterPdf(List<string> combinedPds, string fileName, string? filePath)
    {
        if (combinedPds.Count == 0)
            throw new ValidationException("No PDFs to combine. Please select folders containing PDFs.");

        if (filePath == null)
            throw new ValidationException("Unable to find parent directory. Please try again.");

        var outputFilePath = Path.Combine(filePath, $"{fileName}.pdf");

        //Don't want to delete existing files, just append date/time to create a new file
        if (Path.Exists(outputFilePath))
            outputFilePath = Path.Combine(filePath, $"{Path.GetFileName(fileName)}-{DateTime.Now:yyMMddThhmmss}.pdf");

        await using var pdfWriter = new PdfWriter(outputFilePath);
        using var pdfDocument = new PdfDocument(pdfWriter);
        var pdfMerger = new PdfMerger(pdfDocument);

        foreach (var pdfFile in combinedPds)
        {
            using var inputPdf = new PdfDocument(new PdfReader(pdfFile));
            pdfMerger.Merge(inputPdf, 1, inputPdf.GetNumberOfPages());
        }
    }
}