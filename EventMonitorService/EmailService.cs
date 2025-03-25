using System.Net;
using System.Net.Mail;

namespace EventMonitorService;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }
    
    public async Task SendEmailAsync(string subject, string body, CancellationToken cancellationToken)
    {
        var appConfig = AppFileService.LoadOrCreateConfig();

        if (appConfig.SmtpEmail == null)
        {
            _logger.LogError("SmtpEmail is null in app config");
            return;
        }
        
        if (appConfig.SmtpEmail.SmtpUsername == null)
        {
            _logger.LogError("SmtpUsername is null in app config");
            return;
        }
        
        if (appConfig.AlertRecipientEmail == null)
        {
            _logger.LogError("AlertRecipientEmail is null in app config");
            return;
        }
        
        using var smtp = new SmtpClient(appConfig.SmtpEmail.SmtpServer, appConfig.SmtpEmail.SmtpPort ?? 465);
        smtp.Credentials = new NetworkCredential(appConfig.SmtpEmail.SmtpUsername, appConfig.SmtpEmail.SmtpPassword);
        smtp.EnableSsl = true; // Set to false if SSL is not required

        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(appConfig.SmtpEmail.SmtpUsername);
        mailMessage.Subject = subject;
        mailMessage.Body = body;
        mailMessage.IsBodyHtml = true;

        mailMessage.To.Add(appConfig.AlertRecipientEmail);

        await smtp.SendMailAsync(mailMessage, cancellationToken);
        _logger.LogInformation("Email sent successfully.");
    }
}