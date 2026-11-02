using LifeEpitaph.Shared.Ai;
using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Api.Services;

/// <summary>
/// Mock weekly chapter: turns short scraps into a gentle Quixote-style vignette.
/// </summary>
public sealed class MockChapterGenerator : IChapterGenerator
{
    public Task<ChapterDto> GenerateAsync(
        Guid userId,
        DateOnly weekStart,
        IReadOnlyList<AnswerDto> answers,
        CancellationToken cancellationToken = default)
    {
        var scraps = answers
            .Where(a => !string.IsNullOrWhiteSpace(a.Body))
            .Select(a => a.Body)
            .ToList();

        var (title, body) = QuixoteChapterComposer.Compose(weekStart, scraps);

        var chapter = new ChapterDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WeekStart = weekStart,
            Title = title,
            Body = body,
            SourceAnswerIds = answers.Select(a => a.Id).ToList(),
            GeneratedAt = DateTimeOffset.UtcNow,
            Status = "ready",
        };

        return Task.FromResult(chapter);
    }
}
