using System.Text.Json;
using Microsoft.Extensions.Logging;
using OmniExtract.Core.Models;

namespace OmniExtract.App.Services;

public class OutputWriter
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
    private readonly ILogger<OutputWriter> _logger;

    public OutputWriter(ILogger<OutputWriter> logger)
    {
        _logger = logger;
    }

    public async Task WriteAsync(string sourcePath, UniversalOutput output, CancellationToken ct = default)
    {
        var outputPath = sourcePath + ".json";
        var exists = File.Exists(outputPath);

        var json = JsonSerializer.Serialize(output, JsonOpts);
        await File.WriteAllTextAsync(outputPath, json, ct);

        if (exists)
            _logger.LogInformation("Overwritten existing output: {Path}", outputPath);
        else
            _logger.LogInformation("Written: {Path}", outputPath);
    }
}
