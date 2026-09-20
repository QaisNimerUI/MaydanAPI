namespace Maydan.Application.Interfaces;

// Projects audit: no file-upload feature existed anywhere in this codebase before this ticket
// (grepped the whole backend for IFormFile/IFileStorage — the only prior hit was
// CreateProjectRequest.WorkPermitImage itself, unused since ProjectsController was fully
// commented out). Stream-based rather than IFormFile-based deliberately: Maydan.Application is a
// plain class library with no ASP.NET Core reference (unlike Maydan.API, which is
// Sdk="Microsoft.NET.Sdk.Web"), so this interface stays framework-agnostic and the one caller that
// has an IFormFile (ProjectsController) opens its own read stream before calling in.
public interface IFileStorageService
{
    // Saves content under a named subfolder (e.g. "work-permits") and returns a web-relative path
    // (e.g. "/uploads/work-permits/<generated-name><extension>") suitable for persisting on an
    // entity and serving back via static file hosting (see Program.cs's app.UseStaticFiles()).
    Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string subfolder,
        CancellationToken cancellationToken = default);
}
