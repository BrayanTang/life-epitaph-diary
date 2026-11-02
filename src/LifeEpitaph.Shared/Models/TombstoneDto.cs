namespace LifeEpitaph.Shared.Models;

public sealed class TombstoneDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Epitaph { get; set; } = string.Empty;
    public string Status { get; set; } = "安静";
    public string SkinId { get; set; } = "soft-stone";
    public bool IsListedPublic { get; set; }
    public int FlowerCount { get; set; }
}
