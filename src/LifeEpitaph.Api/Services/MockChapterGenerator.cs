using LifeEpitaph.Shared.Ai;
using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Api.Services;

public sealed class MockChapterGenerator : IChapterGenerator
{
    public Task<ChapterDto> GenerateAsync(
        Guid userId,
        DateOnly weekStart,
        IReadOnlyList<AnswerDto> answers,
        CancellationToken cancellationToken = default)
    {
        var snippets = answers
            .Where(a => !string.IsNullOrWhiteSpace(a.Body))
            .Select(a => a.Body.Trim())
            .Take(5)
            .ToList();

        var body = snippets.Count == 0
            ? "这一周很安静。碎片还在路上，故事会慢慢长出来。"
            : "这一周，你留下了这些轻轻的痕迹：\n\n"
              + string.Join("\n", snippets.Select((s, i) => $"{i + 1}. {s}"))
              + "\n\n（Mock 章节 · 接入真实模型后将改写为连贯剧情）";

        var chapter = new ChapterDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WeekStart = weekStart,
            Title = $"第 {weekStart:yyyy-MM-dd} 周 · 安静的碎片",
            Body = body,
            SourceAnswerIds = answers.Select(a => a.Id).ToList(),
            GeneratedAt = DateTimeOffset.UtcNow,
            Status = "ready",
        };

        return Task.FromResult(chapter);
    }
}
