using LifeEpitaph.App.Models;
using LifeEpitaph.Shared.Models;
using SQLite;

namespace LifeEpitaph.App.Services;

public sealed class LocalStore
{
    private readonly SQLiteAsyncConnection _db;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public LocalStore()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "life-epitaph.db3");
        _db = new SQLiteAsyncConnection(path);
    }

    public async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
                return;

            await _db.CreateTableAsync<LocalTombstone>();
            await _db.CreateTableAsync<LocalAnswer>();
            await _db.CreateTableAsync<LocalChapter>();
            await _db.CreateTableAsync<LocalChapterEnrichment>();
            await _db.CreateTableAsync<LocalJournalEntry>();
            await _db.CreateTableAsync<LocalSession>();
            await _db.CreateTableAsync<LocalPlazaPost>();

            var tombstone = await _db.Table<LocalTombstone>().FirstOrDefaultAsync();
            if (tombstone is null)
            {
                await _db.InsertAsync(new LocalTombstone());
            }

            await EnsureStoryChaptersAsync();

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<LocalSession?> GetSessionAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalSession>().FirstOrDefaultAsync();
    }

    public async Task SaveSessionAsync(LocalSession session)
    {
        await EnsureInitializedAsync();
        session.Id = 1;
        await _db.InsertOrReplaceAsync(session);
    }

    public async Task<LocalTombstone> GetTombstoneAsync()
    {
        await EnsureInitializedAsync();
        return (await _db.Table<LocalTombstone>().FirstAsync());
    }

    public async Task SaveTombstoneAsync(LocalTombstone tombstone)
    {
        await EnsureInitializedAsync();
        await _db.InsertOrReplaceAsync(tombstone);
    }

    public async Task<LocalAnswer?> GetAnswerForDayAsync(DateOnly day)
    {
        await EnsureInitializedAsync();
        var key = day.ToString("yyyy-MM-dd");
        return await _db.Table<LocalAnswer>().Where(a => a.AnsweredOn == key).FirstOrDefaultAsync();
    }

    public async Task SaveAnswerAsync(LocalAnswer answer)
    {
        await EnsureInitializedAsync();
        var existing = await GetAnswerForDayAsync(DateOnly.Parse(answer.AnsweredOn));
        if (existing is not null)
        {
            existing.Body = answer.Body;
            existing.PromptId = answer.PromptId;
            existing.PromptText = answer.PromptText;
            existing.UpdatedAt = answer.UpdatedAt;
            await _db.UpdateAsync(existing);
        }
        else
        {
            await _db.InsertAsync(answer);
        }
    }

    public async Task<List<LocalAnswer>> ListRecentAnswersAsync(int take = 14)
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalAnswer>()
            .OrderByDescending(a => a.AnsweredOn)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<LocalChapter>> ListChaptersAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalChapter>()
            .OrderByDescending(c => c.WeekStart)
            .ToListAsync();
    }

    public async Task<LocalChapter?> GetChapterAsync(string id)
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalChapter>().Where(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<string?> GetChapterEnrichmentAsync(string chapterId)
    {
        await EnsureInitializedAsync();
        var row = await _db.Table<LocalChapterEnrichment>()
            .Where(e => e.ChapterId == chapterId)
            .FirstOrDefaultAsync();
        return row?.Body;
    }

    public async Task SaveChapterEnrichmentAsync(string chapterId, string body)
    {
        await EnsureInitializedAsync();
        await _db.InsertOrReplaceAsync(new LocalChapterEnrichment
        {
            ChapterId = chapterId,
            Body = body,
            UpdatedAt = DateTimeOffset.Now.ToString("O"),
        });
    }

    public async Task<LocalJournalEntry> AddJournalEntryAsync(string body)
    {
        await EnsureInitializedAsync();
        var now = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm");
        var entry = new LocalJournalEntry
        {
            Body = body.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };
        await _db.InsertAsync(entry);
        return entry;
    }

    public async Task<List<LocalJournalEntry>> ListJournalEntriesAsync(int take = 40)
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalJournalEntry>()
            .OrderByDescending(e => e.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<LocalPlazaPost> ShareJournalToPlazaAsync(LocalJournalEntry entry)
    {
        await EnsureInitializedAsync();
        var existing = await _db.Table<LocalPlazaPost>()
            .Where(p => p.Kind == "journal" && p.SourceId == entry.Id)
            .FirstOrDefaultAsync();
        if (existing is not null)
            return existing;

        var post = new LocalPlazaPost
        {
            Kind = "journal",
            SourceId = entry.Id,
            Title = "一条自由日志",
            Body = entry.Body,
            FlowerCount = 0,
            CreatedAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm"),
        };
        await _db.InsertAsync(post);
        return post;
    }

    public async Task<LocalPlazaPost> ShareChapterToPlazaAsync(LocalChapter chapter)
    {
        await EnsureInitializedAsync();
        var existing = await _db.Table<LocalPlazaPost>()
            .Where(p => p.Kind == "chapter" && p.SourceId == chapter.Id)
            .FirstOrDefaultAsync();
        if (existing is not null)
            return existing;

        var post = new LocalPlazaPost
        {
            Kind = "chapter",
            SourceId = chapter.Id,
            Title = chapter.Title,
            Body = chapter.Body,
            FlowerCount = 0,
            CreatedAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm"),
        };
        await _db.InsertAsync(post);
        return post;
    }

    public async Task<List<LocalPlazaPost>> ListMyPlazaPostsAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalPlazaPost>()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<LocalPlazaPost?> AddFlowerToMyPostAsync(string postId)
    {
        await EnsureInitializedAsync();
        var post = await _db.Table<LocalPlazaPost>().Where(p => p.Id == postId).FirstOrDefaultAsync();
        if (post is null)
            return null;
        post.FlowerCount += 1;
        await _db.UpdateAsync(post);
        return post;
    }

    public async Task<PlazaCardDto> ToPlazaCardAsync(LocalPlazaPost post, bool isMine = true)
    {
        var tombstone = await GetTombstoneAsync();
        _ = Guid.TryParse(tombstone.Id, out var tid);
        return new PlazaCardDto
        {
            TombstoneId = tid == Guid.Empty ? Guid.NewGuid() : tid,
            PostId = post.Id,
            Kind = post.Kind,
            DisplayName = tombstone.DisplayName,
            Title = post.Title,
            Body = post.Body,
            FlowerCount = post.FlowerCount,
            IsMine = isMine,
            CreatedAt = post.CreatedAt,
        };
    }

    private async Task EnsureStoryChaptersAsync()
    {
        var demos = new[]
        {
            new LocalChapter
            {
                Id = "demo-chapter-prologue",
                WeekStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-14)).ToString("yyyy-MM-dd"),
                Title = "序章 · 一块安静的石头",
                Body =
                    "那一周几乎没有大事发生。你把钥匙放进口袋，路过同一棵树，买了同一种面包。" +
                    "可在故事里，口袋是盔甲的暗格，那棵树是边界上的瞭望塔，面包是出征前的干粮。" +
                    "平凡并不妨碍你成为自己生活的骑士——哪怕对手只是路边的风车。",
                Status = "ready",
            },
            new LocalChapter
            {
                Id = "demo-chapter-windmill",
                WeekStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-7)).ToString("yyyy-MM-dd"),
                Title = "唐吉诃德大战风车",
                Body =
                    "周一的地铁门开合得像巨兽眨眼。你挤进去，被人群轻轻推着前进，心里其实只想早点到公司。" +
                    "故事却把这一幕写成：你骑着想象中的瘦马，冲向旋转的金属风车——它们轰鸣、闪光，看似强大。" +
                    "你没有真的打倒它们，但你抬头了。抬头本身，就是一次小小的开战。",
                Status = "ready",
            },
        };

        foreach (var demo in demos)
        {
            var existing = await _db.Table<LocalChapter>().Where(c => c.Id == demo.Id).FirstOrDefaultAsync();
            if (existing is null)
                await _db.InsertAsync(demo);
            else
            {
                existing.Title = demo.Title;
                existing.Body = demo.Body;
                existing.WeekStart = demo.WeekStart;
                existing.Status = demo.Status;
                await _db.UpdateAsync(existing);
            }
        }

        if (await _db.Table<LocalChapter>().CountAsync() == 0)
            await _db.InsertAllAsync(demos);
    }
}
