using Maydan.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Maydan.Infrastructure.Email;

// TEMPORARY (2026-09-23): no real email provider is configured yet — Yousef hasn't decided which
// one to use (SendGrid/SMTP/etc.). Logs the would-be email instead of sending it, so the real
// forgot-password flow (AuthService.ForgotPasswordAsync) is fully exercisable end-to-end today —
// during local dev/testing, the reset link is read straight out of this log line. Swap the
// Program.cs registration (`AddScoped<IEmailSender, ...>`) for a real implementation once a
// provider is chosen; nothing else in the codebase depends on this class directly, only on
// IEmailSender.
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[TEMPORARY EMAIL PLACEHOLDER — no real provider configured] To: {ToEmail} | Subject: {Subject}\n{Body}",
            toEmail, subject, bodyHtml);

        return Task.CompletedTask;
    }
}
