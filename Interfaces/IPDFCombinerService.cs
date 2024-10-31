using System.Windows.Documents.DocumentStructures;

namespace NFSUAuditFilesWizard.Interfaces;

public interface IPDFCombinerService
{
    public IAsyncEnumerable<string> CombinePdfsInFolders(List<string> folderPaths);
    public Task CreateMasterPdf(List<string> combinedPds, string fileName, string filePath);
}