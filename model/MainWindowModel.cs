using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PingMonitor.dto;

namespace PingMonitor.model;

public class MainWindowModel : ObservableObject
{
    private ObservableCollection<Device> _devices = new();
    private string? _input;

    private string? _log;

    public string? Input
    {
        get { return _input; }
        set { SetProperty(ref _input, value); }
    }

    public string? Log
    {
        get { return _log; }
        set { SetProperty(ref _log, value); }
    }

    public ObservableCollection<Device> Devices
    {
        get { return _devices; }
        set { SetProperty(ref _devices, value); }
    }
}