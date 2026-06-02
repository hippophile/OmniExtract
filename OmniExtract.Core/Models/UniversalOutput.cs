using System.Text.Json.Serialization;

namespace OmniExtract.Core.Models;

public class UniversalOutput
{
    [JsonPropertyName("meta")]
    public OutputMeta Meta { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("categories")]
    public OutputCategories Categories { get; set; } = new();

    [JsonPropertyName("data")]
    public Dictionary<string, object?> Data { get; set; } = new();

    [JsonPropertyName("tables")]
    public List<List<List<string>>> Tables { get; set; } = new();

    [JsonPropertyName("gaps")]
    public List<string> Gaps { get; set; } = new();
}

public class OutputCategories
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("subdomain")]
    public string Subdomain { get; set; } = string.Empty;

    [JsonPropertyName("sensitivity")]
    public string Sensitivity { get; set; } = "internal";
}
