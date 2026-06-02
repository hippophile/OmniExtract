using Microsoft.Extensions.Options;
using OmniExtract.Core.Config;
using OmniExtract.Core.Models;
using SharpToken;

namespace OmniExtract.App.Services;

public class TokenCounter
{
    private readonly GptEncoding _encoding;
    private readonly ProcessingSettings _settings;

    public TokenCounter(IOptions<ProcessingSettings> settings)
    {
        _encoding = GptEncoding.GetEncodingForModel("gpt-4o-mini");
        _settings = settings.Value;
    }

    public int CountTokens(string text) => _encoding.Encode(text).Count;

    public int CountMessageTokens(List<GptMessage> messages)
    {
        var tokens = 0;
        foreach (var message in messages)
        {
            tokens += 4;
            var (role, content) = message switch
            {
                SystemGptMessage s    => ("system", s.Content),
                UserGptMessage u      => ("user", u.Text),
                AssistantGptMessage a => ("assistant", a.Content),
                _                     => ("unknown", string.Empty)
            };
            tokens += _encoding.Encode(role).Count;
            tokens += _encoding.Encode(content).Count;
        }
        tokens += 2;
        return tokens;
    }

    public int EstimateAvailableTokens(List<GptMessage> messages)
    {
        var used = CountMessageTokens(messages);
        var available = _settings.ModelContextLimit - used - _settings.TokenBuffer - _settings.MaxOutputTokens;
        return Math.Max(0, available);
    }

    public string TruncateToTokens(string text, int maxTokens)
    {
        var tokens = _encoding.Encode(text);
        if (tokens.Count <= maxTokens) return text;
        return _encoding.Decode(tokens.Take(maxTokens).ToList());
    }
}
