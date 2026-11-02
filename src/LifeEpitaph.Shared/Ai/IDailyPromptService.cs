using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Shared.Ai;

/// <summary>
/// Resolves the daily prompt. Production providers may call an LLM;
/// Mock implementations use a static question bank.
/// </summary>
public interface IDailyPromptService
{
    Task<DailyPromptDto> GetPromptForDateAsync(DateOnly date, string locale = "zh-CN", CancellationToken cancellationToken = default);
}
