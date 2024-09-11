using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using PingMonitor.csv;
using PingMonitor.dto;
using PingMonitor.model;
using Timer = System.Timers.Timer;

namespace PingMonitor.view;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowModel _model = new();
    private readonly Timer? _timer;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = _model;
        _timer = new Timer(5000);
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        await CheckOnlineStatusAsync();
    }

    private async Task CheckOnlineStatusAsync()
    {
        List<Device> devices = _model.Devices.ToList();
        foreach (Device device in devices)
        {
            try
            {
                using (Ping ping = new())
                {
                    string toCheck = string.IsNullOrWhiteSpace(device.IP) ? device.Host : device.IP;
                    PingReply reply = await ping.SendPingAsync(toCheck, 3000);

                    if (device.Status != reply.Status)
                    {
                        StringBuilder buf = new();
                        buf.Append("[").Append(DateTime.Now.ToString("HH:mm:ss")).Append("] ");
                        if (!string.IsNullOrWhiteSpace(device.Host))
                        {
                            buf.Append("'").Append(device.Host).Append("' ");
                        }

                        if (!string.IsNullOrWhiteSpace(device.IP)) { buf.Append("(").Append(device.IP).Append(") "); }

                        buf.Append("changed Status from '").Append(device.Status.ToString()).Append("'").Append(" to '")
                            .Append(reply.Status.ToString()).Append("' after ")
                            .Append((DateTime.Now - device.Time).ToString(@"hh\:mm\:ss"));
                        WriteToLog(buf.ToString());
                    }

                    device.Status = reply.Status;
                    device.TimeString = (DateTime.Now - device.Time).ToString(@"hh\:mm\:ss");
                }
            }
            catch
            {
                device.Status = IPStatus.Unknown;
                device.Message = "Error getting Status";
            }
            finally { device.TimeString = (DateTime.Now - device.Time).ToString(@"hh\:mm\:ss"); }
        }
    }

    private void LoadFile_Clicked(object sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new()
        {
            Filter = "CSV files (*.csv)|*.csv",
            InitialDirectory = @"C:\",
            Multiselect = false,
            CheckFileExists = true,
            CheckPathExists = true
        };

        if (ofd.ShowDialog() == true) { LoadDevices(ofd.FileName); }
    }

    private async void LoadDevices(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            WriteToLog($"'{filename}' is null or empty");
            return;
        }

        if (!File.Exists(filename))
        {
            WriteToLog($"'{filename}' does not exist");
            return;
        }

        using StreamReader reader = new(filename);
        CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            Delimiter = ","
        };
        using CsvReader csv = new(reader, csvConfig);
        csv.Context.RegisterClassMap<OptionalMapper>();
        IEnumerable<Device> records = csv.GetRecords<Device>();

        foreach (Device record in records)
        {
            string? hostname = record.Host;
            string? ipAddress = record.IP;

            if (string.IsNullOrWhiteSpace(hostname) && string.IsNullOrWhiteSpace(ipAddress))
            {
                WriteToLog("csv entry with both hostname and ip null or empty skipped");
                continue;
            }

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                try
                {
                    IPAddress[] hostAddressesAsync = await Dns.GetHostAddressesAsync(hostname);
                    if (hostAddressesAsync.Length > 0) { record.IP = hostAddressesAsync[0].ToString(); }
                    else { WriteToLog($"Could not resolve hostname '{hostname}' to an IP address"); }
                }
                catch { WriteToLog($"Could not resolve hostname '{hostname}' to an IP address"); }
            }

            _model.Devices.Add(record);
        }
    }

    private void WriteToLog(string message)
    {
        _model.Log += message + Environment.NewLine;
    }

    private async void Input_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (string.IsNullOrWhiteSpace(_model.Input))
            {
                WriteToLog("Input is null or empty");
                _model.Input = string.Empty;
                return;
            }

            Device temp = new();

            if (Regex.IsMatch(_model.Input, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}", RegexOptions.None))
            {
                temp.IP = _model.Input;
                _model.Devices.Add(temp);
                _model.Input = string.Empty;
                return;
            }

            temp.Host = _model.Input;
            _model.Input = string.Empty;

            try
            {
                IPAddress[] hostAddressesAsync = await Dns.GetHostAddressesAsync(temp.Host);
                if (hostAddressesAsync.Length > 0) { temp.IP = hostAddressesAsync[0].ToString(); }
                else { WriteToLog($"Could not resolve hostname '{temp.Host}' to an IP address"); }
            }
            catch { WriteToLog($"Could not resolve hostname '{temp.Host}' to an IP address"); }

            _model.Devices.Add(temp);

            e.Handled = true;
        }
    }

    private void DeviceList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            ListView listview = (ListView)sender;
            if (listview.SelectedItems != null)
            {
                List<Device> itemsToRemove = listview.SelectedItems.Cast<Device>().ToList();
                foreach (Device item in itemsToRemove) { _model.Devices.Remove(item); }
            }
        }
    }

    private void SaveFile_Clicked(object sender, RoutedEventArgs e)
    {
        SaveFileDialog sfd = new()
        {
            Filter = "CSV files (*.csv)|*.csv",
            InitialDirectory = @"C:\",
            CheckPathExists = true
        };

        if (sfd.ShowDialog() == true) { SaveDevices(sfd.FileName); }
    }

    private void SaveDevices(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            WriteToLog($"'{filename}' is null or empty");
            return;
        }

        using StreamWriter writer = new(filename);
        CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            Delimiter = ","
        };
        using CsvWriter csv = new(writer, csvConfig);
        csv.WriteRecords(_model.Devices);
    }
}