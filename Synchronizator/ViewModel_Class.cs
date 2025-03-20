using System.Collections.ObjectModel;
using System.Windows;

public class ViewModel
{
    public ObservableCollection<LVItemSource_Class> Items { get; set; }

    public ViewModel()
    {
        Items = new ObservableCollection<LVItemSource_Class>();
    }

    public void AddItem(string parameter, bool Show_Gear)
    {
        var visibility = Visibility.Visible;

        if (Show_Gear == true) visibility = Visibility.Visible;
        else visibility = Visibility.Hidden;
        Items.Add(new LVItemSource_Class { Parameter = parameter, Show_Gear = visibility });
    }
}