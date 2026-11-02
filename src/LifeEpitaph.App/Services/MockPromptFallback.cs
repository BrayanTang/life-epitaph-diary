using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.App.Services;

/// <summary>Client-side question bank when API is unreachable.</summary>
public static class MockPromptFallback
{
    private static readonly string[] Bank =
    [
        "今天有哪一刻，让你觉得稍微轻松了一点？",
        "如果给今天取一个温柔的标题，你会叫它什么？",
        "有没有一件小事，你想悄悄记下来，不必给别人看？",
        "此刻的你，更像是在等待，还是在慢慢走？",
        "今天有没有想对自己说一句「没事的」？",
        "哪一句没说出口的话，在心里待了很久？",
        "如果墓碑旁放一朵花，你会为今天的自己献什么颜色？",
        "今天的天气像你的心情吗？哪里像，哪里不像？",
        "有没有一个瞬间，你突然想起某个人或某个地方？",
        "睡前如果只留一句话给明天的自己，会是什么？",
        "通勤路上，你把什么想象成了要挑战的「风车」？",
        "口袋里的钥匙，今天更像武器，还是更像回家的许可？",
        "有没有一件极小的事，值得被夸张成一场冒险？",
        "如果给今天的自己封一个温柔的骑士名，会是什么？",
        "今天哪一步，其实已经算你抬头了？",
        "晚饭的味道，像不像某种胜利后的犒赏？",
        "夜里还醒着的时候，你在守护什么，又在放过什么？",
        "若只许保留一个今天的画面，你会留下哪一帧？",
    ];

    public static DailyPromptDto ForToday()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        return new DailyPromptDto
        {
            Id = Guid.Parse($"{date.Year:D4}{date.Month:D2}{date.Day:D2}-0000-4000-8000-000000000001"),
            Date = date,
            Text = Bank[Math.Abs(date.DayNumber) % Bank.Length],
            Locale = "zh-CN",
        };
    }
}
