namespace OmniExtract.Core.Models;

public abstract record GptMessage;
public record SystemGptMessage(string Content) : GptMessage;
public record AssistantGptMessage(string Content) : GptMessage;
public record UserGptMessage(string Text, List<string>? Images = null) : GptMessage;
