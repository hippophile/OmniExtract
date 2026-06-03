using OmniExtract.Core.Models;

namespace OmniExtract.Web.Models;

public enum JobStatus { Queued, Processing, Done, Failed, Cancelled }

public class ProcessingJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public string? ErrorMessage { get; set; }
    public string? ErrorDetail { get; set; }
    public UniversalOutput? Result { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string TempPath { get; set; } = string.Empty;
    public string Stage { get; set; } = "Queued";
    public string? ResultId { get; set; }
    public string? UploadedFileUrl { get; set; }
    public CancellationTokenSource Cts { get; set; } = new();
    public DateTime StageStartedAt { get; set; } = DateTime.UtcNow;
    public int TokenCount { get; set; }
    public int ChunkCount { get; set; }
}
