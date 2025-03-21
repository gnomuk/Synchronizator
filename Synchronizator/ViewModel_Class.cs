using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

public class ViewModel : INotifyPropertyChanged
{
    public ObservableCollection<LVItemSource_Class> Items { get; set; }

    public ViewModel()
    {
        Items = new ObservableCollection<LVItemSource_Class>();
    }

    public void AddItem(string parameter, bool Show_Gear, bool isChecked )
    {
        var visibility = Visibility.Visible;

        if (Show_Gear == true) visibility = Visibility.Visible;
        else visibility = Visibility.Hidden;
        Items.Add(new LVItemSource_Class { Parameter = parameter, Show_Gear = visibility, IsChecked = isChecked });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

