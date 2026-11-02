namespace LifeEpitaph.Shared.Models;

public sealed class DailyPromptDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Locale { get; set; } = "zh-CN";
}
