using OmniExtract.Core.Models;

namespace OmniExtract.Web.Services;

public enum ExtractionRating { Unrated, Good, Bad }

public class ResultsEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string FileName { get; set; } = string.Empty;
    public UniversalOutput Output { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
    public bool IsMock { get; set; }
    public bool IsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetail { get; set; }
    public ExtractionRating Rating { get; set; } = ExtractionRating.Unrated;
    public string? OriginalFileUrl { get; set; }
}

public class ResultsRepository
{
    private readonly List<ResultsEntry> _entries = [];

    public ResultsRepository()
    {
        var mocks = MockDataService.GetMockResults();
        for (var i = 0; i < mocks.Count; i++)
        {
            var (fileName, output) = mocks[i];
            output.Meta.SourceFile = fileName;
            _entries.Add(new ResultsEntry
            {
                FileName = fileName,
                Output = output,
                ProcessedAt = DateTime.UtcNow.AddHours(-(i * 5 + Random.Shared.Next(1, 4))),
                IsMock = true
            });
        }
    }

    public IReadOnlyList<ResultsEntry> GetAll() =>
        _entries.OrderByDescending(e => e.ProcessedAt).ToList();

    public ResultsEntry? GetById(string id) =>
        _entries.FirstOrDefault(e => e.Id == id);

    public ResultsEntry Add(string fileName, UniversalOutput output, string? fileUrl = null)
    {
        var entry = new ResultsEntry
        {
            FileName = fileName,
            Output = output,
            ProcessedAt = DateTime.UtcNow,
            IsMock = false,
            OriginalFileUrl = fileUrl
        };
        _entries.Insert(0, entry);
        return entry;
    }

    public void Rate(string id, ExtractionRating rating)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == id);
        if (entry is null) return;
        entry.Rating = entry.Rating == rating ? ExtractionRating.Unrated : rating;
    }

    public ResultsEntry AddFailed(string fileName, string errorMessage, string errorDetail)
    {
        var entry = new ResultsEntry
        {
            FileName = fileName,
            Output = new UniversalOutput(),
            ProcessedAt = DateTime.UtcNow,
            IsMock = false,
            IsFailed = true,
            ErrorMessage = errorMessage,
            ErrorDetail = errorDetail
        };
        _entries.Insert(0, entry);
        return entry;
    }
}
