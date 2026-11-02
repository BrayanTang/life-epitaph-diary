using LifeEpitaph.App.Models;
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
            await _db.CreateTableAsync<LocalSession>();

            var tombstone = await _db.Table<LocalTombstone>().FirstOrDefaultAsync();
            if (tombstone is null)
            {
                await _db.InsertAsync(new LocalTombstone());
            }

            if (await _db.Table<LocalChapter>().CountAsync() == 0)
            {
                await _db.InsertAllAsync(new[]
                {
                    new LocalChapter
                    {
                        WeekStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-14)).ToString("yyyy-MM-dd"),
                        Title = "序章 · 一块安静的石头",
                        Body = "（占位）建墓之后的第一周，故事还很轻。",
                        Status = "ready",
                    },
                    new LocalChapter
                    {
                        WeekStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-7)).ToString("yyyy-MM-dd"),
                        Title = "短答慢慢叠成线",
                        Body = "（占位）本周章节将由 Mock / 真实 AI 生成。",
                        Status = "draft",
                    },
                });
            }

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
}
