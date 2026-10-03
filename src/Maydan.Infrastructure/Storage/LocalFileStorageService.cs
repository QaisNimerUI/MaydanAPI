using Maydan.Application.Interfaces;

namespace Maydan.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(string rootPath)
    {
        _rootPath = rootPath;
    }

    public async Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string subfolder,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var relativeSubfolder = subfolder.Replace('\\', '/').Trim('/');
        var targetDirectory = Path.Combine(_rootPath, relativeSubfolder);

        Directory.CreateDirectory(targetDirectory);

        var targetPath = Path.Combine(targetDirectory, safeFileName);
        await using var output = File.Create(targetPath);
        await stream.CopyToAsync(output, cancellationToken);

        return $"/uploads/{relativeSubfolder}/{safeFileName}";
    }
}
