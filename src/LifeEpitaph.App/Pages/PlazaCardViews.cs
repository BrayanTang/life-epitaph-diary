using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.App.Pages;

internal static class PlazaCardViews
{
    public static IEnumerable<View> BuildDetailViews(PlazaCardDto card, bool preview = false)
    {
        var soft = (Color)Application.Current!.Resources["SoftInk"];
        var muted = (Color)Application.Current.Resources["MutedInk"];

        if (card.IsMine || preview)
        {
            yield return new Label
            {
                Text = preview ? "（预览）按勾选项展示" : "我的墓碑 · 已分享",
                FontSize = 12,
                TextColor = (Color)Application.Current.Resources["AccentMoss"],
            };
        }

        if (!string.IsNullOrWhiteSpace(card.DisplayName) || !string.IsNullOrWhiteSpace(card.Status))
        {
            var title = string.IsNullOrWhiteSpace(card.Status)
                ? card.DisplayName
                : string.IsNullOrWhiteSpace(card.DisplayName)
                    ? card.Status
                    : $"{card.DisplayName}  ·  {card.Status}";
            yield return new Label
            {
                Text = title,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = soft,
            };
        }

        if (!string.IsNullOrWhiteSpace(card.Epitaph))
        {
            yield return new Label
            {
                Text = $"「{card.Epitaph}」",
                FontSize = 15,
                LineHeight = 1.35,
                TextColor = soft,
            };
        }

        if (!string.IsNullOrWhiteSpace(card.SkinId))
        {
            yield return new Label
            {
                Text = $"皮肤  {card.SkinId}",
                FontSize = 13,
                TextColor = muted,
            };
        }

        if (!string.IsNullOrWhiteSpace(card.MoodSnippet))
        {
            yield return new Label
            {
                Text = $"此刻  {card.MoodSnippet}",
                FontSize = 14,
                TextColor = soft,
            };
        }

        if (card.RecentChapterTitles.Count > 0)
        {
            yield return new Label
            {
                Text = "近章标题",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = soft,
                Margin = new Thickness(0, 4, 0, 0),
            };
            foreach (var title in card.RecentChapterTitles)
            {
                yield return new Label
                {
                    Text = $"· {title}",
                    FontSize = 13,
                    TextColor = muted,
                };
            }
        }

        if (card.SharedLabels.Count > 0)
        {
            yield return new Label
            {
                Text = $"公开字段：{string.Join(" · ", card.SharedLabels)}",
                FontSize = 11,
                TextColor = muted,
                Margin = new Thickness(0, 4, 0, 0),
            };
        }

        if (!preview)
        {
            yield return new Label
            {
                Text = $"献花  {card.FlowerCount}",
                FontSize = 12,
                TextColor = muted,
            };
        }
    }
}
