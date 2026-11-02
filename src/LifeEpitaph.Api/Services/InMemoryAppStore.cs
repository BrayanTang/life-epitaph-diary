using System.Collections.Concurrent;
using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Api.Services;

/// <summary>M1 in-memory store for anonymous sessions, answers, and plaza mocks.</summary>
public sealed class InMemoryAppStore
{
    private readonly ConcurrentDictionary<string, Guid> _deviceToUser = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<(Guid UserId, DateOnly Day), AnswerDto> _answers = new();
    private readonly ConcurrentDictionary<Guid, TombstoneDto> _tombstones = new();
    private readonly ConcurrentDictionary<Guid, PlazaCardDto> _publishedCards = new();

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

    public TombstoneDto? SetListedPublic(Guid userId, bool isListedPublic, PlazaCardDto? card)
    {
        if (!_tombstones.TryGetValue(userId, out var tombstone))
            return null;

        tombstone.IsListedPublic = isListedPublic;
        if (!isListedPublic)
        {
            _publishedCards.TryRemove(tombstone.Id, out _);
            return tombstone;
        }

        var published = card ?? new PlazaCardDto
        {
            TombstoneId = tombstone.Id,
            DisplayName = tombstone.DisplayName,
            Epitaph = tombstone.Epitaph,
            Status = tombstone.Status,
            SkinId = tombstone.SkinId,
            FlowerCount = tombstone.FlowerCount,
        };
        published.TombstoneId = tombstone.Id;
        published.FlowerCount = tombstone.FlowerCount;
        published.IsMine = false;
        _publishedCards[tombstone.Id] = published;
        return tombstone;
    }

    public IReadOnlyList<PlazaCardDto> ListPlaza()
    {
        var cards = new List<PlazaCardDto>();
        cards.AddRange(_publishedCards.Values);
        cards.AddRange(PlazaCards);
        return cards;
    }

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
            MoodSnippet = "今天只想慢慢走路。",
            SharedLabels = ["墓名", "墓志铭", "状态", "皮肤", "近章标题", "一句心情"],
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
            MoodSnippet = "窗边的光刚好够写一行字。",
            SharedLabels = ["墓名", "墓志铭", "状态", "皮肤", "近章标题", "一句心情"],
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
            MoodSnippet = "地铁里没说话，但故事开始了。",
            SharedLabels = ["墓名", "墓志铭", "状态", "皮肤", "近章标题", "一句心情"],
        },
    ];

    public PlazaCardDto? OfferFlower(Guid tombstoneId)
    {
        if (_publishedCards.TryGetValue(tombstoneId, out var published))
        {
            published.FlowerCount += 1;
            if (_tombstones.Values.FirstOrDefault(t => t.Id == tombstoneId) is { } mine)
                mine.FlowerCount = published.FlowerCount;
            return published;
        }

        var card = PlazaCards.FirstOrDefault(c => c.TombstoneId == tombstoneId);
        if (card is null)
            return null;

        card.FlowerCount += 1;
        return card;
    }
}
