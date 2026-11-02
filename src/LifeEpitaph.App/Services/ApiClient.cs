using LifeEpitaph.Shared.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LifeEpitaph.App.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly AnonymousSessionService _session;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(IHttpClientFactory httpClientFactory, AnonymousSessionService session)
    {
        _http = httpClientFactory.CreateClient("LifeEpitaphApi");
        _session = session;
    }

    public async Task<DailyPromptDto?> GetTodayPromptAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<DailyPromptDto>("prompts/today?locale=zh-CN", JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<PlazaCardDto>> GetPlazaAsync()
    {
        try
        {
            var cards = await _http.GetFromJsonAsync<List<PlazaCardDto>>("plaza", JsonOptions);
            return cards ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task SaveAnswerRemoteAsync(SaveAnswerRequest request)
    {
        try
        {
            await EnsureAuthHeaderAsync();
            await _http.PutAsJsonAsync("answers", request);
        }
        catch
        {
            // Local autosave already persisted; remote sync is best-effort in M1.
        }
    }

    public async Task<PlazaCardDto> OfferFlowerAsync(Guid tombstoneId)
    {
        var response = await _http.PostAsync($"plaza/{tombstoneId:D}/flowers", content: null);
        response.EnsureSuccessStatusCode();
        var card = await response.Content.ReadFromJsonAsync<PlazaCardDto>(JsonOptions);
        if (card is null)
            throw new InvalidOperationException("献花响应为空");
        return card;
    }

    private async Task EnsureAuthHeaderAsync()
    {
        var session = _session.Current ?? await _session.EnsureSessionAsync();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", session.AccessToken);
    }
}
