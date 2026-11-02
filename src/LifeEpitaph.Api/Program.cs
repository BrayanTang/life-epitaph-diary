using System.Security.Claims;
using LifeEpitaph.Api.Services;
using LifeEpitaph.Shared.Ai;
using LifeEpitaph.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<InMemoryAppStore>();
builder.Services.AddSingleton<IAiTextProvider, NullAiTextProvider>();
builder.Services.AddSingleton<IDailyPromptService, MockDailyPromptService>();
builder.Services.AddSingleton<IChapterGenerator, MockChapterGenerator>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "LifeEpitaph.Api",
    utc = DateTimeOffset.UtcNow,
}));

app.MapPost("/auth/anonymous", (AnonymousSessionRequest request, InMemoryAppStore store) =>
{
    var session = store.GetOrCreateSession(request);
    return Results.Ok(session);
});

app.MapGet("/prompts/today", async (IDailyPromptService prompts, string? locale) =>
{
    var date = DateOnly.FromDateTime(DateTime.UtcNow);
    var prompt = await prompts.GetPromptForDateAsync(date, locale ?? "zh-CN");
    return Results.Ok(prompt);
});

app.MapGet("/prompts/{date}", async (DateOnly date, IDailyPromptService prompts, string? locale) =>
{
    var prompt = await prompts.GetPromptForDateAsync(date, locale ?? "zh-CN");
    return Results.Ok(prompt);
});

app.MapGet("/answers/today", (HttpRequest http, InMemoryAppStore store) =>
{
    if (!TryGetUserId(http, out var userId))
        return Results.Unauthorized();

    var day = DateOnly.FromDateTime(DateTime.UtcNow);
    var answer = store.GetAnswer(userId, day);
    return answer is null ? Results.NoContent() : Results.Ok(answer);
});

app.MapPut("/answers", (HttpRequest http, SaveAnswerRequest body, InMemoryAppStore store) =>
{
    if (!TryGetUserId(http, out var userId))
        return Results.Unauthorized();

    var saved = store.SaveAnswer(userId, body);
    return Results.Ok(saved);
});

app.MapGet("/answers", (HttpRequest http, InMemoryAppStore store) =>
{
    if (!TryGetUserId(http, out var userId))
        return Results.Unauthorized();

    return Results.Ok(store.ListAnswers(userId));
});

app.MapGet("/tombstone/me", (HttpRequest http, InMemoryAppStore store) =>
{
    if (!TryGetUserId(http, out var userId))
        return Results.Unauthorized();

    var tombstone = store.GetTombstone(userId);
    return tombstone is null ? Results.NotFound() : Results.Ok(tombstone);
});

app.MapPut("/tombstone/me/visibility", (HttpRequest http, PlazaVisibilityRequest body, InMemoryAppStore store) =>
{
    if (!TryGetUserId(http, out var userId))
        return Results.Unauthorized();

    var tombstone = store.SetListedPublic(userId, body.IsListedPublic, body.Card);
    return tombstone is null ? Results.NotFound() : Results.Ok(tombstone);
});

app.MapGet("/plaza", (InMemoryAppStore store) => Results.Ok(store.ListPlaza()));

app.MapPost("/plaza/{tombstoneId:guid}/flowers", (Guid tombstoneId, InMemoryAppStore store) =>
{
    var card = store.OfferFlower(tombstoneId);
    return card is null ? Results.NotFound() : Results.Ok(card);
});

app.MapPost("/chapters/generate", async (
    HttpRequest http,
    InMemoryAppStore store,
    IChapterGenerator generator) =>
{
    if (!TryGetUserId(http, out var userId))
        return Results.Unauthorized();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var weekStart = today.AddDays(-(int)today.DayOfWeek);
    var answers = store.ListAnswers(userId)
        .Where(a => a.AnsweredOn >= weekStart && a.AnsweredOn < weekStart.AddDays(7))
        .ToList();

    var chapter = await generator.GenerateAsync(userId, weekStart, answers);
    return Results.Ok(chapter);
});

app.Run();

static bool TryGetUserId(HttpRequest http, out Guid userId)
{
    userId = default;
    var auth = http.Headers.Authorization.ToString();
    if (string.IsNullOrWhiteSpace(auth))
        return false;

    const string prefix = "Bearer anon.";
    if (!auth.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        return false;

    return Guid.TryParseExact(auth[prefix.Length..], "N", out userId);
}

// Expose for integration tests later
public partial class Program;
