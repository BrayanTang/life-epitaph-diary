using LifeEpitaph.App.Models;
using LifeEpitaph.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace LifeEpitaph.App.Services;

public sealed class AnonymousSessionService
{
    private readonly LocalStore _store;
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AnonymousSessionService(LocalStore store, IHttpClientFactory httpClientFactory)
    {
        _store = store;
        _http = httpClientFactory.CreateClient("LifeEpitaphApi");
    }

    public LocalSession? Current { get; private set; }

    public async Task<LocalSession> EnsureSessionAsync()
    {
        await _store.EnsureInitializedAsync();
        var existing = await _store.GetSessionAsync();
        if (existing is not null && !string.IsNullOrWhiteSpace(existing.AccessToken))
        {
            Current = existing;
            return existing;
        }

        var deviceId = existing?.DeviceId;
        if (string.IsNullOrWhiteSpace(deviceId))
            deviceId = Guid.NewGuid().ToString("N");

        LocalSession session;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var response = await _http.PostAsJsonAsync(
                "auth/anonymous",
                new AnonymousSessionRequest
                {
                    DeviceId = deviceId,
                    Platform = DeviceInfo.Current.Platform.ToString(),
                },
                cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<AnonymousSessionResponse>(JsonOptions, cts.Token)
                              ?? throw new InvalidOperationException("Empty anonymous session response.");

                session = new LocalSession
                {
                    DeviceId = deviceId,
                    UserId = payload.UserId.ToString("N"),
                    AccessToken = payload.AccessToken,
                };
            }
            else
            {
                session = CreateLocalOnlySession(deviceId);
            }
        }
        catch
        {
            // Offline / API down: still open the app with a local anonymous session.
            session = CreateLocalOnlySession(deviceId);
        }

        await _store.SaveSessionAsync(session);
        Current = session;
        return session;
    }

    private static LocalSession CreateLocalOnlySession(string deviceId) => new()
    {
        DeviceId = deviceId,
        UserId = Guid.NewGuid().ToString("N"),
        AccessToken = $"anon.{Guid.NewGuid():N}",
    };
}
