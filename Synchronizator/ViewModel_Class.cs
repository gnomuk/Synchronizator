using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

public class ViewModel : INotifyPropertyChanged
{
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isMonitoring = false;

    public ObservableCollection<LVItemSource_Class> Items { get; set; }
    public ObservableCollection<Settings_ItemSource_Class> IPAdresses { get; set; }

    public ViewModel()
    {
        Items = new ObservableCollection<LVItemSource_Class>();
        IPAdresses = new ObservableCollection<Settings_ItemSource_Class>();
    }

    public void AddItem(string parameter, bool Show_Gear, bool isChecked )
    {
        var visibility = Visibility.Visible;

        if (Show_Gear == true) visibility = Visibility.Visible;
        else visibility = Visibility.Hidden;
        Items.Add(new LVItemSource_Class { Parameter = parameter, Show_Gear = visibility, IsChecked = isChecked });
    }

    public void AddItem(string IPAddress, BitmapImage img)
    {
        IPAdresses.Add(new Settings_ItemSource_Class { IPAddress = IPAddress, ConnectionStatus = img });
    }

    public void SaveIPAdresses(string path)
    {
        File.WriteAllLines(path, IPAdresses.Select(item => item.IPAddress).ToList());
    }

    public void DeleteIPAddress(Settings_ItemSource_Class item)
    {
        if (IPAdresses.Contains(item))
        {
            IPAdresses.Remove(item);
        }
    }

    public async Task CheckIPAddressesAsync()
    {
        foreach (var ipAddress in IPAdresses)
        {
            bool isReachable = await IsIpAddressReachableAsync(ipAddress.IPAddress);
            ipAddress.ConnectionStatus = isReachable ? new BitmapImage(new Uri("pack://application:,,,/Assets/Images/greenPin.png")) : new BitmapImage(new Uri("pack://application:,,,/Assets/Images/grayPin.png"));
            OnPropertyChanged(nameof(IPAdresses));
        }
    }

    private async Task<bool> IsIpAddressReachableAsync(string ipAddress)
    {
        using (Ping ping = new Ping())
        {
            try
            {
                PingReply reply = await ping.SendPingAsync(ipAddress, 1000); // Тайм-аут 1 секунда
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}