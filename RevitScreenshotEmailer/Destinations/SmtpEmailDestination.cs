using System;
using System.Net;
using System.Net.Mail;
using RevitScreenshotEmailer.Capture;

namespace RevitScreenshotEmailer.Destinations;

public sealed class SmtpEmailDestination : IScreenshotDestination
{
    private readonly EmailSettings _settings;

    public SmtpEmailDestination(EmailSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public string DisplayName => _settings.Recipient!;

    public void Send(Screenshot screenshot)
    {
        using var client = CreateClient();
        using var message = ComposeMessage(screenshot);
        client.Send(message);
    }

    private SmtpClient CreateClient()
    {
#pragma warning disable SYSLIB0014 // SmtpClient is the lightest dependency-free choice for a Revit addin.
        return new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            Timeout = 30_000,
        };
#pragma warning restore SYSLIB0014
    }

    private MailMessage ComposeMessage(Screenshot screenshot)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress!, _settings.FromName ?? "Revit Screenshot Emailer"),
            Subject = $"Revit screenshot — {screenshot.DocumentTitle} — {screenshot.ViewName}",
            Body =
                $"Screenshot of view \"{screenshot.ViewName}\" from \"{screenshot.DocumentTitle}\", " +
                $"captured {screenshot.CapturedAt:yyyy-MM-dd HH:mm:ss}.",
            IsBodyHtml = false,
        };
        message.To.Add(_settings.Recipient!);
        message.Attachments.Add(new Attachment(screenshot.FilePath));
        return message;
    }
}
