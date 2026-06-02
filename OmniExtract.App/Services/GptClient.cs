using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmniExtract.Core.Config;
using OmniExtract.Core.Models;

namespace OmniExtract.App.Services;

public class GptClient : IDisposable
{
    private static readonly Regex MarkdownFenceRegex = new(@"^```(?:json)?|```$", RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly HttpClient _http;
    private readonly OpenAISettings _settings;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<GptClient> _logger;
    private readonly string _endpoint;

    public GptClient(IOptions<OpenAISettings> settings, IOptions<ProcessingSettings> processing, ILogger<GptClient> logger)
    {
        var cfg = settings.Value;
        var token = Environment.GetEnvironmentVariable(cfg.ApiKeyEnvVar)
            ?? throw new InvalidOperationException($"Environment variable '{cfg.ApiKeyEnvVar}' not set");

        _settings = cfg;
        _endpoint = cfg.Endpoint.TrimEnd('/') + "/chat/completions";
        _semaphore = new SemaphoreSlim(processing.Value.ApiConcurrency);
        _logger = logger;

        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _http.Timeout = TimeSpan.FromSeconds(180);
    }

    public async Task<string> CallAsync(List<GptMessage> messages, float temperature = 0, int? maxTokens = null, CancellationToken ct = default)
    {
        var result = await CallWithMetadataAsync(messages, temperature, maxTokens, ct);
        return result.Content;
    }

    public async Task<GptCallResult> CallWithMetadataAsync(List<GptMessage> messages, float temperature = 0, int? maxTokens = null, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                await _semaphore.WaitAsync(ct);
                _logger.LogInformation("  GPT: calling API (attempt {Attempt}/5, {MsgCount} messages)...", attempt + 1, messages.Count);

                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(120));

                    var content = await ExecuteAsync(messages, temperature, maxTokens, timeoutCts.Token);
                    _logger.LogInformation("  GPT: response OK ({Length} chars)", content.Length);
                    return new GptCallResult(content, false);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex) when (IsRateLimit(ex))
            {
                var wait = Math.Min(30d * (attempt + 1), 60d);
                _logger.LogWarning("Rate limit; waiting {Wait}s (attempt {Attempt}/5)", Math.Round(wait), attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(wait), ct);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning("GPT call timed out on attempt {Attempt}/5, retrying...", attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
            catch (Exception ex) when (IsOverloaded(ex))
            {
                var wait = 20d * (attempt + 1);
                _logger.LogWarning("Overloaded; waiting {Wait}s (attempt {Attempt}/5)", wait, attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(wait), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GPT API call failed on attempt {Attempt}", attempt + 1);
                throw;
            }
        }

        throw new InvalidOperationException("GPT call failed after max retries");
    }

    private async Task<string> ExecuteAsync(List<GptMessage> messages, float temperature, int? maxTokens, CancellationToken ct)
    {
        var requestMessages = new List<object>();

        foreach (var msg in messages)
        {
            switch (msg)
            {
                case SystemGptMessage sys:
                    requestMessages.Add(new { role = "system", content = sys.Content });
                    break;

                case UserGptMessage user when user.Images?.Count > 0:
                    var contentParts = new List<object>
                    {
                        new { type = "text", text = user.Text }
                    };
                    foreach (var b64 in user.Images)
                    {
                        contentParts.Add(new
                        {
                            type = "image_url",
                            image_url = new { url = $"data:image/png;base64,{b64}" }
                        });
                    }
                    requestMessages.Add(new { role = "user", content = contentParts });
                    break;

                case UserGptMessage user:
                    requestMessages.Add(new { role = "user", content = user.Text });
                    break;

                case AssistantGptMessage asst:
                    requestMessages.Add(new { role = "assistant", content = asst.Content });
                    break;
            }
        }

        var body = new Dictionary<string, object>
        {
            ["model"] = _settings.Model,
            ["messages"] = requestMessages,
            ["temperature"] = temperature,
        };
        if (maxTokens.HasValue)
            body["max_tokens"] = maxTokens.Value;

        var json = JsonSerializer.Serialize(body);
        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);
        var raw = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        return MarkdownFenceRegex.Replace(raw, string.Empty).Trim();
    }

    private static bool IsRateLimit(Exception ex)
    {
        var err = ex.ToString();
        return err.Contains("429") || err.Contains("rate", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOverloaded(Exception ex)
    {
        var err = ex.ToString();
        return err.Contains("503") || err.Contains("UNAVAILABLE", StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose() => _http.Dispose();

    public readonly record struct GptCallResult(string Content, bool Truncated);
}
