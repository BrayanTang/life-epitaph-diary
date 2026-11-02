using LifeEpitaph.Api.Data;
using LifeEpitaph.Shared.Ai;
using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Api.Services;

public sealed class MockDailyPromptService : IDailyPromptService
{
    public Task<DailyPromptDto> GetPromptForDateAsync(
        DateOnly date,
        string locale = "zh-CN",
        CancellationToken cancellationToken = default)
    {
        var prompt = new DailyPromptDto
        {
            Id = Guid.Parse(
                $"{date.Year:D4}{date.Month:D2}{date.Day:D2}-0000-4000-8000-000000000001"),
            Date = date,
            Text = PromptBank.PickForDate(date, locale),
            Locale = locale,
        };
        return Task.FromResult(prompt);
    }
}
