using System.Collections.ObjectModel;
using System.IO;
using NFSUAuditFilesWizard.Interfaces;

public static class FileSystemService
{
    private const string Searchpattern = "*.pdf";
    public static FileSystemItemViewModel GetFileSystemItems(string path)
    {
        var root = new FileSystemItemViewModel
        {
            Name = Path.GetFileName(path),
            Path = path,
            IsExpanded = true
        };

        foreach (var directory in Directory.GetDirectories(path))
        {
            var directoryItem = GetFileSystemItems(directory);
            if (directoryItem.Children.Any())
            {
                root.Children.Add(directoryItem);
            }
        }

        foreach (var file in Directory.GetFiles(path, Searchpattern))
        {
            root.Children.Add(new FileSystemItemViewModel
            {
                Name = Path.GetFileName(file),
                Path = file,
                IsExpanded = true,
            });
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
                SetChildrenChecked(item.Children, true);

                var pdfChildren =
                    item.Children
                        .Where(child => child.Path.EndsWith(".pdf"))
                        .ToList();

                if (pdfChildren.Any())
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

    private static void SetChildrenChecked(ObservableCollection<FileSystemItemViewModel> children, bool isChecked)
    {
        foreach (var child in children)
        {
            child.IsChecked = isChecked;
            if (child.HasChildren)
            {
                SetChildrenChecked(child.Children, isChecked);
            }
        }
    }
}