namespace Maydan.Application.DTOs.SystemConfiguration;

// System Configuration gate (MAYD-133, 2026-09-24): prefill/read shape for the admin screen.
// Deliberately never carries the raw SMTP password/secret — only HasSmtpPassword, so the frontend
// can show "a password is already set" without ever seeing or re-transmitting the real value.
public record SystemConfigurationDto(
    bool IsConfigured,
    string SmtpHost,
    int SmtpPort,
    string SmtpUsername,
    bool HasSmtpPassword,
    string SenderEmail,
    string SenderDisplayName);

// SmtpPassword is nullable/optional on purpose: omitting it on an update means "keep the
// currently-stored password" (see SystemConfigurationService.UpdateAsync) — required only the
// very first time (no password has ever been set yet).
public class UpdateSystemConfigurationDto
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string? SmtpPassword { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;
}
