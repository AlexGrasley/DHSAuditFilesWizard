using System.Collections.ObjectModel;
using System.Windows.Documents.DocumentStructures;

namespace NFSUAuditFilesWizard.Interfaces;

public interface IPdfCombinerService
{
    public IAsyncEnumerable<string> CombinePdfsInFolders(
        ObservableCollection<FileSystemItemViewModel> folderPaths,
        string saveLocation);
    public Task CreateMasterPdf(List<string> combinedPds, string fileName, string filePath);
}