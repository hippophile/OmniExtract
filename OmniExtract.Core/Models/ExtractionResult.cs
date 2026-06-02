namespace OmniExtract.Core.Models;

public class ExtractionResult
{
    public string Text { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public string? Error { get; set; }
    public bool IsGibberish { get; set; }
}
