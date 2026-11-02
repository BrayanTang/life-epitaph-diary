using LifeEpitaph.Shared.Ai;

namespace LifeEpitaph.Api.Services;

/// <summary>Placeholder provider until a real China-usable LLM key is configured.</summary>
public sealed class NullAiTextProvider : IAiTextProvider
{
    public string ProviderName => "null";
    public bool IsConfigured => false;

    public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(
            "No AI provider configured. Use MockDailyPromptService / MockChapterGenerator, or register a real IAiTextProvider.");
}
