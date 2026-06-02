using System.Text;
using System.Text.RegularExpressions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmniExtract.Core.Config;
using OmniExtract.Core.Models;

namespace OmniExtract.App.Services;

public class GptClient : IAsyncDisposable
{
    private static readonly Regex MarkdownFenceRegex = new(@"^```(?:json)?|```$", RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly CopilotClient _copilot;
    private readonly OpenAISettings _settings;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<GptClient> _logger;

    public GptClient(IOptions<OpenAISettings> settings, IOptions<ProcessingSettings> processing, ILogger<GptClient> logger)
    {
        var cfg = settings.Value;
        _copilot = new CopilotClient(new CopilotClientOptions());
        _settings = cfg;
        _semaphore = new SemaphoreSlim(processing.Value.ApiConcurrency);
        _logger = logger;
    }

    public async Task<string> CallAsync(List<GptMessage> messages, float temperature = 0, int? maxTokens = null, CancellationToken ct = default, string? model = null)
    {
        var result = await CallWithMetadataAsync(messages, temperature, maxTokens, ct, model);
        return result.Content;
    }

    public async Task<GptCallResult> CallWithMetadataAsync(List<GptMessage> messages, float temperature = 0, int? maxTokens = null, CancellationToken ct = default, string? model = null)
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

                    var result = await ExecuteAsync(messages, model ?? _settings.Model, timeoutCts.Token);
                    _logger.LogInformation("  GPT: response OK ({Length} chars)", result.Content.Length);
                    return result;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex) when (IsRateLimit(ex))
            {
                var wait = Math.Min(30d * (attempt + 1), 60d);
                _logger.LogWarning("Rate limit; waiting {Wait}s ({Attempt}/5)", Math.Round(wait), attempt + 1);
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
                _logger.LogWarning("Overloaded; waiting {Wait}s ({Attempt}/5)", wait, attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(wait), ct);
            }
            catch (Exception ex) when (IsContextLimitExceeded(ex))
            {
                _logger.LogWarning("Context limit exceeded — not retrying");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GPT API call failed on attempt {Attempt}", attempt + 1);
                throw;
            }
        }

        throw new InvalidOperationException("GPT call failed after max retries");
    }

    private async Task<GptCallResult> ExecuteAsync(List<GptMessage> messages, string model, CancellationToken ct)
    {
        var systemContent = string.Join("\n\n", messages.OfType<SystemGptMessage>().Select(m => m.Content));
        var nonSystem = messages.Where(m => m is not SystemGptMessage).ToList();

        string prompt;
        IList<UserMessageAttachment> attachments = [];

        if (nonSystem.Count == 1 && nonSystem[0] is UserGptMessage single)
        {
            prompt = single.Text;
            if (single.Images?.Count > 0)
                attachments = single.Images
                    .Select(b64 => (UserMessageAttachment)new UserMessageAttachmentBlob { Data = b64, MimeType = "image/png" })
                    .ToList();
        }
        else
        {
            var sb = new StringBuilder();
            UserGptMessage? lastUser = null;
            foreach (var msg in nonSystem)
            {
                switch (msg)
                {
                    case UserGptMessage u:
                        sb.AppendLine($"[User]: {u.Text}");
                        lastUser = u;
                        break;
                    case AssistantGptMessage a:
                        sb.AppendLine($"[Assistant]: {a.Content}");
                        break;
                }
            }
            prompt = sb.ToString().Trim();
            if (lastUser?.Images?.Count > 0)
                attachments = lastUser.Images
                    .Select(b64 => (UserMessageAttachment)new UserMessageAttachmentBlob { Data = b64, MimeType = "image/png" })
                    .ToList();
        }

        string? assistantContent = null;
        var responseTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var session = await _copilot.CreateSessionAsync(new SessionConfig
        {
            Model = model,
            SystemMessage = new SystemMessageConfig
            {
                Mode = SystemMessageMode.Replace,
                Content = systemContent,
            },
            OnPermissionRequest = PermissionHandler.ApproveAll,
            AvailableTools = [],
        }, ct);

        using var sub = session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageEvent msg:
                    assistantContent = msg.Data?.Content ?? string.Empty;
                    break;
                case SessionIdleEvent:
                    responseTcs.TrySetResult(assistantContent ?? string.Empty);
                    break;
                case SessionErrorEvent err:
                    responseTcs.TrySetException(new Exception(err.Data?.Message ?? "Session error"));
                    break;
            }
        });

        await session.SendAsync(new MessageOptions
        {
            Prompt = prompt,
            Attachments = attachments,
        }, ct);

        var response = await responseTcs.Task.WaitAsync(ct);
        var cleaned = MarkdownFenceRegex.Replace(response, string.Empty).Trim();
        return new GptCallResult(cleaned, false);
    }

    public static bool IsContextLimitExceeded(Exception ex) =>
        ex.ToString().Contains("prompt token count", StringComparison.OrdinalIgnoreCase) &&
        ex.ToString().Contains("exceeds the limit", StringComparison.OrdinalIgnoreCase);

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

    public ValueTask DisposeAsync() => _copilot.DisposeAsync();

    public readonly record struct GptCallResult(string Content, bool Truncated);
}
