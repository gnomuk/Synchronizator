using System.ComponentModel;
using System.Windows.Media.Imaging;

public class Settings_ItemSource_Class : INotifyPropertyChanged
{
    private string _ipAddress;
    private BitmapImage _connectionStatus;

    public string IPAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            OnPropertyChanged(nameof(IPAddress));
        }
    }

    public BitmapImage ConnectionStatus
    {
        get => _connectionStatus;
        set
        {
            _connectionStatus = value;
            OnPropertyChanged(nameof(ConnectionStatus));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}