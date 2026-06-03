using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using OmniExtract.App.Services;
using OmniExtract.Core.Models;
using OmniExtract.Web.Models;

namespace OmniExtract.Web.Services;

public class ExtractionOrchestrator
{
    private readonly DocumentProcessor _documentProcessor;
    private readonly ExtractionService _extractionService;
    private readonly ResultsRepository _resultsRepository;
    private readonly TokenCounter _tokenCounter;
    private readonly ArchiveHandler _archiveHandler;
    private readonly IWebHostEnvironment _env;
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
        IWebHostEnvironment env,
        ILogger<ExtractionOrchestrator> logger)
    {
        _documentProcessor = documentProcessor;
        _extractionService = extractionService;
        _resultsRepository = resultsRepository;
        _tokenCounter = tokenCounter;
        _archiveHandler = archiveHandler;
        _env = env;
        _logger = logger;
    }

    public async Task EnqueueAsync(IBrowserFile file, AnalysisMode mode = AnalysisMode.Standard, CancellationToken ct = default)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "omniextract-web", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, file.Name);

        await using (var fs = File.Create(tempPath))
        await using (var stream = file.OpenReadStream(maxAllowedSize: 100L * 1024 * 1024, ct))
            await stream.CopyToAsync(fs, ct);

        // Save a persistent copy so the file can be opened from the result detail page
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var ext = Path.GetExtension(file.Name);
        var savedName = Guid.NewGuid().ToString("N") + ext;
        var savedPath = Path.Combine(uploadsDir, savedName);
        File.Copy(tempPath, savedPath);

        var job = new ProcessingJob
        {
            FileName = file.Name,
            FileSize = file.Size,
            TempPath = tempPath,
            UploadedFileUrl = $"/uploads/{savedName}",
            AnalysisMode = mode,
            Cts = new CancellationTokenSource()
        };

        _jobs.Insert(0, job);
        NotifyState();

        _ = Task.Run(() => ProcessJobAsync(job, job.Cts.Token), CancellationToken.None);
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

            var result = await _extractionService.ExtractAsync(job.TempPath, extracted, job.AnalysisMode, ct);

            job.Stage = "Agent recommendation...";
            job.StageStartedAt = DateTime.UtcNow;
            NotifyState();

            // Brief pause to let the rate-limit window reset after extraction
            await Task.Delay(TimeSpan.FromSeconds(5), ct);

            var recommendation = await _extractionService.RecommendationPassAsync(result, ct);
            if (recommendation is not null)
                result.Data["agent_recommendation"] = recommendation;
            else
                result.Meta.Warnings.Add("Agent recommendation pass failed — result saved without recommendation.");

            job.Result = result;
            job.Status = JobStatus.Done;
            job.Stage = "Complete";
            job.CompletedAt = DateTime.UtcNow;

            var entry = _resultsRepository.Add(job.FileName, result, job.UploadedFileUrl);
            job.ResultId = entry.Id;
            NotifyState();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Job {File} was cancelled", job.FileName);
            job.Status = JobStatus.Cancelled;
            job.Stage = "Cancelled";
            job.CompletedAt = DateTime.UtcNow;
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
            var memberName = Path.GetFileName(memberPath);
            job.Stage = $"Archive: {memberName} ({memberIndex})";
            job.StageStartedAt = DateTime.UtcNow;
            NotifyState();

            try
            {
                var extracted = await _documentProcessor.ExtractAsync(memberPath, innerCt);
                var result = await _extractionService.ExtractAsync(memberPath, extracted, job.AnalysisMode, innerCt);
                memberResults.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Archive member failed: {File}", memberName);
                memberResults.Add(new OmniExtract.Core.Models.UniversalOutput
                {
                    Meta = new OmniExtract.Core.Models.OutputMeta
                    {
                        SourceFile = memberName,
                        ExtractionMethod = "failed",
                        Warnings = [$"Member extraction failed: {ex.Message}"]
                    }
                });
            }
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

        await Task.Delay(TimeSpan.FromSeconds(5), ct);
        var archiveRec = await _extractionService.RecommendationPassAsync(merged, ct);
        if (archiveRec is not null)
            merged.Data["agent_recommendation"] = archiveRec;
        else
            merged.Meta.Warnings.Add("Agent recommendation pass failed — result saved without recommendation.");

        job.Result = merged;
        job.Status = JobStatus.Done;
        job.Stage = "Complete";
        job.CompletedAt = DateTime.UtcNow;

        var entry = _resultsRepository.Add(job.FileName, merged, job.UploadedFileUrl);
        job.ResultId = entry.Id;
        NotifyState();
    }

    public void CancelJobAsync(string id)
    {
        var job = _jobs.FirstOrDefault(j => j.Id == id);
        job?.Cts.Cancel();
    }

    public void RemoveJobAsync(string id)
    {
        var job = _jobs.FirstOrDefault(j => j.Id == id);
        if (job is null) return;
        if (job.Status is JobStatus.Queued or JobStatus.Processing)
            job.Cts.Cancel();
        _jobs.Remove(job);
        NotifyState();
    }

    private void NotifyState() => StateChanged?.Invoke();
}
