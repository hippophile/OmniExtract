using OmniExtract.Core.Models;

namespace OmniExtract.Web.Services;

public class ResultsEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string FileName { get; set; } = string.Empty;
    public UniversalOutput Output { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
    public bool IsMock { get; set; }
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

    public ResultsEntry Add(string fileName, UniversalOutput output)
    {
        var entry = new ResultsEntry
        {
            FileName = fileName,
            Output = output,
            ProcessedAt = DateTime.UtcNow,
            IsMock = false
        };
        _entries.Insert(0, entry);
        return entry;
    }
}
