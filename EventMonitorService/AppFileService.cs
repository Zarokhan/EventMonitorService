using System.Reflection;
using System.Text.Json;

namespace EventMonitorService;

public static class AppFileService
{
    public static string GetAppFolder()
    {
        var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EventMonitorService");

        if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);

        return appFolder;
    }
    
    private static string GetConfigPath()
    {
        return Path.Combine(GetAppFolder(), "config.json");
    }

    private static string GetEmailTemplatePath()
    {
        return Path.Combine(GetAppFolder(), "EmailTemplate.html");
    }

    public static AppConfig LoadOrCreateConfig()
    {
        var configPath = GetConfigPath();

        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        
        // Create a new config if not found
        var defaultConfig = new AppConfig();
        SaveConfig(defaultConfig);
        return defaultConfig;
    }

    public static string GetOrCreateEmailTemplate()
    {
        // Check if the file exists in AppData
        if (File.Exists(GetEmailTemplatePath()))
        {
            return File.ReadAllText(GetEmailTemplatePath());
        }

        // Load embedded resource as a fallback
        var template = LoadEmbeddedTemplate();

        // Save the template to AppData
        File.WriteAllText(GetEmailTemplatePath(), template);

        return template;
    }
    
    private static string LoadEmbeddedTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "EventMonitorService.EmailTemplate.html"; // Update with actual namespace & resource name

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException("Embedded email template not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static void SaveConfig(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonSerializerOptions());
        File.WriteAllText(GetConfigPath(), json);
    }

    private static JsonSerializerOptions JsonSerializerOptions()
    {
        return new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
    }
}