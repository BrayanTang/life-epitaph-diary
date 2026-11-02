namespace LifeEpitaph.Shared.Models;

/// <summary>
/// One plaza post: either a shared journal scrap or a shared chapter story.
/// </summary>
public sealed class PlazaCardDto
{
    public Guid TombstoneId { get; set; }
    public string PostId { get; set; } = string.Empty;
    /// <summary>journal | chapter</summary>
    public string Kind { get; set; } = "journal";
    public string DisplayName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int FlowerCount { get; set; }
    public bool IsMine { get; set; }
    public string CreatedAt { get; set; } = string.Empty;

    // Kept for older mock data compatibility during transition.
    public string Epitaph { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SkinId { get; set; } = string.Empty;
    public IReadOnlyList<string> RecentChapterTitles { get; set; } = [];
    public string MoodSnippet { get; set; } = string.Empty;
    public IReadOnlyList<string> SharedLabels { get; set; } = [];
}
