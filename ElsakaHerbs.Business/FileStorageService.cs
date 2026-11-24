using ElsakaHerbs.interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ElsakaHerbs.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream stream, string originalFileName, string folder)
    {
        var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var targetDir = Path.Combine(webRoot, "uploads", folder);

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var extension = Path.GetExtension(originalFileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(targetDir, uniqueFileName);

        await using var fileStream = new FileStream(physicalPath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        return $"/uploads/{folder}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return Task.CompletedTask;

        try
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var cleanRelative = relativeUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(webRoot, cleanRelative);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                _logger.LogInformation("Deleted physical file: {Path}", physicalPath);
            }
        }
        catch (Exception ex)
        {
            // Database-first, disk-second: deletion errors are logged, not thrown to caller (Rule 6.4)
            _logger.LogError(ex, "Failed to delete physical file: {RelativeUrl}", relativeUrl);
        }

        return Task.CompletedTask;
    }
}
