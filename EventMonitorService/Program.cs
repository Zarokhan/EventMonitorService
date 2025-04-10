using EventMonitorService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "EventMonitorService";
});
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<EmailService>();

var host = builder.Build();

using (var context = new AppDbContext())
{
    context.Database.Migrate();
}

host.Run();