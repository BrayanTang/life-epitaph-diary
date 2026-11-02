namespace LifeEpitaph.Shared.Models;

public sealed class AnswerDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PromptId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateOnly AnsweredOn { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class SaveAnswerRequest
{
    public Guid PromptId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateOnly? AnsweredOn { get; set; }
}
