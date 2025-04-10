using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace EventMonitorService;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly EmailService _emailService;

    public Worker(ILogger<Worker> logger, EmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        AppFileService.LoadOrCreateConfig();
        AppFileService.GetOrCreateEmailTemplate();
        _logger.LogInformation("Worker directory: " + AppFileService.GetAppFolder());
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await ReadEvents(stoppingToken);
            var appConfig = AppFileService.LoadOrCreateConfig();
            await Task.Delay(1000 * appConfig.CycleTimeSeconds, stoppingToken);
        }
    }

    private async Task ReadEvents(CancellationToken stoppingToken)
    {
        var logName = "Application";  // Change to Application "System", "Security", or custom log name

        // Check if the log exists
        if (!EventLog.Exists(logName))
        {
            _logger.LogInformation($"Log '{logName}' does not exist.");
            return;
        }

        var eventLog = new EventLog(logName);
        var appConfig = AppFileService.LoadOrCreateConfig();
        var retentionDate = DateTime.Now.AddDays(-appConfig.RetentionDays);
        await using var dbContext = new AppDbContext();

        var addList = new List<AppDbEventInstance>();
        
        var totalEntries = eventLog.Entries.Count;
        
        for (var i = totalEntries - 1; i >= 0; i--)
        {
            var entry = eventLog.Entries[i];
            
            if (entry.TimeGenerated < retentionDate) continue;

            var monitorEvent = appConfig.MonitorEventsList.FirstOrDefault(q => entry.Source != ".NET Runtime" ? q.Application == entry.Source : q.Application == ExtractApplicationName(entry.Message));
            if (monitorEvent == null) continue;
            
            if ((int)entry.EntryType > monitorEvent.Severity) continue;
            if (dbContext.WinEvents.Any(q => q.Id == entry.Index)) continue;


#if DEBUG
            Console.WriteLine($"[{entry.TimeGenerated}] {entry.EntryType}: {entry.Source} - {entry.Message}");
#endif

            addList.Add(new AppDbEventInstance()
            {
                Id = entry.Index,
                Created = entry.TimeGenerated,
                Message = entry.Message,
                Source = entry.Source != ".NET Runtime" ? entry.Source : ExtractApplicationName(entry.Message),
                Severity = entry.EntryType.ToString(),
                Computer = entry.MachineName
            });
        }

        if (addList.Count == 0) return;

        addList = addList.DistinctBy(q => q.Id).ToList();
        
        await SendEmail(addList, stoppingToken);
        
        await dbContext.WinEvents.AddRangeAsync(addList, stoppingToken);
        await dbContext.SaveChangesAsync(stoppingToken);
        
        // clean up old entries
        var oldEvents = dbContext.WinEvents.Where(e => e.Created < retentionDate);
        dbContext.WinEvents.RemoveRange(oldEvents);
        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task SendEmail(List<AppDbEventInstance> logs, CancellationToken cancellationToken)
    {
        // group by source and message and get list of timeframe for group min to max date
        var template = AppFileService.GetOrCreateEmailTemplate();
        var content = "";
        var groups = logs.GroupBy(q => (q.Source + q.Message), q => q, (key, enumerable) => enumerable.ToList()).ToList();

        foreach (var group in groups)
        {
            var entry = group.First();
            content += $"<h1>{entry.Source}</h1>";
            content += $"<p>Severity: {entry.Severity}</p>";
            if (group.Count > 1)
            {
                content += $"<p>Count: {group.Count}</p>";
                content += $"<p>Date range: {group.MinBy(q => q.Created)?.Created:g} - {group.MaxBy(q => q.Created)?.Created:g}</p>";
            }
            else
            {
                content += $"<p>Date: {entry.Created:g}</p>";
            }

            content += $"<p>Message: \n{entry.Message?.Replace("\n", "</br>")}</p>";
        }

        var machineName = groups.FirstOrDefault()?.FirstOrDefault()?.Computer;
        
        template = template.Replace("{{content}}", content);
        template = template.Replace("{{footer}}", machineName ?? "Look at this cool project.");
        
        await _emailService.SendEmailAsync("Event Monitor Alert", template, cancellationToken);
    }
    
    public static string ExtractApplicationName(string log)
    {
        // Match application name from "Application: XYZ.exe"
        var exeMatch = Regex.Match(log, @"Application:\s+([\w\d]+)\.exe");
        if (exeMatch.Success)
        {
            return exeMatch.Groups[1].Value;
        }

        // Match category name from "Category: XYZ"
        var categoryMatch = Regex.Match(log, @"Category:\s+([\w\d\.]+)");
        if (categoryMatch.Success)
        {
            return categoryMatch.Groups[1].Value;
        }

        return "Unknown";
    }
}