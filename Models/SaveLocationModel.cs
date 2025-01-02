using System.ComponentModel;

public class SaveLocationModel : INotifyPropertyChanged
{
    private string _saveLocation;
    public string SaveLocation
    {
        get => string.IsNullOrWhiteSpace(_saveLocation)
            ? "Select save location"
            : _saveLocation;

        set
        {
            if (_saveLocation != value)
            {
                _saveLocation = value;
                OnPropertyChanged(nameof(SaveLocation));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}