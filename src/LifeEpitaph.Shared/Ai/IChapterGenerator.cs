using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Shared.Ai;

/// <summary>
/// Generates a weekly life-story chapter from answers.
/// Real providers plug in behind this interface; Mock returns stub prose.
/// </summary>
public interface IChapterGenerator
{
    Task<ChapterDto> GenerateAsync(
        Guid userId,
        DateOnly weekStart,
        IReadOnlyList<AnswerDto> answers,
        CancellationToken cancellationToken = default);
}
