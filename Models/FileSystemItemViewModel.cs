using System.Collections.ObjectModel;
using System.ComponentModel;

public class FileSystemItemViewModel : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsChecked { get; set; }
    public bool HasChildren => Children.Count > 0;
    public ObservableCollection<FileSystemItemViewModel> Children { get; set; }

    public FileSystemItemViewModel()
    {
        Children = new ObservableCollection<FileSystemItemViewModel>();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}