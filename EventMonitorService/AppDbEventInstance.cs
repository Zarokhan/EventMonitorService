using System.ComponentModel.DataAnnotations.Schema;

namespace EventMonitorService;

public class AppDbEventInstance
{
    public long Id { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    
    [NotMapped]
    public string? Message { get; set; }
    [NotMapped]
    public string? Source { get; set; }
    [NotMapped]
    public string? Severity { get; set; }
    [NotMapped]
    public string? Computer { get; set; }
}