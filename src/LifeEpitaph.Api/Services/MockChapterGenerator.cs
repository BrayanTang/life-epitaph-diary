using LifeEpitaph.Shared.Ai;
using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.Api.Services;

/// <summary>
/// Mock weekly chapter: turns short scraps into a gentle Quixote-style vignette.
/// </summary>
public sealed class MockChapterGenerator : IChapterGenerator
{
    public Task<ChapterDto> GenerateAsync(
        Guid userId,
        DateOnly weekStart,
        IReadOnlyList<AnswerDto> answers,
        CancellationToken cancellationToken = default)
    {
        var snippets = answers
            .Where(a => !string.IsNullOrWhiteSpace(a.Body))
            .Select(a => a.Body.Trim())
            .Take(5)
            .ToList();

        string title;
        string body;
        if (snippets.Count == 0)
        {
            title = "风车还在远处转";
            body =
                "这一周几乎没有留下字迹。可空白也不是空：" +
                "钥匙仍在口袋里，像未出鞘的短剑；通勤的门开了又关，像巨兽偶尔眨眼。" +
                "故事愿意等你。等你写下第一句短答，它就会把平凡抬成一场小小的开战。";
        }
        else
        {
            title = PickTitle(snippets[0], weekStart);
            body =
                $"在 {weekStart:yyyy年M月d日} 开始的这一周，你本只是路过人间。" +
                "可叙事不肯放过那些轻轻的痕迹——\n\n";
            for (var i = 0; i < snippets.Count; i++)
            {
                body += $"你说过：「{snippets[i]}」\n";
                body += $"于是故事里，它变成了{MetaphorFor(snippets[i])}。\n\n";
            }

            body +=
                "你没有真的打败风车。你只是抬头了。" +
                "对一个普通的星期来说，这就够当一章。";
        }

        var chapter = new ChapterDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WeekStart = weekStart,
            Title = title,
            Body = body,
            SourceAnswerIds = answers.Select(a => a.Id).ToList(),
            GeneratedAt = DateTimeOffset.UtcNow,
            Status = "ready",
        };

        return Task.FromResult(chapter);
    }

    private static string PickTitle(string first, DateOnly weekStart)
    {
        if (first.Contains("地铁", StringComparison.Ordinal) || first.Contains("公司", StringComparison.Ordinal))
            return "唐吉诃德大战风车";
        if (first.Contains("雨", StringComparison.Ordinal))
            return "雨停的那一晚";
        if (first.Contains("花", StringComparison.Ordinal))
            return "献一束看不见的花";
        return $"第 {weekStart:M/d} 周 · 轻轻开战";
    }

    private static string MetaphorFor(string scrap)
    {
        if (scrap.Contains("地铁", StringComparison.Ordinal))
            return "冲向旋转金属风车的一次冲锋——车厢门是盔甲缝隙里的光";
        if (scrap.Contains("钥匙", StringComparison.Ordinal))
            return "藏在暗格里的信物，证明你仍握有归途";
        if (scrap.Contains("咖啡", StringComparison.Ordinal) || scrap.Contains("茶", StringComparison.Ordinal))
            return "营帐前冷却的一盏灯火，提醒骑士也需要休息";
        if (scrap.Contains("想", StringComparison.Ordinal) || scrap.Contains("心", StringComparison.Ordinal))
            return "写在盾牌内侧的悄悄话，只给自己看";
        return "路边一棵普通的树，被你认作了边界上的瞭望塔";
    }
}
