namespace OmniExtract.Core.Models;

public class OutputMeta
{
    public string SourceFile { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public List<string> Language { get; set; } = new();
    public double Confidence { get; set; }
    public string ExtractionMethod { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}
