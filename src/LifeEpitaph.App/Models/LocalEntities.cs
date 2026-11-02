using SQLite;

namespace LifeEpitaph.App.Models;

public sealed class LocalTombstone
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string DisplayName { get; set; } = "无名之石";
    public string Epitaph { get; set; } = "在这里，安静地记下一周的自己。";
    public string Status { get; set; } = "安静";
    public string SkinId { get; set; } = "soft-stone";
    public int IsListedPublic { get; set; }
}

public sealed class LocalAnswer
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Indexed(Name = "IX_Answer_Day", Unique = true)]
    public string AnsweredOn { get; set; } = string.Empty; // yyyy-MM-dd

    public string PromptId { get; set; } = string.Empty;
    public string PromptText { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public sealed class LocalChapter
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string WeekStart { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
}

/// <summary>User-written enrichment layered onto a weekly chapter story.</summary>
public sealed class LocalChapterEnrichment
{
    [PrimaryKey]
    public string ChapterId { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>Free-form private journal scrap — not tied to the daily prompt.</summary>
public sealed class LocalJournalEntry
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Body { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public sealed class LocalSession
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    public string DeviceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

/// <summary>A journal scrap or chapter the user shared to plaza.</summary>
public sealed class LocalPlazaPost
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>journal | chapter</summary>
    public string Kind { get; set; } = "journal";
    public string SourceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int FlowerCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
