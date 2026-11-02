namespace LifeEpitaph.Shared.Ai;

/// <summary>
/// Pluggable LLM text provider. Ready for a China-usable API later.
/// Do not put API keys in the client; wire secrets only on the server.
/// </summary>
public interface IAiTextProvider
{
    string ProviderName { get; }
    bool IsConfigured { get; }

    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
