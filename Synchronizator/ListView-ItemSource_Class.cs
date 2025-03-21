using System.ComponentModel;
using System.Windows;

public class LVItemSource_Class : INotifyPropertyChanged
{
    private bool _isChecked;


    public string Parameter { get; set; }
    public Visibility Show_Gear { get; set; }
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked != value)
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
