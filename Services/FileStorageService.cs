using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace TechMoveSystems.Services;

public interface IFileStorageService
{
    bool IsValidPdf(IFormFile? file);
    Task<string> SaveAgreementAsync(IFormFile file, CancellationToken cancellationToken = default);
    string GetAbsolutePath(string relativePath);
}

public class FileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf" };

    public bool IsValidPdf(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        return AllowedExtensions.Contains(extension);
    }

    public async Task<string> SaveAgreementAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (!IsValidPdf(file))
        {
            throw new InvalidOperationException("Only non-empty PDF files can be uploaded.");
        }

        var uploadFolder = Path.Combine(environment.WebRootPath, "uploads", "agreements");
        Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid():N}.pdf";
        var absolutePath = Path.Combine(uploadFolder, fileName);

        await using var stream = File.Create(absolutePath);
        await file.CopyToAsync(stream, cancellationToken);

        return Path.Combine("uploads", "agreements", fileName).Replace("\\", "/");
    }

    public string GetAbsolutePath(string relativePath)
    {
        var sanitizedPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
        return Path.Combine(environment.WebRootPath, sanitizedPath);
    }
}
