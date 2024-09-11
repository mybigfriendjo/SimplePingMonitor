using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PingMonitor.dto;

public class Device : ObservableObject
{
    private string? _host;
    private string? _ip;
    private string? _message;
    private IPStatus _status = IPStatus.Unknown;
    private string? _statusString;
    private DateTime _time = DateTime.Now;
    private string? _timestring;

    public string? Message
    {
        get { return _message; }
        set { SetProperty(ref _message, value); }
    }

    public IPStatus Status
    {
        get { return _status; }
        set
        {
            SetProperty(ref _status, value);
            StatusString = _status.ToString();
        }
    }

    public string? StatusString
    {
        get { return _statusString; }
        set { SetProperty(ref _statusString, value); }
    }

    public string? Host
    {
        get { return _host; }
        set { SetProperty(ref _host, value); }
    }

    public string? IP
    {
        get { return _ip; }
        set { SetProperty(ref _ip, value); }
    }

    public DateTime Time
    {
        get { return _time; }
        set { SetProperty(ref _time, value); }
    }

    public string? TimeString
    {
        get { return _timestring; }
        set { SetProperty(ref _timestring, value); }
    }
}