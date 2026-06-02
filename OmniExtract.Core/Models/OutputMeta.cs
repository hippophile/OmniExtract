using System.Text.Json.Serialization;

namespace OmniExtract.Core.Models;

public class OutputMeta
{
    [JsonPropertyName("source_file")]
    public string SourceFile { get; set; } = string.Empty;

    [JsonPropertyName("document_type")]
    public string DocumentType { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public List<string> Language { get; set; } = new();

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("extraction_method")]
    public string ExtractionMethod { get; set; } = string.Empty;

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}
