using System.IO.Compression;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace OmniExtract.App.Services;

public class ArchiveHandler
{
    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".tar", ".gz", ".tgz", ".tar.gz", ".bz2", ".tar.bz2", ".7z", ".rar"
    };

    private readonly ILogger<ArchiveHandler> _logger;

    public ArchiveHandler(ILogger<ArchiveHandler> logger)
    {
        _logger = logger;
    }

    public static bool IsArchive(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ArchiveExtensions.Contains(ext)) return true;
        var name = Path.GetFileName(filePath).ToLowerInvariant();
        return name.EndsWith(".tar.gz") || name.EndsWith(".tar.bz2");
    }

    public async Task<List<string>> ExtractAsync(string archivePath, Func<string, CancellationToken, Task> processFile, CancellationToken ct = default)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"omniextract_arc_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var processed = new List<string>();

        try
        {
            Extract(archivePath, tempDir);
            await ProcessDirectory(tempDir, processFile, processed, ct);
        }
        finally
        {
            TryDeleteDir(tempDir);
        }

        return processed;
    }

    private void Extract(string archivePath, string outDir)
    {
        var ext = Path.GetExtension(archivePath).ToLowerInvariant();

        if (ext == ".zip")
        {
            _logger.LogInformation("Archive: extracting ZIP {File}", Path.GetFileName(archivePath));
            ZipFile.ExtractToDirectory(archivePath, outDir, overwriteFiles: true);
            return;
        }

        _logger.LogInformation("Archive: extracting {File} via SharpCompress", Path.GetFileName(archivePath));
        using var archive = ArchiveFactory.Open(archivePath);
        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            entry.WriteToDirectory(outDir, new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = true,
            });
        }
    }

    private async Task ProcessDirectory(string dir, Func<string, CancellationToken, Task> processFile, List<string> processed, CancellationToken ct)
    {
        foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
        {
            if (ct.IsCancellationRequested) break;
            if (Path.GetExtension(file).Equals(".json", StringComparison.OrdinalIgnoreCase)) continue;

            if (IsArchive(file))
            {
                // Recurse into nested archives
                var innerProcessed = await ExtractAsync(file, processFile, ct);
                processed.AddRange(innerProcessed);
            }
            else
            {
                await processFile(file, ct);
                processed.Add(file);
            }
        }
    }

    private static void TryDeleteDir(string path)
    {
        try { Directory.Delete(path, recursive: true); } catch { }
    }
}
