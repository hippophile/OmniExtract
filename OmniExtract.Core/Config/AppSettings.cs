namespace OmniExtract.Core.Config;

public class OpenAISettings
{
    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com";
    public string Model { get; set; } = "gpt-4.1";
    public string VisionModel { get; set; } = "gpt-4o";
    public string ApiKeyEnvVar { get; set; } = "GITHUB_TOKEN";
}

public class ProcessingSettings
{
    public int ApiConcurrency { get; set; } = 2;
    public int VisionChunkSize { get; set; } = 6;
    public int VisionMaxDimension { get; set; } = 1024;
    public int PdfDpi { get; set; } = 150;
    public int MaxOutputTokens { get; set; } = 4096;
    public int ModelContextLimit { get; set; } = 128000;
    public int TokenBuffer { get; set; } = 8000;
}

public class PathSettings
{
    public string WatchDir { get; set; } = "./inbox";
}
