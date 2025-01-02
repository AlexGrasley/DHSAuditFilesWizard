using System.Collections.ObjectModel;
using System.IO;
using NFSUAuditFilesWizard.Interfaces;

public static class FileSystemService
{
    public static FileSystemItemViewModel GetFileSystemItems(string path)
    {
        var root = new FileSystemItemViewModel { Name = System.IO.Path.GetFileName(path), Path = path };

        foreach (var directory in Directory.GetDirectories(path))
        {
            var directoryItem = GetFileSystemItems(directory);
            if (directoryItem.Children.Any())
            {
                root.Children.Add(directoryItem);
            }
        }

        foreach (var file in Directory.GetFiles(path, "*.pdf"))
        {
            root.Children.Add(new FileSystemItemViewModel { Name = System.IO.Path.GetFileName(file), Path = file });
        }

        return root;
    }
    public static List<FileSystemItemViewModel> FlattenFileSystemItems(ObservableCollection<FileSystemItemViewModel> items)
    {
        var result = new List<FileSystemItemViewModel>();

        foreach (var item in items)
        {
            if (item.IsChecked)
            {
                var pdfChildren =
                    item.Children
                        .Where(child => child.Path.EndsWith(".pdf"))
                        .ToList();

                if (!pdfChildren.Any())
                    continue;

                result.Add(new FileSystemItemViewModel
                {
                    Name = item.Name,
                    Path = item.Path,
                    Children = new ObservableCollection<FileSystemItemViewModel>(pdfChildren)
                });
            }

            result.AddRange(FlattenFileSystemItems(item.Children));
        }

        return result;
    }
}