using Maydan.Application.Interfaces;

namespace Maydan.Infrastructure.Storage;

// Simple local-disk storage — the only implementation this ticket needed, since no other
// file-upload feature existed to reuse. uploadsRootPath is resolved in Program.cs (where
// IWebHostEnvironment.WebRootPath is naturally available) and passed in as a plain string so this
// class, like the rest of Maydan.Infrastructure, stays free of any ASP.NET Core hosting reference.
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsRootPath;

    public LocalFileStorageService(string uploadsRootPath)
    {
        _uploadsRootPath = uploadsRootPath;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string subfolder,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";

        var folderPath = Path.Combine(_uploadsRootPath, subfolder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        // Web-relative, forward-slash path regardless of host OS — matches how
        // app.UseStaticFiles() serves wwwroot content and how the frontend would request it back.
        return $"/uploads/{subfolder}/{fileName}".Replace('\\', '/');
    }
}
