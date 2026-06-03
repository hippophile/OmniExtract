using Microsoft.AspNetCore.Components.Forms;
using OmniExtract.App.Services;
using OmniExtract.Web.Models;

namespace OmniExtract.Web.Services;

public class ExtractionOrchestrator
{
    private readonly DocumentProcessor _documentProcessor;
    private readonly ExtractionService _extractionService;
    private readonly ResultsRepository _resultsRepository;
    private readonly TokenCounter _tokenCounter;
    private readonly ArchiveHandler _archiveHandler;
    private readonly ILogger<ExtractionOrchestrator> _logger;

    private readonly List<ProcessingJob> _jobs = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public event Action? StateChanged;
    public IReadOnlyList<ProcessingJob> Jobs => _jobs.AsReadOnly();

    public ExtractionOrchestrator(
        DocumentProcessor documentProcessor,
        ExtractionService extractionService,
        ResultsRepository resultsRepository,
        TokenCounter tokenCounter,
        ArchiveHandler archiveHandler,
        ILogger<ExtractionOrchestrator> logger)
    {
        _documentProcessor = documentProcessor;
        _extractionService = extractionService;
        _resultsRepository = resultsRepository;
        _tokenCounter = tokenCounter;
        _archiveHandler = archiveHandler;
        _logger = logger;
    }

    public async Task EnqueueAsync(IBrowserFile file, CancellationToken ct = default)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "omniextract-web", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, file.Name);

        await using (var fs = File.Create(tempPath))
        await using (var stream = file.OpenReadStream(maxAllowedSize: 100L * 1024 * 1024, ct))
            await stream.CopyToAsync(fs, ct);

        var job = new ProcessingJob
        {
            FileName = file.Name,
            FileSize = file.Size,
            TempPath = tempPath
        };

        _jobs.Insert(0, job);
        NotifyState();

        _ = Task.Run(() => ProcessJobAsync(job, CancellationToken.None), CancellationToken.None);
    }

    private async Task ProcessJobAsync(ProcessingJob job, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            job.Status = JobStatus.Processing;
            job.Stage = "Parsing file...";
            job.StageStartedAt = DateTime.UtcNow;
            job.StartedAt = DateTime.UtcNow;
            NotifyState();

            if (ArchiveHandler.IsArchive(job.TempPath))
            {
                await ProcessArchiveJobAsync(job, ct);
                return;
            }

            var extracted = await _documentProcessor.ExtractAsync(job.TempPath, ct);

            job.Stage = "Chunking document...";
            job.StageStartedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(extracted.Text))
            {
                job.TokenCount = _tokenCounter.CountTokens(extracted.Text);
                job.ChunkCount = Math.Max(1, (int)Math.Ceiling(extracted.Text.Length / 80_000.0));
            }
            NotifyState();

            job.Stage = "Running AI analysis...";
            job.StageStartedAt = DateTime.UtcNow;
            NotifyState();

            var result = await _extractionService.ExtractAsync(job.TempPath, extracted, ct);

            job.Result = result;
            job.Status = JobStatus.Done;
            job.Stage = "Complete";
            job.CompletedAt = DateTime.UtcNow;

            var entry = _resultsRepository.Add(job.FileName, result);
            job.ResultId = entry.Id;
            NotifyState();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process {File}", job.FileName);
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.ErrorDetail = ex.ToString();
            job.Stage = "Failed";
            job.CompletedAt = DateTime.UtcNow;
            var failedEntry = _resultsRepository.AddFailed(job.FileName, ex.Message, ex.ToString());
            job.ResultId = failedEntry.Id;
            NotifyState();
        }
        finally
        {
            _semaphore.Release();
            try
            {
                var dir = Path.GetDirectoryName(job.TempPath);
                if (dir != null && Directory.Exists(dir)) Directory.Delete(dir, true);
            }
            catch { }
        }
    }

    private async Task ProcessArchiveJobAsync(ProcessingJob job, CancellationToken ct)
    {
        var memberResults = new List<OmniExtract.Core.Models.UniversalOutput>();
        var memberIndex = 0;

        await _archiveHandler.ExtractAsync(job.TempPath, async (memberPath, innerCt) =>
        {
            memberIndex++;
            job.Stage = $"Processing file {memberIndex} of ?...";
            job.StageStartedAt = DateTime.UtcNow;
            NotifyState();

            var extracted = await _documentProcessor.ExtractAsync(memberPath, innerCt);
            var result = await _extractionService.ExtractAsync(memberPath, extracted, innerCt);
            memberResults.Add(result);
        }, ct);

        OmniExtract.Core.Models.UniversalOutput merged;
        if (memberResults.Count == 0)
        {
            merged = new OmniExtract.Core.Models.UniversalOutput
            {
                Meta = new OmniExtract.Core.Models.OutputMeta
                {
                    ExtractionMethod = "archive",
                    Warnings = ["ZIP contained no processable files"]
                }
            };
        }
        else if (memberResults.Count == 1)
        {
            merged = memberResults[0];
        }
        else
        {
            merged = ExtractionService.MergeResults(memberResults);
        }

        merged.Meta.SourceFile = job.FileName;

        job.Result = merged;
        job.Status = JobStatus.Done;
        job.Stage = "Complete";
        job.CompletedAt = DateTime.UtcNow;

        var entry = _resultsRepository.Add(job.FileName, merged);
        job.ResultId = entry.Id;
        NotifyState();
    }

    private void NotifyState() => StateChanged?.Invoke();
}
