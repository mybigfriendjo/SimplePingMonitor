using System.Globalization;
using CsvHelper.Configuration;
using PingMonitor.dto;

namespace PingMonitor.csv;

public sealed class OptionalMapper : ClassMap<Device>
{
    public OptionalMapper()
    {
        Map(m => m.Host).Name("Host").Optional();
        Map(m => m.IP).Name("IP").Optional();

        /*
        Map(m => m.Message).Name("Message").Optional();
        Map(m => m.Status).Name("Status").Optional();
        Map(m => m.Time).Name("Time").Optional();
        //*/

        //*
        Map(m => m.Message).Name("Message").Ignore();
        Map(m => m.Status).Name("Status").Ignore();
        Map(m => m.StatusString).Name("StatusString").Ignore();
        Map(m => m.Time).Name("Time").Ignore();
        Map(m => m.TimeString).Name("TimeString").Ignore();
        //*/
    }
}