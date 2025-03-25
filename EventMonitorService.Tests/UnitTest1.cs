using System.Diagnostics;

namespace EventMonitorService.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var logName = "Application";  // Change to Application "System", "Security", or custom log name
        var eventLog = new EventLog(logName);
        foreach (EventLogEntry entry in eventLog.Entries)
        {
            if (entry.Source != ".NET Runtime") continue;

            var test = "test";
        }
    }
}