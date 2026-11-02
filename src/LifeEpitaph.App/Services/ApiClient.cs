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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            return await _http.GetFromJsonAsync<DailyPromptDto>("prompts/today?locale=zh-CN", JsonOptions, cts.Token);
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var cards = await _http.GetFromJsonAsync<List<PlazaCardDto>>("plaza", JsonOptions, cts.Token);
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
            using var requestMessage = await CreateAuthorizedAsync(HttpMethod.Put, "answers");
            requestMessage.Content = JsonContent.Create(request);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await _http.SendAsync(requestMessage, cts.Token);
        }
        catch
        {
        }
    }

    public async Task<PlazaCardDto> OfferFlowerAsync(Guid tombstoneId)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"plaza/{tombstoneId:D}/flowers");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var response = await _http.SendAsync(requestMessage, cts.Token);
        response.EnsureSuccessStatusCode();
        var card = await response.Content.ReadFromJsonAsync<PlazaCardDto>(JsonOptions, cts.Token);
        if (card is null)
            throw new InvalidOperationException("献花响应为空");
        return card;
    }

    public async Task SetPlazaVisibilityAsync(bool isListedPublic)
    {
        using var requestMessage = await CreateAuthorizedAsync(HttpMethod.Put, "tombstone/me/visibility");
        requestMessage.Content = JsonContent.Create(new PlazaVisibilityRequest { IsListedPublic = isListedPublic });
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var response = await _http.SendAsync(requestMessage, cts.Token);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpRequestMessage> CreateAuthorizedAsync(HttpMethod method, string url)
    {
        var session = _session.Current ?? await _session.EnsureSessionAsync();
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return request;
    }
}
