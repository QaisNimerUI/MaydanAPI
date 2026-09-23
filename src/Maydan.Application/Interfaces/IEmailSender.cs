namespace Maydan.Application.Interfaces;

// Forgot-password recovery (2026-09-23): deliberately minimal/generic — recipient, subject, HTML
// body, nothing provider-specific (no attachments/templates/from-address config) since Yousef
// hasn't chosen a real provider yet (SendGrid/SMTP/etc.). A real implementation is a small,
// isolated follow-up: register it in Program.cs in place of the current TEMPORARY
// LoggingEmailSender (Maydan.Infrastructure/Email/LoggingEmailSender.cs) — no caller of this
// interface needs to change.
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}
