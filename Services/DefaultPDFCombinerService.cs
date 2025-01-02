using System.Collections.ObjectModel;
using System.Windows.Navigation;
using NFSUAuditFilesWizard.Interfaces;

namespace NFSUAuditFilesWizard.Services;

public class DefaultPDFCombinerService : IPDFCombinerService
{
    public async IAsyncEnumerable<string> CombinePdfsInFolders(
        ObservableCollection<FileSystemItemViewModel> folderPaths)
    {
        yield break;
    }

    public async Task CreateMasterPdf(List<string> combinedPds, string fileName, string filePath)
    {
        //does nothing
    }
}