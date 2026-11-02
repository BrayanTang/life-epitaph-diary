namespace LifeEpitaph.Shared.Models;

/// <summary>
/// Plaza-safe card: epitaph + skin + recent chapter titles only (never chapter body).
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
}
