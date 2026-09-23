using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

// System Configuration gate (MAYD-133, 2026-09-24): a system-wide singleton — at most one real row
// ever exists (enforced in SystemConfigurationService, not by a unique constraint on a constant,
// since EF/SQL Server has no clean "at most one row" table constraint). SmtpPasswordProtected is
// ciphertext (see ISecretProtector's own comment for why), never the raw password — nothing in this
// codebase should ever read/deserialize it except through ISecretProtector.Unprotect, and only the
// real SMTP-sending implementation (not yet built — LoggingEmailSender stays in place for now) will
// ever need to.
public class SystemConfiguration : SharedEntities
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPasswordProtected { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;
}
