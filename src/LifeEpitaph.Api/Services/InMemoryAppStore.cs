using System.Collections.Concurrent;
using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Api.Services;

/// <summary>M1 in-memory store for anonymous sessions, answers, and plaza posts.</summary>
public sealed class InMemoryAppStore
{
    private readonly ConcurrentDictionary<string, Guid> _deviceToUser = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<(Guid UserId, DateOnly Day), AnswerDto> _answers = new();
    private readonly ConcurrentDictionary<Guid, TombstoneDto> _tombstones = new();
    private readonly ConcurrentDictionary<string, PlazaCardDto> _publishedPosts = new(StringComparer.Ordinal);

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

        tombstone.IsListedPublic = isListedPublic || card is not null;
        if (card is null)
            return tombstone;

        if (string.IsNullOrWhiteSpace(card.PostId))
            card.PostId = Guid.NewGuid().ToString("N");
        card.TombstoneId = tombstone.Id;
        card.DisplayName = string.IsNullOrWhiteSpace(card.DisplayName) ? tombstone.DisplayName : card.DisplayName;
        card.IsMine = false;
        _publishedPosts[card.PostId] = card;
        return tombstone;
    }

    public IReadOnlyList<PlazaCardDto> ListPlaza()
    {
        var cards = _publishedPosts.Values
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
        cards.AddRange(MockPosts);
        return cards;
    }

    private static readonly List<PlazaCardDto> MockPosts =
    [
        new PlazaCardDto
        {
            TombstoneId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
            PostId = "mock-journal-1",
            Kind = "journal",
            DisplayName = "雾里的猫",
            Title = "一条自由日志",
            Body = "路过人间，轻轻放下一束花。今天只想慢慢走路。",
            FlowerCount = 12,
            CreatedAt = "2026-11-01 21:10",
        },
        new PlazaCardDto
        {
            TombstoneId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
            PostId = "mock-chapter-1",
            Kind = "chapter",
            DisplayName = "南风石",
            Title = "周一的白噪音",
            Body = "不必被熟人看见，也值得被温柔记录。窗边的光刚好够写一行字。",
            FlowerCount = 5,
            CreatedAt = "2026-11-01 18:40",
        },
        new PlazaCardDto
        {
            TombstoneId = Guid.Parse("33333333-3333-4333-8333-333333333333"),
            PostId = "mock-chapter-2",
            Kind = "chapter",
            DisplayName = "未署名",
            Title = "一个人的地铁",
            Body = "碎片会自己长成故事。地铁里没说话，但故事开始了。",
            FlowerCount = 28,
            CreatedAt = "2026-10-30 09:12",
        },
    ];

    public PlazaCardDto? OfferFlower(Guid tombstoneId)
    {
        var post = _publishedPosts.Values.FirstOrDefault(p => p.TombstoneId == tombstoneId)
                   ?? MockPosts.FirstOrDefault(p => p.TombstoneId == tombstoneId);
        if (post is null)
            return null;

        post.FlowerCount += 1;
        return post;
    }
}
