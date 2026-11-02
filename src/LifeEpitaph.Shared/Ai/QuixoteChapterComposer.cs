namespace LifeEpitaph.Shared.Ai;

/// <summary>Turns short scraps into a gentle Quixote-style vignette (mock AI).</summary>
public static class QuixoteChapterComposer
{
    public static (string Title, string Body) Compose(DateOnly weekStart, IReadOnlyList<string> scraps)
    {
        var snippets = scraps
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Take(5)
            .ToList();

        if (snippets.Count == 0)
        {
            return (
                "风车还在远处转",
                "这一周几乎没有留下字迹。可空白也不是空：" +
                "钥匙仍在口袋里，像未出鞘的短剑；通勤的门开了又关，像巨兽偶尔眨眼。" +
                "故事愿意等你。等你写下第一句短答，它就会把平凡抬成一场小小的开战。");
        }

        var title = PickTitle(snippets[0], weekStart);
        var body =
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

        return (title, body);
    }

    private static string PickTitle(string first, DateOnly weekStart)
    {
        if (ContainsAny(first, "地铁", "公司", "通勤", "加班"))
            return "唐吉诃德大战风车";
        if (ContainsAny(first, "雨", "伞"))
            return "雨停的那一晚";
        if (ContainsAny(first, "花", "献花"))
            return "献一束看不见的花";
        if (ContainsAny(first, "钥匙", "回家", "门"))
            return "口袋里的信物";
        if (ContainsAny(first, "咖啡", "茶", "晚饭"))
            return "营帐前的灯火";
        if (ContainsAny(first, "想", "心", "哭", "笑"))
            return "写在盾牌内侧";
        return $"第 {weekStart:M/d} 周 · 轻轻开战";
    }

    private static string MetaphorFor(string scrap)
    {
        if (ContainsAny(scrap, "地铁", "公司", "通勤"))
            return "冲向旋转金属风车的一次冲锋——车厢门是盔甲缝隙里的光";
        if (ContainsAny(scrap, "钥匙", "回家"))
            return "藏在暗格里的信物，证明你仍握有归途";
        if (ContainsAny(scrap, "咖啡", "茶", "晚饭"))
            return "营帐前冷却的一盏灯火，提醒骑士也需要休息";
        if (ContainsAny(scrap, "雨", "伞"))
            return "停在城门外的一场洗礼，盔甲湿了，心却轻了一点";
        if (ContainsAny(scrap, "花"))
            return "献到无名石碑旁的一束光，不必署名";
        if (ContainsAny(scrap, "想", "心", "哭", "笑"))
            return "写在盾牌内侧的悄悄话，只给自己看";
        if (ContainsAny(scrap, "夜", "睡", "梦"))
            return "帐篷外未熄的一星火，守着明天继续赶路";
        return "路边一棵普通的树，被你认作了边界上的瞭望塔";
    }

    private static bool ContainsAny(string text, params string[] keys)
        => keys.Any(k => text.Contains(k, StringComparison.Ordinal));
}
