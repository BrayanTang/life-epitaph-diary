namespace LifeEpitaph.Api.Data;

/// <summary>Static Chinese question bank used when no live AI provider is configured.</summary>
public static class PromptBank
{
    public static readonly IReadOnlyList<string> ZhCn =
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
        "今天你有没有对自己诚实一点点？",
        "若把今天写成一页空白日记，你会先写下哪个词？",
        "有没有一份安静，是你今天真正需要的？",
        "如果生活是一出剧，今天是哪一幕的开场？",
        "此刻你最想被看见的，是哪一面？",
        "通勤路上，你把什么想象成了要挑战的「风车」？",
        "口袋里的钥匙，今天更像武器，还是更像回家的许可？",
        "有没有一件极小的事，值得被夸张成一场冒险？",
        "如果给今天的自己封一个温柔的骑士名，会是什么？",
        "今天哪一步，其实已经算你抬头了？",
    ];

    public static string PickForDate(DateOnly date, string locale = "zh-CN")
    {
        var bank = ZhCn;
        var index = Math.Abs(date.DayNumber) % bank.Count;
        return bank[index];
    }
}
