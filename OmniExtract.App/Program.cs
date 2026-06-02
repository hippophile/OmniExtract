using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmniExtract.App.Services;
using OmniExtract.Core.Config;

if (args.Length == 0)
{
    PrintUsage();
    return 0;
}

// Build host
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<OpenAISettings>(ctx.Configuration.GetSection("OpenAI"));
        services.Configure<ProcessingSettings>(ctx.Configuration.GetSection("Processing"));
        services.Configure<PathSettings>(ctx.Configuration.GetSection("Paths"));
        services.AddSingleton<GptClient>();
        services.AddSingleton<TokenCounter>();
        services.AddSingleton<LibreOfficeBridge>();
        services.AddSingleton<DocumentProcessor>();
        services.AddSingleton<ArchiveHandler>();
        services.AddSingleton<ExtractionService>();
        services.AddSingleton<OutputWriter>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

var processor = host.Services.GetRequiredService<DocumentProcessor>();
var archiveHandler = host.Services.GetRequiredService<ArchiveHandler>();
var extraction = host.Services.GetRequiredService<ExtractionService>();
var writer = host.Services.GetRequiredService<OutputWriter>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

// Parse arguments
var watchMode = args.Contains("--watch");
var paths = args.Where(a => !a.StartsWith("--")).ToArray();

if (paths.Length == 0)
{
    Console.Error.WriteLine("ERROR: No path provided.");
    PrintUsage();
    return 1;
}

var target = paths[0];

if (!File.Exists(target) && !Directory.Exists(target))
{
    Console.Error.WriteLine($"ERROR: Path does not exist: {target}");
    return 1;
}

if (watchMode)
{
    if (!Directory.Exists(target))
    {
        Console.Error.WriteLine($"ERROR: Watch mode requires a directory path.");
        return 1;
    }
    await RunWatchMode(target, cts.Token);
}
else if (Directory.Exists(target))
{
    await RunFolderMode(target, cts.Token);
}
else
{
    await ProcessFile(target, cts.Token);
}

return 0;

async Task ProcessFile(string filePath, CancellationToken ct)
{
    if (Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
        return;

    logger.LogInformation("Processing: {File}", Path.GetFileName(filePath));

    if (ArchiveHandler.IsArchive(filePath))
    {
        await archiveHandler.ExtractAsync(filePath, ProcessFile, ct);
        return;
    }

    var extracted = await processor.ExtractAsync(filePath, ct);
    var output = await extraction.ExtractAsync(filePath, extracted, ct);
    await writer.WriteAsync(filePath, output, ct);
}

async Task RunFolderMode(string dir, CancellationToken ct)
{
    logger.LogInformation("Folder mode: {Dir}", dir);
    var files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
        .Where(f => !Path.GetExtension(f).Equals(".json", StringComparison.OrdinalIgnoreCase))
        .Where(f => !File.Exists(f + ".json"))
        .ToList();

    logger.LogInformation("Found {Count} unprocessed files", files.Count);
    foreach (var file in files)
    {
        if (ct.IsCancellationRequested) break;
        await ProcessFile(file, ct);
    }
    logger.LogInformation("Folder processing complete.");
}

async Task RunWatchMode(string dir, CancellationToken ct)
{
    logger.LogInformation("Watch mode starting on: {Dir}", dir);

    // Process existing files first
    await RunFolderMode(dir, ct);

    logger.LogInformation("Watching for new files. Press Ctrl+C to stop.");

    var tcs = new TaskCompletionSource();
    ct.Register(() => tcs.TrySetResult());

    using var watcher = new FileSystemWatcher(dir)
    {
        IncludeSubdirectories = true,
        EnableRaisingEvents = true,
        NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
    };

    watcher.Created += async (_, e) =>
    {
        if (ct.IsCancellationRequested) return;
        if (Path.GetExtension(e.FullPath).Equals(".json", StringComparison.OrdinalIgnoreCase)) return;

        // Brief wait for file to finish writing
        await Task.Delay(500, ct);
        try { await ProcessFile(e.FullPath, ct); }
        catch (Exception ex) { logger.LogError(ex, "Watch mode processing error for {File}", e.Name); }
    };

    await tcs.Task;
    logger.LogInformation("Watch mode stopped.");
}

static void PrintUsage()
{
    Console.WriteLine("""
        OmniExtract — Universal Document Extraction Engine

        Usage:
          omniextract <file>            Process a single file
          omniextract <folder>          Process all files in a folder recursively
          omniextract --watch <folder>  Watch a folder and process new files automatically

        Output:
          Each file produces a JSON file alongside the original (e.g. report.pdf → report.pdf.json)

        Requirements:
          GITHUB_TOKEN environment variable must be set (used for GPT-4.1 via GitHub Models)
          LibreOffice (optional): sudo apt install libreoffice — enables legacy format support

        Examples:
          omniextract ~/documents/invoice.pdf
          omniextract ~/documents/
          omniextract --watch ~/inbox/
        """);
}
