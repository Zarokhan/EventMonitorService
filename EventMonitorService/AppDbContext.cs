namespace EventMonitorService;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<AppDbEventInstance> WinEvents { get; set; } // The table for the User model

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = Path.Combine(AppFileService.GetAppFolder(), "eventmonitorservice.db");
        options.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppDbEventInstance>()
            .Property(q => q.Id)
            .ValueGeneratedNever();
        
        // Create an index on the Created property
        modelBuilder.Entity<AppDbEventInstance>()
            .HasIndex(e => e.Created)
            .HasDatabaseName("IX_AppDbEventInstance_Created");  // Optional: Specify the index name
    }
}