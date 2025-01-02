using System.Collections.ObjectModel;
using System.Windows.Documents.DocumentStructures;

namespace NFSUAuditFilesWizard.Interfaces;

public interface IPDFCombinerService
{
    public IAsyncEnumerable<string> CombinePdfsInFolders(ObservableCollection<FileSystemItemViewModel> folderPaths);
    public Task CreateMasterPdf(List<string> combinedPds, string fileName, string filePath);
}