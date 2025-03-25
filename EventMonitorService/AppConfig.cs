namespace EventMonitorService;

public class AppConfig
{
    public int RetentionDays { get; set; } = 90;  // How many days look back and save events
    public int CycleTimeSeconds { get; set; } = 10; // Loop wait time

    public List<MonitorEvents> MonitorEventsList { get; set; } =
    [
        new MonitorEvents()
        {
            Application = "MyApplication",
            Severity = 2,
        },

        new MonitorEvents()
        {
            Application = "MySecondApplication",
            Severity = 3,
        }
    ];

    public string? AlertRecipientEmail { get; set; } = "alert-recipient-email@yourdomain.com";
    
    public SmtpEmail? SmtpEmail { get; set; } = new();
}

public class MonitorEvents
{
    public string Application { get; set; } = "MyApplication";
    public int Severity { get; set; } = 3; // 1 = Critical, 2 = Error, 3 = Warning, 4 = Information, 5 = Verbose
}

public class SmtpEmail
{
    public int? SmtpPort { get; set; } = 587; // Use 465 for SSL
    public string? SmtpServer { get; set; } = "server.yourdomain.com";
    public string? SmtpUsername { get; set; }= "yourname@yourdomain.com";
    public string? SmtpPassword { get; set; } = "your-email-password";
}