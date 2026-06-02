using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace OmniExtract.App.Services;

public class LibreOfficeBridge
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<LibreOfficeBridge> _logger;

    public LibreOfficeBridge(ILogger<LibreOfficeBridge> logger)
    {
        _logger = logger;
    }

    public async Task<string?> ConvertToPdfAsync(string inputPath, CancellationToken ct = default)
    {
        var libreOfficePath = FindLibreOffice();
        if (libreOfficePath is null)
        {
            _logger.LogWarning("LibreOffice not found. Skipping conversion for {File}", Path.GetFileName(inputPath));
            return null;
        }

        var outDir = Path.Combine(Path.GetTempPath(), $"omniextract_{Guid.NewGuid():N}");
        Directory.CreateDirectory(outDir);

        await _lock.WaitAsync(ct);
        try
        {
            _logger.LogInformation("LibreOffice: converting {File}", Path.GetFileName(inputPath));

            var psi = new ProcessStartInfo
            {
                FileName = libreOfficePath,
                Arguments = $"--headless --convert-to pdf --outdir \"{outDir}\" \"{inputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start LibreOffice process");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

            await process.WaitForExitAsync(timeoutCts.Token);

            if (process.ExitCode != 0)
            {
                var err = await process.StandardError.ReadToEndAsync(ct);
                _logger.LogWarning("LibreOffice exited {Code}: {Err}", process.ExitCode, err);
                Directory.Delete(outDir, recursive: true);
                return null;
            }

            var pdfFile = Directory.GetFiles(outDir, "*.pdf").FirstOrDefault();
            if (pdfFile is null)
            {
                _logger.LogWarning("LibreOffice produced no PDF for {File}", Path.GetFileName(inputPath));
                Directory.Delete(outDir, recursive: true);
                return null;
            }

            _logger.LogInformation("LibreOffice: produced {Pdf}", Path.GetFileName(pdfFile));
            return pdfFile;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("LibreOffice conversion timed out for {File}", Path.GetFileName(inputPath));
            TryDeleteDir(outDir);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LibreOffice conversion error for {File}", Path.GetFileName(inputPath));
            TryDeleteDir(outDir);
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string? FindLibreOffice()
    {
        var candidates = new[]
        {
            "libreoffice",
            "/usr/bin/libreoffice",
            "/usr/local/bin/libreoffice",
            "/opt/libreoffice/program/soffice",
        };

        foreach (var candidate in candidates)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = candidate,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(3000);
                if (p?.ExitCode == 0) return candidate;
            }
            catch { }
        }

        return null;
    }

    private static void TryDeleteDir(string path)
    {
        try { Directory.Delete(path, recursive: true); } catch { }
    }
}
