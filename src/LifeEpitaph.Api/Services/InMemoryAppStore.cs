using System.Collections.Concurrent;
using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Api.Services;

/// <summary>M1 in-memory store for anonymous sessions, answers, and plaza mocks.</summary>
public sealed class InMemoryAppStore
{
    private readonly ConcurrentDictionary<string, Guid> _deviceToUser = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<(Guid UserId, DateOnly Day), AnswerDto> _answers = new();
    private readonly ConcurrentDictionary<Guid, TombstoneDto> _tombstones = new();

    public AnonymousSessionResponse GetOrCreateSession(AnonymousSessionRequest request)
    {
        var deviceId = string.IsNullOrWhiteSpace(request.DeviceId)
            ? Guid.NewGuid().ToString("N")
            : request.DeviceId.Trim();

        var isNew = false;
        var userId = _deviceToUser.GetOrAdd(deviceId, _ =>
        {
            isNew = true;
            return Guid.NewGuid();
        });

        if (isNew)
        {
            _tombstones[userId] = new TombstoneDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DisplayName = "无名之石",
                Epitaph = "在这里，安静地记下一周的自己。",
                Status = "安静",
                SkinId = "soft-stone",
                IsListedPublic = false,
                FlowerCount = 0,
            };
        }

        return new AnonymousSessionResponse
        {
            UserId = userId,
            AccessToken = $"anon.{userId:N}",
            IsNewUser = isNew,
        };
    }

    public AnswerDto SaveAnswer(Guid userId, SaveAnswerRequest request)
    {
        var day = request.AnsweredOn ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var key = (userId, day);
        var now = DateTimeOffset.UtcNow;

        return _answers.AddOrUpdate(
            key,
            _ => new AnswerDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PromptId = request.PromptId,
                Body = request.Body ?? string.Empty,
                AnsweredOn = day,
                CreatedAt = now,
                UpdatedAt = now,
            },
            (_, existing) =>
            {
                existing.PromptId = request.PromptId;
                existing.Body = request.Body ?? string.Empty;
                existing.UpdatedAt = now;
                return existing;
            });
    }

    public AnswerDto? GetAnswer(Guid userId, DateOnly day)
        => _answers.TryGetValue((userId, day), out var answer) ? answer : null;

    public IReadOnlyList<AnswerDto> ListAnswers(Guid userId)
        => _answers.Values.Where(a => a.UserId == userId).OrderByDescending(a => a.AnsweredOn).ToList();

    public TombstoneDto? GetTombstone(Guid userId)
        => _tombstones.TryGetValue(userId, out var t) ? t : null;

    private static readonly List<PlazaCardDto> PlazaCards =
    [
        new PlazaCardDto
        {
            TombstoneId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
            DisplayName = "雾里的猫",
            Epitaph = "路过人间，轻轻放下一束花。",
            Status = "发呆",
            SkinId = "moss-kitty",
            FlowerCount = 12,
            RecentChapterTitles = ["雨停的那一晚", "把钥匙放在口袋里"],
        },
        new PlazaCardDto
        {
            TombstoneId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
            DisplayName = "南风石",
            Epitaph = "不必被熟人看见，也值得被温柔记录。",
            Status = "安静",
            SkinId = "soft-stone",
            FlowerCount = 5,
            RecentChapterTitles = ["周一的白噪音"],
        },
        new PlazaCardDto
        {
            TombstoneId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
            DisplayName = "未署名",
            Epitaph = "碎片会自己长成故事。",
            Status = "想说话",
            SkinId = "lantern-night",
            FlowerCount = 28,
            RecentChapterTitles = ["第三章 · 短答的回声", "窗边的咖啡凉了", "一个人的地铁"],
        },
    ];

    public static IReadOnlyList<PlazaCardDto> MockPlazaCards() => PlazaCards;

    public static PlazaCardDto? OfferFlower(Guid tombstoneId)
    {
        var card = PlazaCards.FirstOrDefault(c => c.TombstoneId == tombstoneId);
        if (card is null)
            return null;

        card.FlowerCount += 1;
        return card;
    }
}
