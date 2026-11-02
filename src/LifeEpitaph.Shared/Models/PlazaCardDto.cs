namespace LifeEpitaph.Shared.Models;

/// <summary>
/// Plaza-safe card. Never includes chapter body or full private journal.
/// Fields may be empty when the owner chose not to share them.
/// </summary>
public sealed class PlazaCardDto
{
    public Guid TombstoneId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Epitaph { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SkinId { get; set; } = string.Empty;
    public int FlowerCount { get; set; }
    public IReadOnlyList<string> RecentChapterTitles { get; set; } = [];
    /// <summary>Optional one-line mood from private journal when shared.</summary>
    public string MoodSnippet { get; set; } = string.Empty;
    public bool IsMine { get; set; }
    public IReadOnlyList<string> SharedLabels { get; set; } = [];
}
