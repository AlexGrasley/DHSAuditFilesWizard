using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Channels;
using System.Windows.Shapes;
using iText.Forms;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using iText.Kernel.Utils;
using NFSUAuditFilesWizard.Interfaces;
using Path = System.IO.Path;

namespace NFSUAuditFilesWizard.Services;

public class PdfCombinerService : IPdfCombinerService
{
    public async IAsyncEnumerable<string> CombinePdfsInFolders(
        ObservableCollection<FileSystemItemViewModel> folderPaths,
        string saveLocation)
    {
        var folders = FileSystemService.FlattenFileSystemItems(folderPaths);
        var channel = Channel.CreateUnbounded<string>();

        var tasks = folders.Select(async folder =>
        {
            var pdfFiles = folder.Children;
            if (pdfFiles.Count == 0)
                return;

            var outputFilePath = Path.Combine(saveLocation, $"{Path.GetFileName(folder.Name)}.pdf");

            // Don't want to delete existing files, just append date/time to create a new file
            if (File.Exists(outputFilePath))
                outputFilePath = Path.Combine(saveLocation, $"{Path.GetFileName(folder.Name)}-{DateTime.Now:yyMMddThhmmss}.pdf");

            await using (var pdfWriter = new PdfWriter(outputFilePath))
            using (var pdfDocument = new PdfDocument(pdfWriter))
            {
                var pdfMerger = new PdfMerger(pdfDocument, GetMergerProperties());
                foreach (var pdfFile in pdfFiles)
                {
                    using var inputPdf = new PdfDocument(new PdfReader(pdfFile.Path));
                    pdfMerger.Merge(inputPdf, 1, inputPdf.GetNumberOfPages());
                }
            }

            await channel.Writer.WriteAsync(outputFilePath);
        }).ToList();

        _ = Task.WhenAll(tasks).ContinueWith(_ => channel.Writer.Complete());

        await foreach (var result in channel.Reader.ReadAllAsync())
        {
            yield return result;
        }
    }

    public async Task CreateMasterPdf(List<string> combinedPds, string fileName, string? filePath)
    {
        if (combinedPds.Count == 0)
            throw new ValidationException("No PDFs to combine. Please select folders containing PDFs.");

        if (filePath == null)
            throw new ValidationException("Unable to find selected directory. Please try again.");

        var outputFilePath = Path.Combine(filePath, $"{fileName}.pdf");

        //Don't want to delete existing files, just append date/time to create a new file
        if (Path.Exists(outputFilePath))
            outputFilePath = Path.Combine(filePath, $"{Path.GetFileName(fileName)}-{DateTime.Now:yyMMddThhmmss}.pdf");

        await using var pdfWriter = new PdfWriter(outputFilePath);
        using var pdfDocument = new PdfDocument(pdfWriter);
        var pdfMerger = new PdfMerger(pdfDocument, GetMergerProperties());

        var rootOutline = pdfDocument.GetOutlines(false);

        var pages = 1;
        foreach (var pdfFile in combinedPds)
        {
            using var inputPdf = new PdfDocument(new PdfReader(pdfFile));
            var pdfFileName = Path.GetFileNameWithoutExtension(pdfFile);
            var inputOutline = inputPdf.GetOutlines(true);
            if (inputOutline?.GetDestination() == null)
            {
                inputOutline?.AddDestination(PdfExplicitDestination.CreateFit(inputPdf.GetFirstPage()));
            }

            var subPages = inputPdf.GetNumberOfPages();
            pdfMerger.Merge(inputPdf, 1, subPages);

            var link = rootOutline.AddOutline(pdfFileName);
            CopyOutline(inputOutline, link, pdfDocument);
            link.AddDestination(PdfExplicitDestination.CreateFit(pdfDocument.GetPage(pages)));

            pages += subPages;
        }
    }

    private void CopyOutline(PdfOutline sourceOutline, PdfOutline targetParentOutline, PdfDocument targetDocument)
    {
        // Create a new outline in the target document
        var newOutline = targetParentOutline.AddOutline(sourceOutline.GetTitle());

        if (sourceOutline.GetDestination() != null)
            newOutline.AddDestination(sourceOutline?.GetDestination());

        // Copy other properties if needed (e.g., color, style)
        var color = sourceOutline?.GetColor();
        if (color != null)
        {
            newOutline.SetColor(color);
        }

        var style = sourceOutline?.GetStyle();
        if (style.HasValue)
        {
            newOutline.SetStyle(style.Value);
        }

        // Recursively copy child outlines
        foreach (var child in sourceOutline?.GetAllChildren() ?? [])
        {
            CopyOutline(child, newOutline, targetDocument);
        }
    }

    private PdfMergerProperties GetMergerProperties()
    {
        return new PdfMergerProperties()
            .SetMergeOutlines(true)
            .SetMergeTags(true)
            .SetCloseSrcDocuments(true);
    }
}