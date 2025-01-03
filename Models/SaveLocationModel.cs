using System.ComponentModel;

public class SaveLocationModel : INotifyPropertyChanged
{
    private string _saveLocation;
    private bool _isSet;
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
                _isSet = true;
                OnPropertyChanged(nameof(SaveLocation));
            }
        }
    }

    public bool IsSet => _isSet;


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}