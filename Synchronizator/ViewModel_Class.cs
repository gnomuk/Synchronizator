using System.Collections.ObjectModel;

public class ViewModel
{
    public ObservableCollection<LVItemSource_Class> Items { get; set; }

    public ViewModel()
    {
        Items = new ObservableCollection<LVItemSource_Class>();
    }

    public void AddItem(string parameter)
    {
        Items.Add(new LVItemSource_Class { Parameter = parameter });
    }
}