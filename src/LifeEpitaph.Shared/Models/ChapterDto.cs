namespace LifeEpitaph.Shared.Models;

public sealed class ChapterDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly WeekStart { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<Guid> SourceAnswerIds { get; set; } = [];
    public DateTimeOffset GeneratedAt { get; set; }
    /// <summary>draft | ready | failed</summary>
    public string Status { get; set; } = "draft";
}
