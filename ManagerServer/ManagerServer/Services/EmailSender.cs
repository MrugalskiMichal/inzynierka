using System.Net;
using System.Net.Mail;

public class EmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAlertEmail(string to, string subject, string body)
    {
        var smtpHost = _config["Email:SmtpHost"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"]);
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];
        var from = _config["Email:From"];

        var message = new MailMessage(from, to, subject, body);

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        try
        {
            await client.SendMailAsync(message);
            Console.WriteLine("Mail sent successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending: {ex.Message}");
        }
    }
    public async Task SendEmail(string to, string subject, string body, byte[]? attachment = null)
    {
        var smtpHost = _config["Email:SmtpHost"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"]);
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];
        var from = _config["Email:From"];

        var message = new MailMessage(from, to, subject, body);

        if (attachment != null)
        {
            message.Attachments.Add(new Attachment(new MemoryStream(attachment), "report.pdf"));
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        try
        {
            await client.SendMailAsync(message);
            Console.WriteLine("Mail sent successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending : {ex.Message}");
        }
    }
}
