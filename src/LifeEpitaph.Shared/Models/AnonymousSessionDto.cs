namespace LifeEpitaph.Shared.Models;

public sealed class AnonymousSessionRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string Platform { get; set; } = "unknown";
}

public sealed class AnonymousSessionResponse
{
    public Guid UserId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public bool IsNewUser { get; set; }
}
