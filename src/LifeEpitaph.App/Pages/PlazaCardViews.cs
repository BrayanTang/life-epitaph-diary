using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.App.Pages;

internal static class PlazaCardViews
{
    // Kept as a tiny helper for any remaining callers; plaza UI now builds cards inline.
    public static IEnumerable<View> BuildDetailViews(PlazaCardDto card, bool preview = false)
    {
        yield return new Label
        {
            Text = string.IsNullOrWhiteSpace(card.Title) ? "分享" : card.Title,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["SoftInk"],
        };
        yield return new Label
        {
            Text = card.Body,
            FontSize = 14,
            TextColor = (Color)Application.Current.Resources["SoftInk"],
        };
    }
}
